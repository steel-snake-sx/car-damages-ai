namespace CarDamageClaims.Api.Services.Email;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? HtmlBody { get; set; }

    public string? TextBody { get; set; }
}
