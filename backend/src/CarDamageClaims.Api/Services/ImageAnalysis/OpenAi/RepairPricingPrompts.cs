using System.Text.Json;
using CarDamageClaims.Api.Services.ImageAnalysis;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public class RepairPricingPrompts
{
    public string GetPricingPrompt(
        string? carBrand,
        string? carModel,
        int? carYear,
        IReadOnlyList<ImageDamageItemResult> items
    )
    {
        var parts = items
            .Select(x => new
            {
                part_name = x.PartName,
                severity = x.Severity,
                confidence = x.Confidence,
            })
            .ToArray();

        var partsJson = JsonSerializer.Serialize(parts);

        return $"""
            Ты оцениваешь стоимость покупки запчастей в рублях только по доверенным источникам: avito.ru, drom.ru, auto.ru, exist.ru, emex.ru, zzap.ru, autodoc.ru.
            Автомобиль: {carBrand ?? "не указан"} {carModel ?? ""} {carYear?.ToString() ?? ""}.
            Список поврежденных деталей (не менять, цены дать для каждой позиции): {partsJson}

            Правила:
            - Верни цены для КАЖДОЙ детали из входного списка, не пропускай элементы.
            - Значение part_name в ответе должно совпадать с названием из входного списка.
            - Для каждой детали собери 3-5 релевантных предложений.
            - Исключи: "на запчасти", "под восстановление", "неполный комплект", "без крепежа", неясное состояние.
            - Используй медиану релевантных цен.
            - Если у детали мало данных, используй ближайший рыночный аналог и явно укажи это в notes.
            - Верни только JSON.

            Обязательные поля JSON:
            - repair_total_min: number
            - repair_total_max: number
            - part_prices: array of objects with fields part_name (string) and price (number)
            - notes: array of strings
            """;
    }
}
