using Microsoft.EntityFrameworkCore;
using RealtimeNotifications.Application.Abstractions;
using RealtimeNotifications.Domain.Entities;

namespace RealtimeNotifications.Infrastructure.Persistence;

public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}
