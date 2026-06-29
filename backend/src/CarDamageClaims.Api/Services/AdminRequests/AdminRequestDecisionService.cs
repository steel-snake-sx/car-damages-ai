using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.Models;
using CarDamageClaims.Api.Services.Email;
using CarDamageClaims.Api.Services.Email.Templates;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class AdminRequestDecisionService(
    AppDbContext dbContext,
    IEmailService emailService,
    ILogger<AdminRequestDecisionService> logger
)
{
    public async Task<ApproveRequestResult> ApproveAsync(
        Guid id,
        string? approverEmail,
        CancellationToken cancellationToken
    )
    {
        var request = await dbContext
            .DamageRequests.Include(x => x.EstimateItems)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null)
        {
            return new ApproveRequestResult { Status = ApproveRequestStatus.NotFound };
        }

        if (request.Status != DamageRequestStatus.AiProcessed)
        {
            return new ApproveRequestResult { Status = ApproveRequestStatus.InvalidStatus };
        }

        var approver = await ResolveApproverAsync(approverEmail, cancellationToken);
        if (approver is null)
        {
            return new ApproveRequestResult { Status = ApproveRequestStatus.ApproverMissing };
        }

        request.Status = DamageRequestStatus.Approved;
        request.ApprovedByUserId = null;
        request.UpdatedAt = DateTime.UtcNow;

        var approvalMessage = RequestDecisionEmail.BuildApprovalMessage(request);

        var notification = new NotificationOutbox
        {
            Id = Guid.NewGuid(),
            DamageRequestId = request.Id,
            RecipientEmail = request.Email,
            Subject = approvalMessage.Subject,
            NotificationType = NotificationType.ApprovalEmail,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.NotificationOutbox.Add(notification);

        try
        {
            var emailResult = await emailService.SendAsync(approvalMessage, cancellationToken);

            if (emailResult.Success)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = emailResult.OccurredAtUtc;
                request.Status = DamageRequestStatus.Notified;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = emailResult.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ошибка отправки email при одобрении заявки {RequestId}",
                request.Id
            );
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
        }

        request.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ApproveRequestResult
        {
            Status = ApproveRequestStatus.Success,
            RequestId = request.Id,
            RequestStatus = request.Status,
            ApprovedByUserId = request.ApprovedByUserId,
            NotificationStatus = notification.Status,
            UpdatedAt = request.UpdatedAt,
        };
    }

    public async Task<RejectRequestResult> RejectAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await dbContext.DamageRequests.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken
        );
        if (request is null)
        {
            return new RejectRequestResult { Status = RejectRequestStatus.NotFound };
        }

        if (request.Status != DamageRequestStatus.AiProcessed)
        {
            return new RejectRequestResult { Status = RejectRequestStatus.InvalidStatus };
        }

        request.Status = DamageRequestStatus.Rejected;
        request.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RejectRequestResult
        {
            Status = RejectRequestStatus.Success,
            RequestId = request.Id,
            RequestStatus = request.Status,
            UpdatedAt = request.UpdatedAt,
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

public enum ApproveRequestStatus
{
    Success,
    NotFound,
    InvalidStatus,
    ApproverMissing,
}

public sealed class ApproveRequestResult
{
    public ApproveRequestStatus Status { get; init; }

    public Guid RequestId { get; init; }

    public DamageRequestStatus RequestStatus { get; init; }

    public Guid? ApprovedByUserId { get; init; }

    public NotificationStatus NotificationStatus { get; init; }

    public DateTime UpdatedAt { get; init; }
}

public enum RejectRequestStatus
{
    Success,
    NotFound,
    InvalidStatus,
}

public sealed class RejectRequestResult
{
    public RejectRequestStatus Status { get; init; }

    public Guid RequestId { get; init; }

    public DamageRequestStatus RequestStatus { get; init; }

    public DateTime UpdatedAt { get; init; }
}
