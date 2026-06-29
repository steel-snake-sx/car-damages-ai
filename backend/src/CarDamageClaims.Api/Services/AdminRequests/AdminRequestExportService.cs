using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.Localization;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class AdminRequestExportService(
    AppDbContext dbContext,
    RequestDocxReportWriter requestDocxReportWriter,
    RequestListExcelWriter requestListExcelWriter,
    IWebHostEnvironment environment,
    ILogger<AdminRequestExportService> logger
)
{
    public async Task<SingleRequestExportResult?> ExportRequestAsync(
        Guid id,
        AppLanguage lang,
        CancellationToken cancellationToken
    )
    {
        var request = await dbContext
            .DamageRequests.AsNoTracking()
            .Include(x => x.Photos)
            .Include(x => x.EstimateItems)
            .Include(x => x.ApprovedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return null;
        }

        var images = request
            .Photos.OrderBy(x => x.SortOrder)
            .Take(3)
            .Select(photo => ResolveStorageFilePath(photo))
            .Where(path => System.IO.File.Exists(path))
            .ToList();

        var fileBytes = requestDocxReportWriter.Build(
            request,
            lang,
            images,
            logger,
            out var hasZipSignature
        );
        var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var fileName = requestDocxReportWriter.BuildWordFileName(request);

        logger.LogInformation(
            "DOCX export generated. RequestId={RequestId}, FileName={FileName}, Length={Length}, ZipSignature={ZipSignature}, ContentType={ContentType}",
            request.Id,
            fileName,
            fileBytes.Length,
            hasZipSignature,
            contentType
        );

        return new SingleRequestExportResult
        {
            RequestId = request.Id,
            FileBytes = fileBytes,
            FileName = fileName,
            ContentType = contentType,
            HasZipSignature = hasZipSignature,
        };
    }

    public async Task<AllRequestsExportResult> ExportAllAsync(
        AppLanguage lang,
        CancellationToken cancellationToken
    )
    {
        var requests = await dbContext
            .DamageRequests.AsNoTracking()
            .Include(x => x.ApprovedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var fileBytes = requestListExcelWriter.BuildAll(requests, lang);

        return new AllRequestsExportResult
        {
            FileBytes = fileBytes,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = "requests_export.xlsx",
        };
    }

    private string ResolveStorageFilePath(Models.DamageRequestPhoto photo)
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

public sealed class SingleRequestExportResult
{
    public Guid RequestId { get; init; }

    public byte[] FileBytes { get; init; } = [];

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public bool HasZipSignature { get; init; }
}

public sealed class AllRequestsExportResult
{
    public byte[] FileBytes { get; init; } = [];

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;
}
