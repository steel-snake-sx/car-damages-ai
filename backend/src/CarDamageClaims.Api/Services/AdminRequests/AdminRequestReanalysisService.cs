using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.Models;
using CarDamageClaims.Api.Services.ImageAnalysis;
using CarDamageClaims.Api.Services.ImageAnalysis.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class AdminRequestReanalysisService(
    AppDbContext dbContext,
    IImageAnalysisService imageAnalysisService,
    IWebHostEnvironment environment
)
{
    public async Task<ReanalyzeRequestResult> ReanalyzeAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var request = await dbContext
            .DamageRequests.Include(x => x.Photos)
            .Include(x => x.EstimateItems)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return new ReanalyzeRequestResult { Status = ReanalyzeRequestStatus.NotFound };
        }

        var filePaths = request
            .Photos.OrderBy(x => x.SortOrder)
            .Take(3)
            .Select(photo => ResolveStorageFilePath(photo))
            .Where(path => System.IO.File.Exists(path))
            .ToList();

        if (filePaths.Count == 0)
        {
            return new ReanalyzeRequestResult { Status = ReanalyzeRequestStatus.NoValidImages };
        }

        ImageAnalysisResult analysisResult;
        try
        {
            analysisResult = await imageAnalysisService.AnalyzeAsync(
                filePaths,
                request.CarBrand,
                request.CarModel,
                request.CarYear,
                cancellationToken
            );
        }
        catch (NotCarDetectedException)
        {
            return new ReanalyzeRequestResult { Status = ReanalyzeRequestStatus.NotCarDetected };
        }
        catch (AiServiceUnavailableException ex)
        {
            var isPricingIssue =
                !string.IsNullOrWhiteSpace(ex.Message)
                && ex.Message.Contains("pricing", StringComparison.OrdinalIgnoreCase);

            return new ReanalyzeRequestResult
            {
                Status = ReanalyzeRequestStatus.AiUnavailable,
                IsPricingIssue = isPricingIssue,
            };
        }
        catch (InvalidOperationException)
        {
            return new ReanalyzeRequestResult { Status = ReanalyzeRequestStatus.AiInvalidResponse };
        }

        request.AiIsCar = analysisResult.IsCar;
        request.AiSummary = analysisResult.Summary;
        request.AiEstimatedTotalCost = analysisResult.EstimatedTotalCost;
        request.Status = DamageRequestStatus.AiProcessed;
        request.UpdatedAt = DateTime.UtcNow;

        dbContext.DamageEstimateItems.RemoveRange(request.EstimateItems);

        if (analysisResult.DamageItems.Count > 0)
        {
            var estimateItems = analysisResult.DamageItems.Select(item => new DamageEstimateItem
            {
                Id = Guid.NewGuid(),
                DamageRequestId = request.Id,
                PartName = item.PartName,
                DamageDescription = item.DamageDescription,
                Severity = item.Severity,
                EstimatedCost = item.EstimatedCost,
                Confidence = item.Confidence,
            });

            dbContext.DamageEstimateItems.AddRange(estimateItems);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ReanalyzeRequestResult
        {
            Status = ReanalyzeRequestStatus.Success,
            RequestId = request.Id,
            RequestStatus = request.Status,
            AiSummary = request.AiSummary,
            AiEstimatedTotalCost = request.AiEstimatedTotalCost,
            UpdatedAt = request.UpdatedAt,
        };
    }

    private string ResolveStorageFilePath(DamageRequestPhoto photo)
    {
        if (!string.IsNullOrWhiteSpace(photo.FileName))
        {
            return Path.Combine(environment.ContentRootPath, "storage", photo.FileName);
        }

        var candidate = photo
            .FilePath.Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
        return Path.Combine(environment.ContentRootPath, candidate);
    }
}

public enum ReanalyzeRequestStatus
{
    Success,
    NotFound,
    NoValidImages,
    NotCarDetected,
    AiUnavailable,
    AiInvalidResponse,
}

public sealed class ReanalyzeRequestResult
{
    public ReanalyzeRequestStatus Status { get; init; }

    public bool IsPricingIssue { get; init; }

    public Guid RequestId { get; init; }

    public DamageRequestStatus RequestStatus { get; init; }

    public string AiSummary { get; init; } = string.Empty;

    public decimal AiEstimatedTotalCost { get; init; }

    public DateTime UpdatedAt { get; init; }
}
