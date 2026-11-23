using FluentAssertions;
using RealtimeNotifications.Application.Common;
using RealtimeNotifications.Application.Features.Notifications;
using RealtimeNotifications.Domain.Entities;
using RealtimeNotifications.Domain.Enums;
using Xunit;

namespace RealtimeNotifications.UnitTests.Application;

public class MarkNotificationReadHandlerTests
{
    [Fact]
    public async Task Marks_the_recipients_notification_read()
    {
        await using var context = TestDbContextFactory.Create();
        var notification = new Notification("user-1", "Title", "Body", NotificationType.Info);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new MarkNotificationReadHandler(context);
        await handler.Handle(new MarkNotificationReadCommand(notification.Id, "user-1"), CancellationToken.None);

        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Another_users_notification_is_not_found()
    {
        await using var context = TestDbContextFactory.Create();
        var notification = new Notification("user-1", "Title", "Body", NotificationType.Info);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new MarkNotificationReadHandler(context);
        var act = () => handler.Handle(new MarkNotificationReadCommand(notification.Id, "user-2"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        notification.IsRead.Should().BeFalse();
    }
}
