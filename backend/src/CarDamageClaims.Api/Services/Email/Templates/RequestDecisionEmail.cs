using CarDamageClaims.Api.Models;
using CarDamageClaims.Api.Services.Email;

namespace CarDamageClaims.Api.Services.Email.Templates;

public static class RequestDecisionEmail
{
    public static EmailMessage BuildApprovalMessage(DamageRequest request)
    {
        var subject = EmailTemplateFormatting.BuildCarResultSubject(request);
        var textBody = RequestDecisionText.BuildApprovalText(request);
        var htmlBody = RequestDecisionHtml.BuildApprovalHtml(request);

        return new EmailMessage
        {
            To = request.Email,
            Subject = subject,
            Body = textBody,
            TextBody = textBody,
            HtmlBody = htmlBody,
        };
    }

    public static EmailMessage BuildRejectionMessage(DamageRequest request)
    {
        var subject = EmailTemplateFormatting.BuildCarResultSubject(request);
        var textBody = RequestDecisionText.BuildRejectionText(request);
        var htmlBody = RequestDecisionHtml.BuildRejectionHtml(request);

        return new EmailMessage
        {
            To = request.Email,
            Subject = subject,
            Body = textBody,
            TextBody = textBody,
            HtmlBody = htmlBody,
        };
    }
}
