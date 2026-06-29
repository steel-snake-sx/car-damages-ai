namespace CarDamageClaims.Api.Models;

public class NotificationOutbox
{
    public Guid Id { get; set; }

    public Guid DamageRequestId { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public NotificationType NotificationType { get; set; }

    public NotificationStatus Status { get; set; }

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DamageRequest DamageRequest { get; set; } = null!;
}
