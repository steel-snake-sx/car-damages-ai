namespace CarDamageClaims.Api.Services.ImageAnalysis;

public interface IImageAnalysisService
{
    Task<ImageAnalysisResult> AnalyzeAsync(
        IReadOnlyList<string> filePaths,
        string? carBrand = null,
        string? carModel = null,
        int? carYear = null,
        CancellationToken cancellationToken = default
    );
}
