namespace CarDamageClaims.Api.Services.ImageAnalysis;

public class ImageDamageItemResult
{
    public string PartName { get; set; } = string.Empty;

    public string DamageDescription { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public decimal EstimatedCost { get; set; }

    public double Confidence { get; set; }
}
