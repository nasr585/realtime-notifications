using FluentAssertions;
using NSubstitute;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Application.DTOs;
using RealtimeNotifications.Application.Features.Notifications;
using RealtimeNotifications.Domain.Enums;
using Xunit;

namespace RealtimeNotifications.UnitTests.Application;

public class SendNotificationHandlerTests
{
    [Fact]
    public async Task Valid_command_persists_and_broadcasts_to_the_recipient_group()
    {
        await using var context = TestDbContextFactory.Create();
        var broadcaster = Substitute.For<INotificationBroadcaster>();
        var handler = new SendNotificationHandler(context, broadcaster, new SendNotificationValidator());

        var dto = await handler.Handle(
            new SendNotificationCommand("user-1", "Build finished", "Commit abc123", NotificationType.Success),
            CancellationToken.None);

        context.Notifications.Single().Id.Should().Be(dto.Id);
        dto.Type.Should().Be("Success");

        await broadcaster.Received(1).SendToUserAsync(
            "user-1",
            Arg.Is<NotificationDto>(n => n.Id == dto.Id && n.Title == "Build finished"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invalid_command_persists_nothing_and_never_broadcasts()
    {
        await using var context = TestDbContextFactory.Create();
        var broadcaster = Substitute.For<INotificationBroadcaster>();
        var handler = new SendNotificationHandler(context, broadcaster, new SendNotificationValidator());

        var act = () => handler.Handle(
            new SendNotificationCommand("user-1", "", "Body", NotificationType.Info),
            CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        context.Notifications.Should().BeEmpty();
        await broadcaster.DidNotReceiveWithAnyArgs()
            .SendToUserAsync(default!, default!, default);
    }
}
