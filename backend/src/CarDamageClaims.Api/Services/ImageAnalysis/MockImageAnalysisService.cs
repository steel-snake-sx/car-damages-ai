namespace CarDamageClaims.Api.Services.ImageAnalysis;

public class MockImageAnalysisService : IImageAnalysisService
{
    public Task<ImageAnalysisResult> AnalyzeAsync(
        IReadOnlyList<string> filePaths,
        string? carBrand = null,
        string? carModel = null,
        int? carYear = null,
        CancellationToken cancellationToken = default
    )
    {
        var items = new List<ImageDamageItemResult>
        {
            new()
            {
                PartName = "Front bumper",
                DamageDescription = "Surface scratches and minor paint damage",
                Severity = "medium",
                EstimatedCost = 18000m,
                Confidence = 0.91,
            },
            new()
            {
                PartName = "Left fender",
                DamageDescription = "Small dent near wheel arch",
                Severity = "low",
                EstimatedCost = 9500m,
                Confidence = 0.88,
            },
        };

        var result = new ImageAnalysisResult
        {
            IsCar = true,
            Summary =
                "Detected a car with visible cosmetic front-left damage. Repair is likely straightforward without structural work.",
            EstimatedTotalCost = items.Sum(x => x.EstimatedCost),
            Confidence = 0.9,
            DamageItems = items,
        };

        return Task.FromResult(result);
    }
}
