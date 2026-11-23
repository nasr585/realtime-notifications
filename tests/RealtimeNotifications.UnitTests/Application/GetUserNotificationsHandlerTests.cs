using FluentAssertions;
using RealtimeNotifications.Application.Features.Notifications;
using RealtimeNotifications.Domain.Entities;
using RealtimeNotifications.Domain.Enums;
using Xunit;

namespace RealtimeNotifications.UnitTests.Application;

public class GetUserNotificationsHandlerTests
{
    [Fact]
    public async Task Returns_only_the_requested_users_notifications_paged()
    {
        await using var context = TestDbContextFactory.Create();
        for (var i = 0; i < 30; i++)
        {
            context.Notifications.Add(new Notification("user-1", $"Title {i}", "Body", NotificationType.Info));
        }

        context.Notifications.Add(new Notification("user-2", "Other", "Body", NotificationType.Info));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetUserNotificationsHandler(context);

        var page = await handler.Handle(
            new GetUserNotificationsQuery("user-1", Page: 2, PageSize: 20),
            CancellationToken.None);

        page.TotalCount.Should().Be(30);
        page.TotalPages.Should().Be(2);
        page.Items.Should().HaveCount(10);
        page.Items.Should().OnlyContain(n => n.Title.StartsWith("Title"));
    }

    [Fact]
    public async Task UnreadOnly_filters_out_read_notifications()
    {
        await using var context = TestDbContextFactory.Create();
        var read = new Notification("user-1", "Read", "Body", NotificationType.Info);
        read.MarkRead();
        context.Notifications.Add(read);
        context.Notifications.Add(new Notification("user-1", "Unread", "Body", NotificationType.Info));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetUserNotificationsHandler(context);

        var result = await handler.Handle(
            new GetUserNotificationsQuery("user-1", UnreadOnly: true),
            CancellationToken.None);

        result.Items.Should().ContainSingle(n => n.Title == "Unread");
    }
}
