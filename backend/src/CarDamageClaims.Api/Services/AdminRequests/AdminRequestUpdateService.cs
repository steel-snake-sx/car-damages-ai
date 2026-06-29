using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class AdminRequestUpdateService(AppDbContext dbContext)
{
    public async Task<UpdateRequestResult> UpdateAsync(
        Guid id,
        UpdateDamageRequestDto update,
        string? approverEmail,
        CancellationToken cancellationToken
    )
    {
        if (!Enum.TryParse<DamageRequestStatus>(update.Status, ignoreCase: true, out var status))
        {
            return new UpdateRequestResult { Status = UpdateRequestStatus.InvalidStatusValue };
        }

        var request = await dbContext.DamageRequests.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
        if (request is null)
        {
            return new UpdateRequestResult { Status = UpdateRequestStatus.NotFound };
        }

        if (status == DamageRequestStatus.Notified)
        {
            return new UpdateRequestResult
            {
                Status = UpdateRequestStatus.NotifiedNotAllowed,
                CurrentStatus = request.Status,
                AttemptedStatus = status,
            };
        }

        if (status != request.Status && !IsAllowedStatusTransitionForUpdate(request.Status, status))
        {
            return new UpdateRequestResult
            {
                Status = UpdateRequestStatus.InvalidTransition,
                CurrentStatus = request.Status,
                AttemptedStatus = status,
            };
        }

        request.FirstName = update.FirstName.Trim();
        request.LastName = update.LastName.Trim();
        request.MiddleName = update.MiddleName.Trim();
        request.Email = update.Email.Trim();
        request.Phone = update.Phone.Trim();
        request.CarBrand = update.CarBrand.Trim();
        request.CarModel = update.CarModel.Trim();
        request.CarYear = update.CarYear;
        request.Status = status;
        request.AdminDecisionComment = string.IsNullOrWhiteSpace(update.AdminDecisionComment)
            ? null
            : update.AdminDecisionComment.Trim();

        if (status == DamageRequestStatus.Approved)
        {
            var approver = await ResolveApproverAsync(approverEmail, cancellationToken);
            if (approver is null)
            {
                return new UpdateRequestResult { Status = UpdateRequestStatus.ApproverMissing };
            }

            request.ApprovedByUserId = approver.Id;
        }

        request.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateRequestResult
        {
            Status = UpdateRequestStatus.Success,
            RequestId = request.Id,
            RequestStatus = request.Status,
            UpdatedAt = request.UpdatedAt,
        };
    }

    private static bool IsAllowedStatusTransitionForUpdate(
        DamageRequestStatus from,
        DamageRequestStatus to
    )
    {
        return (from, to) switch
        {
            (DamageRequestStatus.New, DamageRequestStatus.AiProcessed) => true,
            (DamageRequestStatus.AiProcessed, DamageRequestStatus.Approved) => true,
            (DamageRequestStatus.AiProcessed, DamageRequestStatus.Rejected) => true,
            _ => false,
        };
    }

    private async Task<User?> ResolveApproverAsync(
        string? approverEmail,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(approverEmail))
        {
            return null;
        }

        return await dbContext.Users.FirstOrDefaultAsync(
            x => x.Email == approverEmail,
            cancellationToken
        );
    }
}

public enum UpdateRequestStatus
{
    Success,
    InvalidStatusValue,
    NotFound,
    NotifiedNotAllowed,
    InvalidTransition,
    ApproverMissing,
}

public sealed class UpdateRequestResult
{
    public UpdateRequestStatus Status { get; init; }

    public Guid RequestId { get; init; }

    public DamageRequestStatus RequestStatus { get; init; }

    public DateTime UpdatedAt { get; init; }

    public DamageRequestStatus CurrentStatus { get; init; }

    public DamageRequestStatus AttemptedStatus { get; init; }
}
