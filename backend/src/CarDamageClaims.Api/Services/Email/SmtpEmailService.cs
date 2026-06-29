using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CarDamageClaims.Api.Services.Email;

public class SmtpEmailService(IOptions<EmailOptions> options) : IEmailService
{
    private readonly EmailOptions emailOptions = options.Value;

    public async Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        var occurredAt = DateTime.UtcNow;

        if (!IsValid(message))
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "Email message is invalid.",
                OccurredAtUtc = occurredAt,
            };
        }

        if (!IsConfigured())
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "SMTP is not configured.",
                OccurredAtUtc = occurredAt,
            };
        }

        try
        {
            var mimeMessage = BuildMimeMessage(message);
            using var smtpClient = new SmtpClient();

            var secureSocketOptions = ResolveSecurityMode(emailOptions.Port);
            await smtpClient.ConnectAsync(
                emailOptions.Host,
                emailOptions.Port,
                secureSocketOptions,
                cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(emailOptions.Username))
            {
                await smtpClient.AuthenticateAsync(
                    emailOptions.Username,
                    emailOptions.Password,
                    cancellationToken
                );
            }

            await smtpClient.SendAsync(mimeMessage, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            return new EmailSendResult { Success = true, OccurredAtUtc = DateTime.UtcNow };
        }
        catch (Exception ex)
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                OccurredAtUtc = DateTime.UtcNow,
            };
        }
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(emailOptions.Host)
            && emailOptions.Port > 0
            && !string.IsNullOrWhiteSpace(emailOptions.Username)
            && !string.IsNullOrWhiteSpace(emailOptions.Password);
    }

    private static bool IsValid(EmailMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.To) || string.IsNullOrWhiteSpace(message.Subject))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(message.Body)
            || !string.IsNullOrWhiteSpace(message.TextBody)
            || !string.IsNullOrWhiteSpace(message.HtmlBody);
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();

        var fromEmail = string.IsNullOrWhiteSpace(emailOptions.FromEmail)
            ? emailOptions.Username
            : emailOptions.FromEmail;

        mimeMessage.From.Add(new MailboxAddress(emailOptions.FromName, fromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;

        var textBody = SelectTextBody(message);
        var htmlBody = SelectHtmlBody(message);

        var bodyBuilder = new BodyBuilder { TextBody = textBody, HtmlBody = htmlBody };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        return mimeMessage;
    }

    private static string SelectTextBody(EmailMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            return message.TextBody;
        }

        return message.Body;
    }

    private static string? SelectHtmlBody(EmailMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            return message.HtmlBody;
        }

        return null;
    }

    private static SecureSocketOptions ResolveSecurityMode(int port)
    {
        return port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
    }
}
