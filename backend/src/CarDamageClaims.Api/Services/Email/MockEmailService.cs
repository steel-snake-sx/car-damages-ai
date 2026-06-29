namespace CarDamageClaims.Api.Services.Email;

public class MockEmailService : IEmailService
{
    public Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var isValid =
            !string.IsNullOrWhiteSpace(message.To)
            && !string.IsNullOrWhiteSpace(message.Subject)
            && !string.IsNullOrWhiteSpace(message.Body);

        var result = new EmailSendResult
        {
            Success = isValid,
            ErrorMessage = isValid ? null : "Mock email validation failed.",
            OccurredAtUtc = DateTime.UtcNow,
        };

        return Task.FromResult(result);
    }
}
