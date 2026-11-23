using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealtimeNotifications.Application.Common;
using RealtimeNotifications.Application.DTOs;
using RealtimeNotifications.Application.Features.Notifications;

namespace RealtimeNotifications.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<NotificationDto>> GetMine(
        [FromServices] GetUserNotificationsHandler handler,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await handler.Handle(
            new GetUserNotificationsQuery(CurrentUserId(), unreadOnly, page, pageSize),
            cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> Send(
        [FromBody] SendNotificationCommand command,
        [FromServices] SendNotificationHandler handler,
        CancellationToken cancellationToken)
    {
        var notification = await handler.Handle(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, notification);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(
        Guid id,
        [FromServices] MarkNotificationReadHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new MarkNotificationReadCommand(id, CurrentUserId()), cancellationToken);
        return NoContent();
    }

    private string CurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("The authenticated token carries no user id claim.");
}
