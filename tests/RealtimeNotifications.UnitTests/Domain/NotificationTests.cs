using FluentAssertions;
using RealtimeNotifications.Domain.Common;
using RealtimeNotifications.Domain.Entities;
using RealtimeNotifications.Domain.Enums;
using Xunit;

namespace RealtimeNotifications.UnitTests.Domain;

public class NotificationTests
{
    [Fact]
    public void Valid_notification_is_unread_and_trimmed()
    {
        var notification = new Notification(" user-1 ", " Hello ", " Body ", NotificationType.Info);

        notification.RecipientUserId.Should().Be("user-1");
        notification.Title.Should().Be("Hello");
        notification.IsRead.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_recipient_throws(string? recipient)
    {
        var act = () => new Notification(recipient!, "Title", "Body", NotificationType.Info);

        act.Should().Throw<DomainException>().WithMessage("*recipient*");
    }

    [Fact]
    public void Overlong_title_throws()
    {
        var title = new string('x', Notification.MaxTitleLength + 1);

        var act = () => new Notification("user-1", title, "Body", NotificationType.Info);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkRead_is_idempotent()
    {
        var notification = new Notification("user-1", "Title", "Body", NotificationType.Info);

        notification.MarkRead();
        var first = notification.ReadAtUtc;
        notification.MarkRead();

        notification.IsRead.Should().BeTrue();
        notification.ReadAtUtc.Should().Be(first);
    }
}
