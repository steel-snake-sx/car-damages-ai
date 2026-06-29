using CarDamageClaims.Api.Services.ImageAnalysis;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class DamageInferenceService
{
    public void AddInferredFenderIfLikely(List<ImageDamageItemResult> items)
    {
        var hasFender = items.Any(i => ContainsAny(i.PartName, "крыл", "fender"));
        if (hasFender)
        {
            return;
        }

        var hood = items.FirstOrDefault(i => ContainsAny(i.PartName, "капот", "hood"));
        var headlight = items.FirstOrDefault(i => ContainsAny(i.PartName, "фара", "headlight"));

        if (hood is null || headlight is null)
        {
            return;
        }

        var inferredSide = ExtractSide(headlight.PartName);
        var sideSuffix = inferredSide == "unknown" ? "front" : inferredSide;

        items.Add(
            new ImageDamageItemResult
            {
                PartName = $"крыло ({FormatSideForDisplay(sideSuffix)})",
                DamageDescription = "вероятная деформация по зоне стыка капот-крыло",
                Severity = "medium",
                EstimatedCost = 0,
                Confidence = 0.55,
            }
        );
    }

    public void AddInferredFrontAdjacentParts(List<ImageDamageItemResult> items)
    {
        var hasHood = items.Any(i => ContainsAny(i.PartName, "капот", "hood"));
        var hasFender = items.Any(i => ContainsAny(i.PartName, "крыл", "fender", "wing"));
        var hasHeadlight = items.Any(i => ContainsAny(i.PartName, "фара", "headlight", "lamp"));

        var frontSetCount = new[] { hasHood, hasFender, hasHeadlight }.Count(x => x);
        if (frontSetCount != 1)
        {
            return;
        }

        if (hasFender && !hasHood)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "капот (перед)",
                    DamageDescription = "вероятная деформация соседней детали",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }

        if (hasHood && !hasFender)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "крыло (лево)",
                    DamageDescription = "вероятная деформация соседней детали",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }

        if (hasHeadlight && !hasFender)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "крыло (лево)",
                    DamageDescription = "вероятная деформация рядом с фарой",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }
    }

    public void AddInferredRearAdjacentParts(List<ImageDamageItemResult> items)
    {
        var hasTrunk = items.Any(i => ContainsAny(i.PartName, "багаж", "trunk", "boot"));
        var hasRearBumper = items.Any(i =>
            ContainsAny(i.PartName, "задний бампер", "rear bumper")
            || (ContainsAny(i.PartName, "бампер") && ContainsAny(i.PartName, "зад"))
        );
        var hasRearLight = items.Any(i =>
            ContainsAny(i.PartName, "задняя фара", "фонарь", "taillight", "tail light")
        );
        var hasRearFender = items.Any(i =>
            ContainsAny(i.PartName, "заднее крыло", "rear fender")
            || (ContainsAny(i.PartName, "крыло") && ContainsAny(i.PartName, "зад"))
        );

        var rearSetCount = new[] { hasTrunk, hasRearBumper, hasRearLight, hasRearFender }.Count(x =>
            x
        );
        if (rearSetCount != 1)
        {
            return;
        }

        if (hasTrunk && !hasRearBumper)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "задний бампер (зад)",
                    DamageDescription = "вероятное соседнее повреждение задней зоны",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }

        if (hasRearBumper && !hasTrunk)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "багажник (зад)",
                    DamageDescription = "вероятное соседнее повреждение задней зоны",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }
    }

    public void AddInferredGlassParts(List<ImageDamageItemResult> items)
    {
        var hasWindshield = items.Any(i => ContainsAny(i.PartName, "лобов", "windshield"));
        var hasRearGlass = items.Any(i => ContainsAny(i.PartName, "заднее стекло", "rear window"));
        var hasSideGlass = items.Any(i =>
            ContainsAny(i.PartName, "боковое стекло", "side window", "window")
            && ContainsAny(i.PartName, "лево", "право", "left", "right")
        );

        var hasFrontHit = items.Any(i =>
            ContainsAny(i.PartName, "капот", "бампер", "фара")
            && ContainsAny(i.PartName, "перед", "лево", "право")
        );
        var hasRoofDamage = items.Any(i => ContainsAny(i.PartName, "крыша", "roof"));

        if (hasFrontHit && !hasWindshield)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "лобовое стекло (центр)",
                    DamageDescription = "проверить на скрытые трещины после удара",
                    Severity = "low",
                    EstimatedCost = 0,
                    Confidence = 0.4,
                }
            );
        }

        if (hasRoofDamage && !hasSideGlass)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "переднее боковое стекло (лево)",
                    DamageDescription = "вероятное повреждение при опрокидывании",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }

        if (hasRoofDamage && !hasRearGlass)
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "заднее стекло (центр)",
                    DamageDescription = "вероятное повреждение при опрокидывании",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.5,
                }
            );
        }
    }

    public void AddInferredRolloverParts(List<ImageDamageItemResult> items, string? summary)
    {
        var summaryLower = summary?.ToLowerInvariant() ?? string.Empty;
        var sideImpactSignals = items.Count(i =>
            ContainsAny(i.PartName, "двер", "крыл", "бок", "стекло")
            && ContainsAny(i.PartName, "лево", "право", "left", "right")
        );
        var hasRoofSignal = items.Any(i => ContainsAny(i.PartName, "крыша", "roof"));
        var hasRolloverSignals = ContainsAny(
            summaryLower,
            "опрокид",
            "перевер",
            "перевёр",
            "перевернут",
            "перевёрнут",
            "лежит на боку",
            "rollover",
            "on side"
        );

        if (!hasRolloverSignals && hasRoofSignal && sideImpactSignals >= 2)
        {
            hasRolloverSignals = true;
        }

        if (!hasRolloverSignals)
        {
            return;
        }

        if (!items.Any(i => ContainsAny(i.PartName, "крыша", "roof")))
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "крыша (центр)",
                    DamageDescription = "вероятная деформация при опрокидывании",
                    Severity = "high",
                    EstimatedCost = 0,
                    Confidence = 0.6,
                }
            );
        }

        if (!items.Any(i => ContainsAny(i.PartName, "лобов", "windshield")))
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "лобовое стекло (центр)",
                    DamageDescription = "вероятное повреждение при опрокидывании",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.55,
                }
            );
        }

        if (!items.Any(i => ContainsAny(i.PartName, "заднее стекло", "rear window")))
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "заднее стекло (центр)",
                    DamageDescription = "вероятное повреждение при опрокидывании",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.55,
                }
            );
        }

        if (!items.Any(i => ContainsAny(i.PartName, "багаж", "trunk", "boot")))
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "багажник (зад)",
                    DamageDescription = "вероятная деформация задней зоны при опрокидывании",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.55,
                }
            );
        }

        if (
            !items.Any(i =>
                ContainsAny(i.PartName, "задний бампер", "rear bumper")
                || (ContainsAny(i.PartName, "бампер") && ContainsAny(i.PartName, "зад"))
            )
        )
        {
            items.Add(
                new ImageDamageItemResult
                {
                    PartName = "задний бампер (зад)",
                    DamageDescription = "вероятное соседнее повреждение задней зоны",
                    Severity = "medium",
                    EstimatedCost = 0,
                    Confidence = 0.55,
                }
            );
        }
    }

    public List<ImageDamageItemResult> DeduplicateItems(List<ImageDamageItemResult> items)
    {
        return items
            .GroupBy(item =>
            {
                var category = GetPartCategory(item.PartName);
                if (string.IsNullOrWhiteSpace(category))
                {
                    category = NormalizePartNameToRussian(item.PartName).ToLowerInvariant();
                }

                return $"{category}|{ExtractDisplaySide(item.PartName)}";
            })
            .Select(group =>
            {
                var best = group.OrderByDescending(x => x.Confidence).First();
                return new ImageDamageItemResult
                {
                    PartName = best.PartName,
                    DamageDescription = best.DamageDescription,
                    Severity = best.Severity,
                    EstimatedCost = group.Max(x => x.EstimatedCost),
                    Confidence = best.Confidence,
                };
            })
            .ToList();
    }

    private static string ExtractSide(string partName)
    {
        var name = partName.ToLowerInvariant();
        if (name.Contains("left") || name.Contains("лев"))
        {
            return "left";
        }

        if (name.Contains("right") || name.Contains("прав"))
        {
            return "right";
        }

        return "unknown";
    }

    private static string ExtractDisplaySide(string partName)
    {
        var lower = partName.ToLowerInvariant();
        if (ContainsAny(lower, "лево", "left"))
        {
            return "left";
        }

        if (ContainsAny(lower, "право", "right"))
        {
            return "right";
        }

        if (ContainsAny(lower, "перед", "front"))
        {
            return "front";
        }

        if (ContainsAny(lower, "зад", "rear"))
        {
            return "rear";
        }

        if (ContainsAny(lower, "центр", "center"))
        {
            return "center";
        }

        return "unknown";
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

    private static string FormatSideForDisplay(string side)
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
