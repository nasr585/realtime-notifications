using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace RealtimeNotifications.IntegrationTests;

public class NotificationsApiTests : IClassFixture<NotificationsApiFactory>
{
    private readonly NotificationsApiFactory _factory;
    private readonly HttpClient _client;

    public NotificationsApiTests(NotificationsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Unauthenticated_requests_are_rejected()
    {
        var response = await _client.GetAsync("/api/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Send_then_list_returns_the_notification()
    {
        var token = await GetDemoToken("user-1");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
        {
            Content = JsonContent.Create(new
            {
                recipientUserId = "user-1",
                title = "Build finished",
                body = "Commit abc123",
                type = 2
            })
        };
        request.Headers.Authorization = new("Bearer", token);

        var send = await _client.SendAsync(request);
        send.StatusCode.Should().Be(HttpStatusCode.Created);

        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/notifications");
        listRequest.Headers.Authorization = new("Bearer", token);
        var list = await _client.SendAsync(listRequest);
        var body = await list.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("totalCount").GetInt32().Should().Be(1);
        body.GetProperty("items")[0].GetProperty("title").GetString().Should().Be("Build finished");
    }

    [Fact]
    public async Task SignalR_client_receives_the_notification_in_real_time()
    {
        var token = await GetDemoToken("signalr-user");

        await using var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/notifications", options =>
            {
                // WebSockets are not available on the test server; long polling is.
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();

        var received = new TaskCompletionSource<JsonElement>();
        connection.On<JsonElement>("notificationReceived", notification => received.TrySetResult(notification));

        await connection.StartAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
        {
            Content = JsonContent.Create(new
            {
                recipientUserId = "signalr-user",
                title = "Deployment done",
                body = "v1.2.0 is live",
                type = 2
            })
        };
        request.Headers.Authorization = new("Bearer", token);
        var send = await _client.SendAsync(request);
        send.StatusCode.Should().Be(HttpStatusCode.Created);

        var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        completed.Should().Be(received.Task, "the hub should push the notification to the connected client");
        received.Task.Result.GetProperty("title").GetString().Should().Be("Deployment done");
    }

    [Fact]
    public async Task Marking_another_users_notification_read_returns_404()
    {
        var ownerToken = await GetDemoToken("owner");
        var strangerToken = await GetDemoToken("stranger");

        var sendRequest = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
        {
            Content = JsonContent.Create(new
            {
                recipientUserId = "owner",
                title = "Private",
                body = "For the owner only",
                type = 1
            })
        };
        sendRequest.Headers.Authorization = new("Bearer", ownerToken);
        var send = await _client.SendAsync(sendRequest);
        var created = await send.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var markRead = new HttpRequestMessage(HttpMethod.Post, $"/api/notifications/{id}/read");
        markRead.Headers.Authorization = new("Bearer", strangerToken);
        var response = await _client.SendAsync(markRead);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> GetDemoToken(string userId)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/demo-token", new { userId });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }
}
