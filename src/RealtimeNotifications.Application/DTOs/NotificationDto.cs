using RealtimeNotifications.Domain.Entities;

namespace RealtimeNotifications.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string Type,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc)
{
    public static NotificationDto FromEntity(Notification notification) =>
        new(
            notification.Id,
            notification.Title,
            notification.Body,
            notification.Type.ToString(),
            notification.CreatedAtUtc,
            notification.ReadAtUtc);
}
