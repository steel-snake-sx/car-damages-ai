using System.Text;
using CarDamageClaims.Api.Models;

namespace CarDamageClaims.Api.Services.Email.Templates;

internal static class RequestDecisionText
{
    private const string ProjectName = "Car Damage Claims AI";

    internal static string BuildApprovalText(DamageRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ProjectName);
        sb.AppendLine();
        sb.AppendLine($"Здравствуйте, {EmailTemplateFormatting.BuildGreetingName(request)}");
        sb.AppendLine(
            $"По вашему автомобилю {request.CarBrand} {request.CarModel} {request.CarYear} сформирован итоговый вердикт."
        );
        sb.AppendLine();
        sb.AppendLine($"Номер заявки: {request.Id}");
        sb.AppendLine("Статус: Одобрена");
        sb.AppendLine($"Клиент: {EmailTemplateFormatting.BuildClientFullName(request)}");
        sb.AppendLine($"Автомобиль: {request.CarBrand} {request.CarModel} ({request.CarYear})");
        sb.AppendLine();
        sb.AppendLine("AI резюме:");
        sb.AppendLine(request.AiSummary);
        sb.AppendLine();
        sb.AppendLine("Детализация повреждений:");

        var hasItems = false;
        foreach (
            var item in request.EstimateItems.OrderBy(
                x => x.PartName,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            hasItems = true;
            sb.AppendLine($"- Деталь: {item.PartName}");
            sb.AppendLine($"  Описание: {item.DamageDescription}");
            sb.AppendLine(
                $"  Серьезность: {EmailTemplateFormatting.FormatSeverityRu(item.Severity)}"
            );
            sb.AppendLine($"  Стоимость: {EmailTemplateFormatting.FormatCost(item.EstimatedCost)}");
        }

        if (!hasItems)
        {
            sb.AppendLine("- Повреждения не выявлены.");
        }

        sb.AppendLine();
        sb.AppendLine(
            $"Итоговая стоимость: {EmailTemplateFormatting.FormatCost(request.AiEstimatedTotalCost)}"
        );

        if (!string.IsNullOrWhiteSpace(request.AdminDecisionComment))
        {
            sb.AppendLine($"Комментарий администратора: {request.AdminDecisionComment.Trim()}");
        }

        sb.AppendLine();
        sb.AppendLine("Всего доброго");

        return sb.ToString();
    }

    internal static string BuildRejectionText(DamageRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ProjectName);
        sb.AppendLine();
        sb.AppendLine($"Здравствуйте, {EmailTemplateFormatting.BuildGreetingName(request)}");
        sb.AppendLine(
            $"По вашему автомобилю {request.CarBrand} {request.CarModel} {request.CarYear} сформирован итоговый вердикт."
        );
        sb.AppendLine();
        sb.AppendLine($"Номер заявки: {request.Id}");
        sb.AppendLine("Статус: Отклонена");
        sb.AppendLine($"Клиент: {EmailTemplateFormatting.BuildClientFullName(request)}");
        sb.AppendLine($"Автомобиль: {request.CarBrand} {request.CarModel} ({request.CarYear})");
        sb.AppendLine();
        sb.AppendLine("AI резюме:");
        sb.AppendLine(request.AiSummary);
        sb.AppendLine();
        sb.AppendLine("Детализация повреждений:");

        var hasItems = false;
        foreach (
            var item in request.EstimateItems.OrderBy(
                x => x.PartName,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            hasItems = true;
            sb.AppendLine($"- Деталь: {item.PartName}");
            sb.AppendLine($"  Описание: {item.DamageDescription}");
            sb.AppendLine(
                $"  Серьезность: {EmailTemplateFormatting.FormatSeverityRu(item.Severity)}"
            );
            sb.AppendLine($"  Стоимость: {EmailTemplateFormatting.FormatCost(item.EstimatedCost)}");
        }

        if (!hasItems)
        {
            sb.AppendLine("- Повреждения не выявлены.");
        }

        sb.AppendLine();
        sb.AppendLine(
            $"Итоговая стоимость: {EmailTemplateFormatting.FormatCost(request.AiEstimatedTotalCost)}"
        );
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.AdminDecisionComment))
        {
            sb.AppendLine($"Комментарий администратора: {request.AdminDecisionComment.Trim()}");
        }
        else
        {
            sb.AppendLine("Комментарий администратора: без дополнительного комментария.");
        }

        sb.AppendLine();
        sb.AppendLine("Всего доброго");

        return sb.ToString();
    }
}
