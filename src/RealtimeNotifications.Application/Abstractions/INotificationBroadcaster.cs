using RealtimeNotifications.Application.DTOs;

namespace RealtimeNotifications.Application.Abstractions;

/// <summary>
/// Pushes a notification to the recipient's live connections, when any exist.
/// Implemented over SignalR in the infrastructure layer.
/// </summary>
public interface INotificationBroadcaster
{
    Task SendToUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken);
}
