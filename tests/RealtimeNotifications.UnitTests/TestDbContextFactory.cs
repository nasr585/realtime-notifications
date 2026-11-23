using Microsoft.EntityFrameworkCore;
using RealtimeNotifications.Infrastructure.Persistence;

namespace RealtimeNotifications.UnitTests;

public static class TestDbContextFactory
{
    public static NotificationsDbContext Create()
    {
        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NotificationsDbContext(options);
    }
}
