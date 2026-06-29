using CarDamageClaims.Api.Localization;

namespace CarDamageClaims.Api.Services.Requests;

public class UploadedFileValidator
{
    private const long MaxSingleFileBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/jpg",
    };

    private static readonly HashSet<string> AllowedExtensions = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    };

    public UploadedFileValidationResult Validate(List<IFormFile>? files, AppLanguage lang)
    {
        files ??= new List<IFormFile>();

        if (files.Count < 1)
        {
            return UploadedFileValidationResult.BadRequest(LocalizedMessages.AtLeastOneImage(lang));
        }

        if (files.Count > 3)
        {
            return UploadedFileValidationResult.BadRequest(LocalizedMessages.MaxThreeFiles(lang));
        }

        foreach (var file in files)
        {
            if (file.Length > MaxSingleFileBytes)
            {
                return UploadedFileValidationResult.PayloadTooLarge(
                    lang == AppLanguage.En
                        ? "Each uploaded file must be 10 MB or less."
                        : "Каждый загружаемый файл должен быть не больше 10 МБ."
                );
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                return UploadedFileValidationResult.BadRequest(
                    LocalizedMessages.InvalidImageExtension(lang)
                );
            }

            var contentType = file.ContentType?.Trim() ?? string.Empty;
            if (!AllowedContentTypes.Contains(contentType))
            {
                return UploadedFileValidationResult.BadRequest(
                    LocalizedMessages.InvalidImageTypes(lang)
                );
            }
        }

        return UploadedFileValidationResult.Success(files);
    }
}

public enum UploadedFileValidationStatus
{
    Success,
    BadRequest,
    PayloadTooLarge,
}

public sealed class UploadedFileValidationResult
{
    public UploadedFileValidationStatus Status { get; init; }

    public string Message { get; init; } = string.Empty;

    public List<IFormFile> Files { get; init; } = [];

    public static UploadedFileValidationResult Success(List<IFormFile> files) =>
        new() { Status = UploadedFileValidationStatus.Success, Files = files };

    public static UploadedFileValidationResult BadRequest(string message) =>
        new() { Status = UploadedFileValidationStatus.BadRequest, Message = message };

    public static UploadedFileValidationResult PayloadTooLarge(string message) =>
        new() { Status = UploadedFileValidationStatus.PayloadTooLarge, Message = message };
}
