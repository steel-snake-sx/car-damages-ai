using System.Text;
using CarDamageClaims.Api.Models;

namespace CarDamageClaims.Api.Services.Email.Templates;

internal static class RequestDecisionHtml
{
    private const string ProjectName = "Car Damage Claims AI";
    private const string AccentColor = "#6e9eeb";
    private const string BackgroundColor = "#111214";
    private const string CardColor = "#242526";
    private const string BorderColor = "#33383d";
    private const string MainTextColor = "#e6e6e6";
    private const string MutedTextColor = "#a8a8a8";

    internal static string BuildApprovalHtml(DamageRequest request)
    {
        var summaryText = EmailTemplateFormatting.Escape(request.AiSummary);
        var statusText = "Одобрена";
        var intro =
            $"По вашему автомобилю {EmailTemplateFormatting.Escape(request.CarBrand)} {EmailTemplateFormatting.Escape(request.CarModel)} {request.CarYear} сформирован итоговый вердикт.";

        var content = new StringBuilder();
        content.AppendLine(
            $"<p style=\"margin:0 0 14px 0;color:{MainTextColor};font-size:17px;line-height:1.5;\">Здравствуйте, {EmailTemplateFormatting.Escape(EmailTemplateFormatting.BuildGreetingName(request))}</p>"
        );
        content.AppendLine(
            $"<p style=\"margin:0 0 22px 0;color:{MutedTextColor};font-size:15px;line-height:1.6;\">{intro}</p>"
        );
        content.AppendLine(BuildMetaBlock(request, statusText));
        content.AppendLine(
            $"<div style=\"background:#1a2434;border:1px solid #2a415f;border-radius:14px;padding:16px 18px;margin:0 0 20px 0;\"><p style=\"margin:0 0 8px 0;color:{AccentColor};font-weight:700;font-size:13px;letter-spacing:0.02em;text-transform:uppercase;\">AI резюме</p><p style=\"margin:0;color:{MainTextColor};font-size:14px;line-height:1.6;\">{summaryText}</p></div>"
        );
        content.AppendLine(BuildEstimateTable(request.EstimateItems));
        content.AppendLine(
            $"<div style=\"margin-top:16px;border-top:1px solid {BorderColor};padding-top:16px;\"><p style=\"margin:0;color:{MainTextColor};font-size:15px;\">Итоговая стоимость: <span style=\"color:{AccentColor};font-weight:700;font-size:20px;\">{EmailTemplateFormatting.Escape(EmailTemplateFormatting.FormatCost(request.AiEstimatedTotalCost))}</span></p></div>"
        );

        if (!string.IsNullOrWhiteSpace(request.AdminDecisionComment))
        {
            content.AppendLine(
                $"<div style=\"margin-top:16px;padding-top:16px;border-top:1px solid {BorderColor};\"><p style=\"margin:0 0 6px 0;color:{AccentColor};font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:0.02em;\">Комментарий администратора</p><p style=\"margin:0;color:{MutedTextColor};font-size:14px;line-height:1.55;\">{EmailTemplateFormatting.Escape(request.AdminDecisionComment.Trim())}</p></div>"
            );
        }

        content.AppendLine(
            $"<p style=\"margin:24px 0 0 0;color:{MainTextColor};font-size:14px;\">Всего доброго</p>"
        );
        return BuildLayout(content.ToString());
    }

