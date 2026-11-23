using Microsoft.EntityFrameworkCore;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Application.Common;

namespace RealtimeNotifications.Application.Features.Notifications;

public record MarkNotificationReadCommand(Guid NotificationId, string UserId);

public class MarkNotificationReadHandler(IApplicationDbContext context)
{
    public async Task Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        // A user can only mark their own notifications read.
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId && n.RecipientUserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException("Notification", command.NotificationId);

        notification.MarkRead();
        await context.SaveChangesAsync(cancellationToken);
    }
}
