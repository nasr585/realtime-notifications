using Microsoft.EntityFrameworkCore;
using RealtimeNotifications.Domain.Entities;

namespace RealtimeNotifications.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
