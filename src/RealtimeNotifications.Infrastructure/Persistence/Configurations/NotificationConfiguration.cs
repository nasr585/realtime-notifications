using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealtimeNotifications.Domain.Entities;

namespace RealtimeNotifications.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientUserId).HasMaxLength(100).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(Notification.MaxTitleLength).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(Notification.MaxBodyLength).IsRequired();
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(n => new { n.RecipientUserId, n.CreatedAtUtc });
    }
}