    internal static string BuildRejectionHtml(DamageRequest request)
    {
        var summaryText = EmailTemplateFormatting.Escape(request.AiSummary);
        var statusText = "Отклонена";
        var intro =
            $"По вашему автомобилю {EmailTemplateFormatting.Escape(request.CarBrand)} {EmailTemplateFormatting.Escape(request.CarModel)} {request.CarYear} сформирован итоговый вердикт.";
        var adminComment = string.IsNullOrWhiteSpace(request.AdminDecisionComment)
            ? "Без дополнительного комментария."
            : EmailTemplateFormatting.Escape(request.AdminDecisionComment.Trim());

        var content = new StringBuilder();
        content.AppendLine(
            $"<p style=\"margin:0 0 14px 0;color:{MainTextColor};font-size:17px;line-height:1.5;\">Здравствуйте, {EmailTemplateFormatting.Escape(EmailTemplateFormatting.BuildGreetingName(request))}</p>"
        );
        content.AppendLine(
            $"<p style=\"margin:0 0 22px 0;color:{MutedTextColor};font-size:15px;line-height:1.6;\">{intro}</p>"
        );
        content.AppendLine(BuildMetaBlock(request, statusText));
        content.AppendLine(
            $"<div style=\"background:#1a2434;border:1px solid #2a415f;border-radius:14px;padding:16px 18px;margin:0 0 20px 0;\"><p style=\"margin:0 0 8px 0;color:{AccentColor};font-weight:700;font-size:13px;letter-spacing:0.02em;text-transform:uppercase;\">AI резюме</p><p style=\"margin:0;color:{MainTextColor};font-size:14px;line-height:1.6;\">{summaryText}</p></div>"
        );
        content.AppendLine(BuildEstimateTable(request.EstimateItems));
        content.AppendLine(
            $"<div style=\"margin-top:16px;border-top:1px solid {BorderColor};padding-top:16px;\"><p style=\"margin:0;color:{MainTextColor};font-size:15px;\">Итоговая стоимость: <span style=\"color:{AccentColor};font-weight:700;font-size:20px;\">{EmailTemplateFormatting.Escape(EmailTemplateFormatting.FormatCost(request.AiEstimatedTotalCost))}</span></p></div>"
        );
        content.AppendLine(
            $"<div style=\"background:#261c22;border:1px solid #4c2a34;border-radius:14px;padding:16px 18px;margin:0 0 6px 0;\"><p style=\"margin:0 0 8px 0;color:#f19cb0;font-weight:700;font-size:13px;letter-spacing:0.02em;text-transform:uppercase;\">Комментарий администратора</p><p style=\"margin:0;color:{MainTextColor};font-size:14px;line-height:1.55;\">{adminComment}</p></div>"
        );
        content.AppendLine(
            $"<p style=\"margin:24px 0 0 0;color:{MainTextColor};font-size:14px;\">Всего доброго</p>"
        );
        return BuildLayout(content.ToString());
    }

    private static string BuildLayout(string innerHtml)
    {
        return $@"<!doctype html>
<html lang=""ru"">
  <head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
    <meta name=""color-scheme"" content=""light dark"">
    <meta name=""supported-color-schemes"" content=""light dark"">
    <title>{ProjectName}</title>
    <style>
      @media only screen and (max-width: 600px) {{
        .email-shell {{ padding: 12px !important; }}
        .email-card {{ border-radius: 14px !important; }}
        .email-content {{ padding: 18px !important; }}
      }}
    </style>
  </head>
  <body style=""margin:0;padding:0;background:{BackgroundColor};color:{MainTextColor};font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:{BackgroundColor};"">
      <tr>
        <td class=""email-shell"" align=""center"" style=""padding:20px;"">
          <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:680px;"">
            <tr>
              <td style=""background:#11243f;padding:16px 22px;border-radius:18px 18px 0 0;border:1px solid #2f4160;border-bottom:none;"">
                <p style=""margin:0;color:#ffffff !important;font-size:17px;font-weight:700;letter-spacing:0.02em;""><span style=""color:#ffffff !important;"">{ProjectName}</span></p>
              </td>
            </tr>
            <tr>
              <td class=""email-card"" style=""background:{CardColor};border:1px solid {BorderColor};border-radius:0 0 18px 18px;overflow:hidden;"">
                <div class=""email-content"" style=""padding:24px;"">{innerHtml}</div>
              </td>
            </tr>
            <tr>
              <td style=""padding:12px 8px 0 8px;"">
                <p style=""margin:0;color:#7f8890;font-size:12px;line-height:1.4;text-align:center;"">Это письмо отправлено автоматически сервисом {ProjectName}.</p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
    }

