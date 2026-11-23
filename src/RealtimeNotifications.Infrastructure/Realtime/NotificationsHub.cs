using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RealtimeNotifications.Infrastructure.Realtime;

[Authorize]
public class NotificationsHub : Hub
{
    /// <summary>Client-side callback name used when a notification is pushed.</summary>
    public const string ClientMethod = "notificationReceived";

    public static string UserGroup(string userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Every connection joins the group of its authenticated user, so the
            // broadcaster can reach "this user, on every open tab or device".
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        await base.OnConnectedAsync();
    }
}
