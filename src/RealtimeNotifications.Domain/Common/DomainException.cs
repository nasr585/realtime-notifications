namespace RealtimeNotifications.Domain.Common;

/// <summary>
/// Raised when a domain invariant is violated. Mapped to a 400 response at the API boundary.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
