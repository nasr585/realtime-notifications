using Microsoft.EntityFrameworkCore;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Application.Common;
using RealtimeNotifications.Application.DTOs;

namespace RealtimeNotifications.Application.Features.Notifications;

public record GetUserNotificationsQuery(string UserId, bool UnreadOnly = false, int Page = 1, int PageSize = 20);

public class GetUserNotificationsHandler(IApplicationDbContext context)
{
    public async Task<PagedResult<NotificationDto>> Handle(GetUserNotificationsQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var filtered = context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == query.UserId);

        if (query.UnreadOnly)
        {
            filtered = filtered.Where(n => n.ReadAtUtc == null);
        }

        var ordered = filtered.OrderByDescending(n => n.CreatedAtUtc);
        var totalCount = await ordered.CountAsync(cancellationToken);

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>(
            items.Select(NotificationDto.FromEntity).ToList(),
            page,
            pageSize,
            totalCount);
    }
}
