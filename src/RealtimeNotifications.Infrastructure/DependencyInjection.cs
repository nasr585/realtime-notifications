using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Infrastructure.Persistence;
using RealtimeNotifications.Infrastructure.Realtime;

namespace RealtimeNotifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<NotificationsDbContext>());

        services.AddSignalR();
        services.AddSingleton<INotificationBroadcaster, SignalRNotificationBroadcaster>();

        return services;
    }
}
