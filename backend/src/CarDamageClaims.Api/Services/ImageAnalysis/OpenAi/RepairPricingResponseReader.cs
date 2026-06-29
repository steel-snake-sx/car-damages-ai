using System.Text.Json;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class RepairPricingResponseReader
{
    public PricingResponse ParsePricingResponseFlexible(string json)
    {
        var result = new PricingResponse();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            result.RepairTotalMin = FirstDecimal(
                root,
                "repair_total_min",
                "repairMin",
                "repair_cost_min"
            );
            result.RepairTotalMax = FirstDecimal(
                root,
                "repair_total_max",
                "repairMax",
                "repair_cost_max"
            );

            var partPricesElement = FirstProperty(
                root,
                "part_prices",
                "partPrices",
                "prices",
                "parts",
                "items"
            );
            if (partPricesElement.HasValue)
            {
                var partPrices = partPricesElement.Value;
                if (partPrices.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in partPrices.EnumerateArray())
                    {
                        var partName = FirstString(
                            item,
                            "part_name",
                            "partName",
                            "name",
                            "part",
                            "detail"
                        );
                        var price = FirstDecimal(item, "price", "cost", "median_price", "value");
                        if (!string.IsNullOrWhiteSpace(partName) && price > 0)
                        {
                            result.PartPrices.Add(
                                new PartPriceItem { PartName = partName, Price = price }
                            );
                        }
                    }
                }
                else if (partPrices.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in partPrices.EnumerateObject())
                    {
                        var partName = prop.Name;
                        var price =
                            prop.Value.ValueKind == JsonValueKind.Object
                                ? FirstDecimal(prop.Value, "price", "cost", "median_price", "value")
                                : TryGetDecimal(prop.Value);

                        if (!string.IsNullOrWhiteSpace(partName) && price > 0)
                        {
                            result.PartPrices.Add(
                                new PartPriceItem { PartName = partName, Price = price }
                            );
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            return ParsePricingFromPlainText(json);
        }

        if (result.PartPrices.Count == 0)
        {
            return ParsePricingFromPlainText(json, result);
        }

        return result;
    }

    private PricingResponse ParsePricingFromPlainText(string raw, PricingResponse? seed = null)
    {
        var result = seed ?? new PricingResponse();
        var lines = raw.Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        foreach (var line in lines)
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex < 1)
            {
                continue;
            }

            var left = line[..separatorIndex]
                .Trim()
                .Trim('-', '*', ' ', '"')
                .Replace("part_name", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("price", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("name", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Trim(':', ' ', '"', ',', '{', '}');
            var right = line[(separatorIndex + 1)..].Trim();
            var price = ParseDecimalFromAny(right);
            if (!string.IsNullOrWhiteSpace(left) && price > 0)
            {
                result.PartPrices.Add(new PartPriceItem { PartName = left, Price = price });
            }
        }

        if (result.PartPrices.Count == 0)
        {
            ParsePricingByRegex(raw, result);
        }

        return result;
    }

    private void ParsePricingByRegex(string raw, PricingResponse result)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(
            raw,
            "\"part_name\"\\s*:\\s*\"(?<part>[^\"]+)\"\\s*,\\s*\"price\"\\s*:\\s*(?<price>[0-9][0-9\\.,]*)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        if (matches.Count == 0)
        {
            matches = System.Text.RegularExpressions.Regex.Matches(
                raw,
                "\"part_name\"\\s*:\\s*\"(?<part>[^\"]+)\"[^\\n\\r]*?\"price\"\\s*:\\s*(?<price>[0-9][0-9\\.,]*)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    | System.Text.RegularExpressions.RegexOptions.Singleline
            );
        }

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var part = match.Groups["part"].Value.Trim();
            var price = ParseDecimalFromAny(match.Groups["price"].Value);
            if (!string.IsNullOrWhiteSpace(part) && price > 0)
            {
                result.PartPrices.Add(new PartPriceItem { PartName = part, Price = price });
            }
        }

        if (result.RepairTotalMin <= 0)
        {
            var minMatch = System.Text.RegularExpressions.Regex.Match(
                raw,
                "\"repair_total_min\"\\s*:\\s*(?<value>[0-9][0-9\\.,]*)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            if (minMatch.Success)
            {
                result.RepairTotalMin = ParseDecimalFromAny(minMatch.Groups["value"].Value);
            }
        }

        if (result.RepairTotalMax <= 0)
        {
            var maxMatch = System.Text.RegularExpressions.Regex.Match(
                raw,
                "\"repair_total_max\"\\s*:\\s*(?<value>[0-9][0-9\\.,]*)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            if (maxMatch.Success)
            {
                result.RepairTotalMax = ParseDecimalFromAny(maxMatch.Groups["value"].Value);
            }
        }
    }

    private JsonElement? FirstProperty(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value))
            {
                return value;
            }
        }

        return null;
    }

    private decimal FirstDecimal(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var value = TryGetDecimal(element, name);
            if (value > 0)
            {
                return value;
            }
        }

        return 0;
    }

    private string FirstString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var value = TryGetString(element, name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private decimal TryGetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return 0;
        }

        return TryGetDecimal(value);
    }

    private decimal TryGetDecimal(JsonElement element)
    {
        if (
            element.ValueKind == JsonValueKind.Number
            && element.TryGetDecimal(out var decimalValue)
        )
        {
            return decimalValue;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var raw = element.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return 0;
            }

            return ParseDecimalFromAny(raw);
        }

        return 0;
    }

    private string TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return string.Empty;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private decimal ParseDecimalFromAny(string raw)
    {
        var cleaned = new string(
            raw.Where(ch => char.IsDigit(ch) || ch == '.' || ch == ',').ToArray()
        ).Replace(',', '.');
        if (
            decimal.TryParse(
                cleaned,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed
            )
        )
        {
            return parsed;
        }

        return 0;
    }
}
