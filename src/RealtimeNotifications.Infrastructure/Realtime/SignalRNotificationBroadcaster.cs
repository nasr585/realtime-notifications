using Microsoft.AspNetCore.SignalR;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Application.DTOs;

namespace RealtimeNotifications.Infrastructure.Realtime;

public class SignalRNotificationBroadcaster(IHubContext<NotificationsHub> hubContext) : INotificationBroadcaster
{
    public Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(NotificationsHub.UserGroup(userId))
            .SendAsync(NotificationsHub.ClientMethod, notification, cancellationToken);
}
