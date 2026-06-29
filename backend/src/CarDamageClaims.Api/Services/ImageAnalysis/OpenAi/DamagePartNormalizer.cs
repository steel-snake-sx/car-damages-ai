namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class DamagePartNormalizer
{
    public string NormalizeSide(string? side, bool sideAmbiguous, double sideConfidence)
    {
        if (sideAmbiguous || sideConfidence < 0.55)
        {
            return "unknown";
        }

        var normalized = side?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "left" => "left",
            "right" => "right",
            "front" => "front",
            "rear" => "rear",
            "center" => "center",
            _ => "unknown",
        };
    }

    public string AdjustSideForPart(string normalizedPartName, string side)
    {
        var part = normalizedPartName.ToLowerInvariant();

        if (ContainsAny(part, "лобовое стекло", "заднее стекло", "крыша", "капот", "багажник"))
        {
            return side is "left" or "right" ? "center" : side;
        }

        if (ContainsAny(part, "боковое стекло") && side == "center")
        {
            return "unknown";
        }

        return side;
    }

    public string NormalizeSeverity(string? severity)
    {
        var normalized = severity?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "low" => "low",
            "high" => "high",
            _ => "medium",
        };
    }

    public string ResolvePartName(
        string? partName,
        string? damageType,
        string? evidence,
        string side
    )
    {
        var normalized = NormalizePartNameToRussian(partName);
        var normalizedLower = normalized.ToLowerInvariant();

        if (!IsGenericPartToken(normalizedLower))
        {
            return normalized;
        }

        var context = $"{partName} {damageType} {evidence}".ToLowerInvariant();

        if (ContainsAny(context, "капот", "hood"))
        {
            return "капот";
        }

        if (ContainsAny(context, "фара", "headlight", "lamp"))
        {
            return "фара";
        }

        if (ContainsAny(context, "бампер", "bumper"))
        {
            return "бампер";
        }

        if (ContainsAny(context, "решет", "grille"))
        {
            return "решетка";
        }

        if (ContainsAny(context, "двер", "door"))
        {
            return "дверь";
        }

        if (ContainsAny(context, "крыш", "roof"))
        {
            return "крыша";
        }

        if (ContainsAny(context, "window", "glass", "стекл"))
        {
            if (ContainsAny(context, "windshield", "лобов"))
            {
                return "лобовое стекло";
            }

            if (ContainsAny(context, "rear", "зад"))
            {
                if (ContainsAny(context, "door", "двер"))
                {
                    return "заднее боковое стекло";
                }

                return "заднее стекло";
            }

            if (ContainsAny(context, "front", "перед") && ContainsAny(context, "door", "двер"))
            {
                return "переднее боковое стекло";
            }

            return "боковое стекло";
        }

        if (normalizedLower.Contains("side") || normalizedLower.Contains("бок"))
        {
            return "боковая панель";
        }

        if (normalizedLower.Contains("front") || normalizedLower.Contains("перед"))
        {
            return "бампер";
        }

        if (normalizedLower.Contains("rear") || normalizedLower.Contains("зад"))
        {
            return "багажник";
        }

        return normalized;
    }

    public bool IsGenericPartToken(string value)
    {
        return ContainsAny(
            value,
            "front",
            "rear",
            "side",
            "roof",
            "перед",
            "зад",
            "бок",
            "верх",
            "часть"
        );
    }

    public string NormalizeDamageDescriptionToRussian(string damageType)
    {
        var raw = damageType.Trim();
        var lower = raw.ToLowerInvariant();

        if (ContainsAny(lower, "broken", "crack", "shatter", "разбит", "трещ"))
        {
            return "разбитие";
        }

        if (ContainsAny(lower, "rollover", "перевер", "опрокид"))
        {
            return "повреждение от опрокидывания";
        }

        if (ContainsAny(lower, "dent", "deform", "вмят", "деформ"))
        {
            return "вмятина";
        }

        if (ContainsAny(lower, "scratch", "царап"))
        {
            return "царапины";
        }

        if (ContainsAny(lower, "breakage", "glass break", "window break"))
        {
            return "разбитие";
        }

        if (ContainsAny(lower, "collision", "impact", "удар"))
        {
            return "повреждение от удара";
        }

        return raw;
    }

    public string NormalizePartNameToRussian(string? partName)
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

    public string FormatSideForDisplay(string side)
    {
        return side switch
        {
            "left" => "лево",
            "right" => "право",
            "front" => "перед",
            "rear" => "зад",
            "center" => "центр",
            _ => "неизвестно",
        };
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        var lower = value.ToLowerInvariant();
        return tokens.Any(lower.Contains);
    }
}
