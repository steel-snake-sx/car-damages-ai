using System.Globalization;
using System.Net;
using CarDamageClaims.Api.Models;

namespace CarDamageClaims.Api.Services.Email.Templates;

internal static class EmailTemplateFormatting
{
    internal static string BuildClientFullName(DamageRequest request)
    {
        var parts = new[] { request.LastName, request.FirstName, request.MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim());

        return string.Join(' ', parts);
    }

    internal static string BuildGreetingName(DamageRequest request)
    {
        var parts = new[] { request.FirstName, request.MiddleName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim())
            .ToList();

        if (parts.Count == 0)
        {
            return request.LastName.Trim();
        }

        return string.Join(' ', parts);
    }

    internal static string BuildCarResultSubject(DamageRequest request)
    {
        var brand = request.CarBrand.Trim();
        var model = request.CarModel.Trim();
        var hasYear = request.CarYear > 0;

        if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(model) || !hasYear)
        {
            return "Результат оценки вашего автомобиля";
        }

        return $"Итог оценки автомобиля {brand} {model} {request.CarYear}";
    }

    internal static string FormatCost(decimal value)
    {
        return $"{value.ToString("N2", CultureInfo.GetCultureInfo("ru-RU"))} RUB";
    }

    internal static string Escape(string? value)
    {
        return WebUtility.HtmlEncode(value ?? string.Empty);
    }

    internal static string FormatSeverityRu(string severity)
    {
        var normalized = severity.Trim().ToLowerInvariant();

        return normalized switch
        {
            "low" => "низкая",
            "medium" => "средняя",
            "high" => "высокая",
            _ => severity,
        };
    }
}
