using RealtimeNotifications.Domain.Common;
using RealtimeNotifications.Domain.Enums;

namespace RealtimeNotifications.Domain.Entities;

public class Notification : Entity
{
    public const int MaxTitleLength = 200;
    public const int MaxBodyLength = 2000;

    public string RecipientUserId { get; private set; } = default!;

    public string Title { get; private set; } = default!;

    public string Body { get; private set; } = default!;

    public NotificationType Type { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    public bool IsRead => ReadAtUtc is not null;

    private Notification()
    {
    }

    public Notification(string recipientUserId, string title, string body, NotificationType type)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId))
        {
            throw new DomainException("A notification needs a recipient.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("A notification needs a title.");
        }

        if (title.Length > MaxTitleLength)
        {
            throw new DomainException($"A notification title is limited to {MaxTitleLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("A notification needs a body.");
        }

        if (body.Length > MaxBodyLength)
        {
            throw new DomainException($"A notification body is limited to {MaxBodyLength} characters.");
        }

        RecipientUserId = recipientUserId.Trim();
        Title = title.Trim();
        Body = body.Trim();
        Type = type;
    }

    public void MarkRead()
    {
        ReadAtUtc ??= DateTime.UtcNow;
    }
}
