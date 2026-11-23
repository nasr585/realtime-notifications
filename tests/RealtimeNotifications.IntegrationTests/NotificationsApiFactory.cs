using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtimeNotifications.Infrastructure.Persistence;

namespace RealtimeNotifications.IntegrationTests;

/// <summary>
/// Boots the real API against an in-memory database so each test run
/// exercises the full HTTP and SignalR pipeline without needing SQL Server.
/// </summary>
public class NotificationsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"RealtimeNotificationsTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove every Entity Framework registration the app made for SQL Server,
            // otherwise EF would see two providers and each request could resolve a
            // different underlying store.
            var efDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<NotificationsDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(NotificationsDbContext))
                .ToList();
            foreach (var descriptor in efDescriptors)
            {
                services.Remove(descriptor);
            }

            // Give the in-memory provider its own internal service provider, so the
            // store is shared by every request scope of this factory.
            var inMemoryRoot = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<NotificationsDbContext>(options =>
                options
                    .UseInMemoryDatabase(_databaseName)
                    .UseInternalServiceProvider(inMemoryRoot));
        });
    }
}
