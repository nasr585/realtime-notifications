# Realtime Notifications

A small notification service: a REST API for sending and listing notifications, with delivery to connected clients in real time over SignalR.

This is a portfolio project. It is not used in production and has no external clients; it exists to demonstrate JWT-authenticated SignalR work on ASP.NET Core.

## What it demonstrates

- SignalR hub with JWT bearer authentication
- Per-user groups: every connection of the same user receives their notifications, on any tab or device
- REST + realtime working together: a POST stores the notification and pushes it to live connections in the same operation
- A JWT-secured API (with a demo token issuer so the sample is easy to run)
- An end-to-end test that connects a real SignalR client and asserts the push arrives

## Tech stack

- .NET 8 / ASP.NET Core Web API + SignalR
- Entity Framework Core 8 with SQL Server
- JWT bearer authentication
- xUnit + FluentAssertions for tests
- Docker / Docker Compose

## Architecture

```
src/
  RealtimeNotifications.Domain          Notification entity and its rules
  RealtimeNotifications.Application     Use cases, DTOs, validation, broadcaster abstraction
  RealtimeNotifications.Infrastructure  EF Core, SQL Server, SignalR hub and broadcaster
  RealtimeNotifications.Api             Controllers, JWT setup, demo token issuer, demo client
tests/
  RealtimeNotifications.UnitTests        Domain rules and application handlers
  RealtimeNotifications.IntegrationTests HTTP pipeline plus a live SignalR end-to-end test
```

The application layer depends on an `INotificationBroadcaster` abstraction; the infrastructure layer implements it over SignalR, so the use cases stay independent of the delivery mechanism.

## Run it with Docker

```
docker compose up --build
```

Open http://localhost:5000 for a minimal demo client: enter any user id, connect, then send yourself a notification and watch it arrive without a refresh. Swagger UI is at `/swagger`.

## Run it locally

You need the .NET 8 SDK and a SQL Server instance. Set the connection string in `src/RealtimeNotifications.Api/appsettings.Development.json`, then:

```
dotnet ef database update --project src/RealtimeNotifications.Infrastructure --startup-project src/RealtimeNotifications.Api
dotnet run --project src/RealtimeNotifications.Api
```

## API sketch

```
POST /api/auth/demo-token          { "userId": "demo-user" }        -> { "token": "..." }
POST /api/notifications            (Bearer) send a notification to a user id
GET  /api/notifications            (Bearer) list your own notifications (paged, unreadOnly filter)
POST /api/notifications/{id}/read  (Bearer) mark one of yours as read
WS   /hubs/notifications           (Bearer or ?access_token=) realtime feed, event "notificationReceived"
```

The demo token endpoint is intentionally simple and exists only so the sample runs without an identity provider. The signing key in `appsettings.json` is a development default, not a real secret.

## Tests

```
dotnet test
```

Alongside unit and API tests, one integration test connects an actual `HubConnection` to the in-memory server, sends a notification over REST, and asserts the client receives it through the hub.

## Notes and limitations

- The demo token issuer trusts whatever user id it is given; a real deployment would plug in an identity provider instead.
- A multi-instance deployment would need a SignalR backplane (for example Redis or Azure SignalR Service) so a notification reaches users connected to other instances.
