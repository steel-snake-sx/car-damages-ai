namespace CarDamageClaims.Api.Services.ImageAnalysis;

public class ImageAnalysisResult
{
    public bool IsCar { get; set; }

    public string Summary { get; set; } = string.Empty;

    public decimal EstimatedTotalCost { get; set; }

    public double Confidence { get; set; }

    public List<ImageDamageItemResult> DamageItems { get; set; } = new();
}
