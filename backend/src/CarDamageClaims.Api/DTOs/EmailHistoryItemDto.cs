namespace CarDamageClaims.Api.DTOs;

public class EmailHistoryItemDto
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public string FullName { get; set; } = string.Empty;
}
