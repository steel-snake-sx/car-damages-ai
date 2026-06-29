namespace CarDamageClaims.Api.Services.Email;

public interface IEmailService
{
    Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    );
}
