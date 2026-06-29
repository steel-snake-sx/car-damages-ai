using CarDamageClaims.Api.Models;

namespace CarDamageClaims.Api.Services.Requests;

public class RequestPhotoStorageService(IWebHostEnvironment environment)
{
    public async Task<RequestPhotoStorageResult> SaveAsync(
        Guid damageRequestId,
        DateTime createdAt,
        List<IFormFile> files,
        CancellationToken cancellationToken
    )
    {
        var storageFolderPath = Path.Combine(environment.ContentRootPath, "storage");
        Directory.CreateDirectory(storageFolderPath);

        var createdPhotos = new List<DamageRequestPhoto>();
        var writtenFilePaths = new List<string>();

        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = file.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase)
                    ? ".png"
                    : ".jpg";
            }

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var absoluteFilePath = Path.Combine(storageFolderPath, storedFileName);

            await using (var stream = System.IO.File.Create(absoluteFilePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            writtenFilePaths.Add(absoluteFilePath);

            createdPhotos.Add(
                new DamageRequestPhoto
                {
                    Id = Guid.NewGuid(),
                    DamageRequestId = damageRequestId,
                    FileName = storedFileName,
                    FilePath = $"/storage/{storedFileName}",
                    SortOrder = index,
                    CreatedAt = createdAt,
                }
            );
        }

        return new RequestPhotoStorageResult
        {
            CreatedPhotos = createdPhotos,
            WrittenFilePaths = writtenFilePaths,
        };
    }

    public void CleanupFiles(List<string> writtenFilePaths)
    {
        foreach (var writtenFilePath in writtenFilePaths)
        {
            try
            {
                if (System.IO.File.Exists(writtenFilePath))
                {
                    System.IO.File.Delete(writtenFilePath);
                }
            }
            catch
            {
            }
        }
    }
}

public sealed class RequestPhotoStorageResult
{
    public List<DamageRequestPhoto> CreatedPhotos { get; init; } = [];

    public List<string> WrittenFilePaths { get; init; } = [];
}
