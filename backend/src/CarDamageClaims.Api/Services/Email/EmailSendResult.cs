namespace CarDamageClaims.Api.Services.Email;

public class EmailSendResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}
