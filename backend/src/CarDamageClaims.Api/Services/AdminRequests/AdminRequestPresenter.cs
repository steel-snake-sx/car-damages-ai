using CarDamageClaims.Api.Models;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class AdminRequestPresenter
{
    public object MapResponse(DamageRequest request)
    {
        return new
        {
            id = request.Id,
            createdAt = request.CreatedAt,
            updatedAt = request.UpdatedAt,
            status = request.Status.ToString(),
            firstName = request.FirstName,
            lastName = request.LastName,
            middleName = request.MiddleName,
            fullName = FormatClientFullName(request),
            email = request.Email,
            phone = request.Phone,
            carBrand = request.CarBrand,
            carModel = request.CarModel,
            carYear = request.CarYear,
            aiIsCar = request.AiIsCar,
            aiSummary = request.AiSummary,
            aiEstimatedTotalCost = request.AiEstimatedTotalCost,
            adminDecisionComment = request.AdminDecisionComment,
            approvedByUserId = request.ApprovedByUserId,
            approvedByFullName = FormatUserFullName(request.ApprovedByUser),
            photos = request
                .Photos.OrderBy(x => x.SortOrder)
                .Select(photo => new
                {
                    id = photo.Id,
                    fileName = photo.FileName,
                    filePath = ToPublicStoragePath(photo.FilePath, photo.FileName),
                    sortOrder = photo.SortOrder,
                    createdAt = photo.CreatedAt,
                }),
            estimateItems = request.EstimateItems.Select(item => new
            {
                id = item.Id,
                partName = item.PartName,
                damageDescription = item.DamageDescription,
                severity = item.Severity,
                estimatedCost = item.EstimatedCost,
                confidence = item.Confidence,
            }),
            notifications = request
                .Notifications.OrderByDescending(x => x.CreatedAt)
                .Select(notification => new
                {
                    id = notification.Id,
                    recipientEmail = notification.RecipientEmail,
                    notificationType = notification.NotificationType.ToString(),
                    subject = notification.Subject,
                    status = notification.Status.ToString(),
                    sentAt = notification.SentAt,
                    errorMessage = notification.ErrorMessage,
                    createdAt = notification.CreatedAt,
                }),
        };
    }

    public string FormatFullName(string? lastName, string? firstName, string? middleName)
    {
        var parts = new[] { lastName, firstName, middleName }.Where(part =>
            !string.IsNullOrWhiteSpace(part)
        );

        return string.Join(' ', parts);
    }

    private string FormatClientFullName(DamageRequest request)
    {
        return FormatFullName(request.LastName, request.FirstName, request.MiddleName);
    }

    private string? FormatUserFullName(User? user)
    {
        if (user is null)
        {
            return null;
        }

        return FormatFullName(user.LastName, user.FirstName, user.MiddleName);
    }

    private static string ToPublicStoragePath(string filePath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return $"/storage/{fileName}";
        }

        var normalized = filePath.Replace('\\', '/');
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        return normalized;
    }
}
