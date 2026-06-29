using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Models;
using CarDamageClaims.Api.Services.ImageAnalysis;
using CarDamageClaims.Api.Services.ImageAnalysis.Exceptions;

namespace CarDamageClaims.Api.Services.Requests;

public class RequestSubmissionService(
    AppDbContext dbContext,
    IImageAnalysisService imageAnalysisService,
    UploadedFileValidator uploadedFileValidator,
    RequestPhotoStorageService requestPhotoStorageService
)
{
    public async Task<RequestSubmissionResult> SubmitAsync(
        CreateDamageRequestDto request,
        List<IFormFile>? files,
        AppLanguage lang,
        CancellationToken cancellationToken
    )
    {
        var validationResult = uploadedFileValidator.Validate(files, lang);
        if (validationResult.Status != UploadedFileValidationStatus.Success)
        {
            return new RequestSubmissionResult
            {
                Status =
                    validationResult.Status == UploadedFileValidationStatus.PayloadTooLarge
                        ? RequestSubmissionStatus.PayloadTooLarge
                        : RequestSubmissionStatus.BadRequest,
                Message = validationResult.Message,
            };
        }

        var now = DateTime.UtcNow;
        var damageRequest = new DamageRequest
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            MiddleName = request.MiddleName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            CarBrand = request.CarBrand.Trim(),
            CarModel = request.CarModel.Trim(),
            CarYear = request.CarYear,
            Status = DamageRequestStatus.New,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var createdPhotos = new List<DamageRequestPhoto>();
        var writtenFilePaths = new List<string>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        try
        {
            var storageResult = await requestPhotoStorageService.SaveAsync(
                damageRequest.Id,
                now,
                validationResult.Files,
                cancellationToken
            );
            createdPhotos = storageResult.CreatedPhotos;
            writtenFilePaths = storageResult.WrittenFilePaths;

            dbContext.DamageRequests.Add(damageRequest);

            if (createdPhotos.Count > 0)
            {
                dbContext.DamageRequestPhotos.AddRange(createdPhotos);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var analysisResult = await imageAnalysisService.AnalyzeAsync(
                writtenFilePaths,
                request.CarBrand,
                request.CarModel,
                request.CarYear,
                cancellationToken
            );

            damageRequest.AiIsCar = analysisResult.IsCar;
            damageRequest.AiSummary = analysisResult.Summary;
            damageRequest.AiEstimatedTotalCost = analysisResult.EstimatedTotalCost;
            damageRequest.Status = DamageRequestStatus.AiProcessed;
            damageRequest.UpdatedAt = DateTime.UtcNow;

            if (analysisResult.DamageItems.Count > 0)
            {
                var estimateItems = analysisResult.DamageItems.Select(item => new DamageEstimateItem
                {
                    Id = Guid.NewGuid(),
                    DamageRequestId = damageRequest.Id,
                    PartName = item.PartName,
                    DamageDescription = item.DamageDescription,
                    Severity = item.Severity,
                    EstimatedCost = item.EstimatedCost,
                    Confidence = item.Confidence,
                });

                dbContext.DamageEstimateItems.AddRange(estimateItems);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (NotCarDetectedException)
        {
            await transaction.RollbackAsync(cancellationToken);
            requestPhotoStorageService.CleanupFiles(writtenFilePaths);

            return new RequestSubmissionResult
            {
                Status = RequestSubmissionStatus.BadRequest,
                Message = LocalizedMessages.NotCarDetected(lang),
            };
        }
        catch (AiServiceUnavailableException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            requestPhotoStorageService.CleanupFiles(writtenFilePaths);

            var isPricingIssue =
                !string.IsNullOrWhiteSpace(ex.Message)
                && ex.Message.Contains("pricing", StringComparison.OrdinalIgnoreCase);

            return new RequestSubmissionResult
            {
                Status = RequestSubmissionStatus.ServiceUnavailable,
                Message = isPricingIssue
                    ? LocalizedMessages.AiPricingUnavailable(lang)
                    : LocalizedMessages.AiTemporarilyUnavailable(lang),
            };
        }
        catch (InvalidOperationException)
        {
            await transaction.RollbackAsync(cancellationToken);
            requestPhotoStorageService.CleanupFiles(writtenFilePaths);

            return new RequestSubmissionResult
            {
                Status = RequestSubmissionStatus.ServiceUnavailable,
                Message = LocalizedMessages.AiInvalidResponse(lang),
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            requestPhotoStorageService.CleanupFiles(writtenFilePaths);
            throw;
        }

        return new RequestSubmissionResult
        {
            Status = RequestSubmissionStatus.Created,
            RequestId = damageRequest.Id,
            RequestStatus = damageRequest.Status,
            CreatedAt = damageRequest.CreatedAt,
        };
    }
}

public enum RequestSubmissionStatus
{
    Created,
    BadRequest,
    PayloadTooLarge,
    ServiceUnavailable,
}

public sealed class RequestSubmissionResult
{
    public RequestSubmissionStatus Status { get; init; }

    public Guid RequestId { get; init; }

    public DamageRequestStatus RequestStatus { get; init; }

    public DateTime CreatedAt { get; init; }

    public string Message { get; init; } = string.Empty;
}
