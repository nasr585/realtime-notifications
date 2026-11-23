using FluentValidation;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Application.DTOs;
using RealtimeNotifications.Domain.Entities;
using RealtimeNotifications.Domain.Enums;

namespace RealtimeNotifications.Application.Features.Notifications;

public record SendNotificationCommand(string RecipientUserId, string Title, string Body, NotificationType Type);

public class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.RecipientUserId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Notification.MaxTitleLength);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(Notification.MaxBodyLength);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public class SendNotificationHandler(
    IApplicationDbContext context,
    INotificationBroadcaster broadcaster,
    IValidator<SendNotificationCommand> validator)
{
    public async Task<NotificationDto> Handle(SendNotificationCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var notification = new Notification(command.RecipientUserId, command.Title, command.Body, command.Type);

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);

        var dto = NotificationDto.FromEntity(notification);
        await broadcaster.SendToUserAsync(notification.RecipientUserId, dto, cancellationToken);

        return dto;
    }
}
