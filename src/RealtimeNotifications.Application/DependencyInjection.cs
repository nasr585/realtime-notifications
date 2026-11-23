using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RealtimeNotifications.Application.Features.Notifications;

namespace RealtimeNotifications.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<SendNotificationHandler>();
        services.AddScoped<GetUserNotificationsHandler>();
        services.AddScoped<MarkNotificationReadHandler>();

        return services;
    }
}
