using System.Text.Json.Serialization;

namespace CarDamageClaims.Api.Services.ImageAnalysis.OpenAi;

public sealed class PricingResponse
{
    [JsonPropertyName("repair_total_min")]
    public decimal RepairTotalMin { get; set; }

    [JsonPropertyName("repair_total_max")]
    public decimal RepairTotalMax { get; set; }

    [JsonPropertyName("part_prices")]
    public List<PartPriceItem> PartPrices { get; set; } = new();
}
