using CarDamageClaims.Api.Services.ImageAnalysis;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class RepairCostMatcher
{
    public void ApplyPricing(List<ImageDamageItemResult> items, PricingResponse pricing)
    {
        if (pricing.PartPrices.Count == 0)
        {
            return;
        }

        var pricesByCategory = pricing
            .PartPrices.Select(x => new { Category = GetPartCategory(x.PartName), Price = x.Price })
            .Where(x => !string.IsNullOrWhiteSpace(x.Category) && x.Price > 0)
            .GroupBy(x => x.Category)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.Price).OrderBy(x => x).ToList()
            );

        foreach (var item in items)
        {
            var itemCategory = GetPartCategory(item.PartName);
            if (
                !string.IsNullOrWhiteSpace(itemCategory)
                && pricesByCategory.TryGetValue(itemCategory, out var prices)
            )
            {
                item.EstimatedCost = CalculateMedian(prices);
                continue;
            }

            var match = pricing.PartPrices.FirstOrDefault(p =>
                ContainsAny(item.PartName, p.PartName) || ContainsAny(p.PartName, item.PartName)
            );
            if (match is not null && match.Price > 0)
            {
                item.EstimatedCost = Math.Round(match.Price, 0);
            }
        }
    }

    private static decimal CalculateMedian(IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
        {
            return 0;
        }

        var middle = values.Count / 2;
        if (values.Count % 2 == 1)
        {
            return Math.Round(values[middle], 0);
        }

        return Math.Round((values[middle - 1] + values[middle]) / 2m, 0);
    }

    public void ApplyCategoryFallbackPrices(List<ImageDamageItemResult> items)
    {
        foreach (var item in items.Where(x => x.EstimatedCost <= 0))
        {
            var category = GetPartCategory(item.PartName);
            var basePrice = category switch
            {
                "капот" => 18000m,
                "крыло" => 8500m,
                "дверь" => 19000m,
                "боковая панель" => 18000m,
                "бампер" => 11000m,
                "фара" => 5000m,
                "фонарь" => 4500m,
                "решетка" => 3500m,
                "багажник" => 16000m,
                "лобовое стекло" => 8000m,
                "заднее стекло" => 6500m,
                "боковое стекло" => 4500m,
                "крыша" => 22000m,
                "зеркало" => 3500m,
                _ => 0m,
            };

            if (basePrice <= 0)
            {
                continue;
            }

            item.EstimatedCost = Math.Round(ApplySeverityMultiplier(basePrice, item.Severity), 0);
        }
    }

    private static decimal ApplySeverityMultiplier(decimal basePrice, string severity)
    {
        return severity.ToLowerInvariant() switch
        {
            "high" => basePrice * 1.15m,
            "low" => basePrice * 0.85m,
            _ => basePrice,
        };
    }

    private static string GetPartCategory(string partName)
    {
        var normalized = NormalizePartNameToRussian(partName).ToLowerInvariant();

        if (ContainsAny(normalized, "капот"))
        {
            return "капот";
        }

        if (ContainsAny(normalized, "крыло"))
        {
            return "крыло";
        }

        if (ContainsAny(normalized, "фара"))
        {
            return "фара";
        }

        if (ContainsAny(normalized, "фонарь"))
        {
            return "фонарь";
        }

        if (ContainsAny(normalized, "фонарь", "taillight", "tail light"))
        {
            return "фонарь";
        }

        if (ContainsAny(normalized, "бампер"))
        {
            return "бампер";
        }

        if (ContainsAny(normalized, "решетка"))
        {
            return "решетка";
        }

        if (ContainsAny(normalized, "багаж", "trunk", "boot"))
        {
            return "багажник";
        }

        if (ContainsAny(normalized, "дверь"))
        {
            return "дверь";
        }

        if (ContainsAny(normalized, "боковая панель", "боковина"))
        {
            return "боковая панель";
        }

        if (ContainsAny(normalized, "боковина", "боковая панель", "сторона кузова"))
        {
            return "боковая панель";
        }

        if (ContainsAny(normalized, "багаж"))
        {
            return "багажник";
        }

        if (ContainsAny(normalized, "лобовое стекло"))
        {
            return "лобовое стекло";
        }

        if (ContainsAny(normalized, "заднее стекло"))
        {
            return "заднее стекло";
        }

        if (ContainsAny(normalized, "боковое стекло"))
        {
            return "боковое стекло";
        }

        if (ContainsAny(normalized, "крыша", "roof"))
        {
            return "крыша";
        }

        if (ContainsAny(normalized, "зерк"))
        {
            return "зеркало";
        }

        if (ContainsAny(normalized, "лобов", "windshield"))
        {
            return "лобовое стекло";
        }

        if (ContainsAny(normalized, "задн") && ContainsAny(normalized, "стекл", "window"))
        {
            return "заднее стекло";
        }

        if (ContainsAny(normalized, "боков") && ContainsAny(normalized, "стекл", "window"))
        {
            return "боковое стекло";
        }

        return string.Empty;
    }

    private static string NormalizePartNameToRussian(string? partName)
    {
        var raw = partName?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var lower = raw.ToLowerInvariant();
        if (ContainsAny(lower, "hood", "капот"))
        {
            return "капот";
        }

        if (ContainsAny(lower, "fender", "wing", "крыл"))
        {
            return "крыло";
        }

        if (ContainsAny(lower, "headlight", "lamp", "фара"))
        {
            return "фара";
        }

        if (ContainsAny(lower, "bumper", "бампер"))
        {
            return "бампер";
        }

        if (ContainsAny(lower, "grille", "решет"))
        {
            return "решетка";
        }

        if (ContainsAny(lower, "door", "двер"))
        {
            return "дверь";
        }

        if (
            ContainsAny(
                lower,
                "боковина",
                "боковая панель",
                "сторона кузова",
                "side body",
                "quarter panel"
            )
        )
        {
            return "боковая панель";
        }

        if (ContainsAny(lower, "roof", "крыша", "крыш"))
        {
            return "крыша";
        }

        if (ContainsAny(lower, "window", "glass", "стекл"))
        {
            if (ContainsAny(lower, "windshield", "лобов"))
            {
                return "лобовое стекло";
            }

            if (ContainsAny(lower, "rear", "зад"))
            {
                if (ContainsAny(lower, "door", "двер"))
                {
                    return "заднее боковое стекло";
                }

                return "заднее стекло";
            }

            if (ContainsAny(lower, "front", "перед") && ContainsAny(lower, "door", "двер"))
            {
                return "переднее боковое стекло";
            }

            return "боковое стекло";
        }

        return raw;
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        var lower = value.ToLowerInvariant();
        return tokens.Any(lower.Contains);
    }
}