    private static string BuildMetaBlock(DamageRequest request, string statusText)
    {
        var rows = new[]
        {
            BuildMetaRow("Номер заявки", request.Id.ToString()),
            BuildMetaRow("Статус", statusText),
            BuildMetaRow("Клиент", EmailTemplateFormatting.BuildClientFullName(request)),
            BuildMetaRow(
                "Автомобиль",
                $"{request.CarBrand} {request.CarModel} ({request.CarYear})"
            ),
        };

        return $"<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:0 0 18px 0;border:1px solid {BorderColor};border-radius:12px;overflow:hidden;\">{string.Join(string.Empty, rows)}</table>";
    }

    private static string BuildMetaRow(string label, string value)
    {
        return $"<tr><td style=\"padding:11px 14px;border-bottom:1px solid {BorderColor};width:34%;color:{MutedTextColor};font-size:13px;\">{EmailTemplateFormatting.Escape(label)}</td><td style=\"padding:11px 14px;border-bottom:1px solid {BorderColor};color:{MainTextColor};font-size:14px;\">{EmailTemplateFormatting.Escape(value)}</td></tr>";
    }

    private static string BuildEstimateTable(IEnumerable<DamageEstimateItem> items)
    {
        var itemList = items.ToList();
        var sb = new StringBuilder();
        sb.Append(
            $"<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border-collapse:collapse;border:1px solid {BorderColor};border-radius:12px;overflow:hidden;\">"
        );
        sb.Append(
            $"<tr style=\"background:#1d2735;\"><th align=\"left\" style=\"padding:10px 12px;color:{MainTextColor};font-size:12px;letter-spacing:0.02em;text-transform:uppercase;border-bottom:1px solid {BorderColor};\">Деталь</th><th align=\"left\" style=\"padding:10px 12px;color:{MainTextColor};font-size:12px;letter-spacing:0.02em;text-transform:uppercase;border-bottom:1px solid {BorderColor};\">Описание</th><th align=\"left\" style=\"padding:10px 12px;color:{MainTextColor};font-size:12px;letter-spacing:0.02em;text-transform:uppercase;border-bottom:1px solid {BorderColor};\">Серьезность</th><th align=\"right\" style=\"padding:10px 12px;color:{MainTextColor};font-size:12px;letter-spacing:0.02em;text-transform:uppercase;border-bottom:1px solid {BorderColor};\">Стоимость</th></tr>"
        );

        if (itemList.Count == 0)
        {
            sb.Append(
                $"<tr><td colspan=\"4\" style=\"padding:14px 12px;color:{MutedTextColor};font-size:14px;border-bottom:1px solid {BorderColor};\">Повреждения не выявлены</td></tr>"
            );
            sb.Append("</table>");
            return sb.ToString();
        }

        foreach (var item in itemList)
        {
            sb.Append("<tr>");
            sb.Append(
                $"<td style=\"padding:12px;color:{MainTextColor};font-size:13px;border-bottom:1px solid {BorderColor};vertical-align:top;\">{EmailTemplateFormatting.Escape(item.PartName)}</td>"
            );
            sb.Append(
                $"<td style=\"padding:12px;color:{MutedTextColor};font-size:13px;line-height:1.5;border-bottom:1px solid {BorderColor};vertical-align:top;\">{EmailTemplateFormatting.Escape(item.DamageDescription)}</td>"
            );
            sb.Append(
                $"<td style=\"padding:12px;color:{MainTextColor};font-size:13px;border-bottom:1px solid {BorderColor};vertical-align:top;\">{EmailTemplateFormatting.Escape(EmailTemplateFormatting.FormatSeverityRu(item.Severity))}</td>"
            );
            sb.Append(
                $"<td align=\"right\" style=\"padding:12px;color:{MainTextColor};font-size:13px;border-bottom:1px solid {BorderColor};vertical-align:top;\">{EmailTemplateFormatting.Escape(EmailTemplateFormatting.FormatCost(item.EstimatedCost))}</td>"
            );
            sb.Append("</tr>");
        }

        sb.Append("</table>");
        return sb.ToString();
    }
}
