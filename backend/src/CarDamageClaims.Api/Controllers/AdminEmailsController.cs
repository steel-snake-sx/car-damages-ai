using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.Models;
using CarDamageClaims.Api.Services;
using CarDamageClaims.Api.Services.Email;
using CarDamageClaims.Api.Services.Email.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Controllers;

[ApiController]
[Route("api/admin/emails")]
[Authorize(Policy = "AdminOrManager")]
public class AdminEmailsController(
    AppDbContext dbContext,
    IEmailService emailService,
    ILogger<AdminEmailsController> logger
) : ControllerBase
{
    [HttpPost("{id:guid}/resend")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Resend(Guid id, CancellationToken cancellationToken)
    {
        var notification = await dbContext
            .NotificationOutbox.Include(x => x.DamageRequest)
                .ThenInclude(x => x.EstimateItems)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (notification is null)
        {
            return NotFound(new { message = "Email record not found." });
        }

        if (notification.Status != NotificationStatus.Failed)
        {
            return Conflict(new { message = "Only failed emails can be resent." });
        }

        var message = BuildMessage(notification);
        if (message is null)
        {
            return BadRequest(
                new { message = "Unable to rebuild email message for this notification type." }
            );
        }

        notification.Status = NotificationStatus.Pending;
        notification.ErrorMessage = null;

        try
        {
            var sendResult = await emailService.SendAsync(message, cancellationToken);
            if (sendResult.Success)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = sendResult.OccurredAtUtc;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = sendResult.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ошибка повторной отправки email {NotificationId}",
                notification.Id
            );
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(
            new
            {
                id = notification.Id,
                requestId = notification.DamageRequestId,
                status = notification.Status.ToString(),
                sentAt = notification.SentAt,
                errorMessage = notification.ErrorMessage,
            }
        );
    }

    private static EmailMessage? BuildMessage(NotificationOutbox notification)
    {
        return notification.NotificationType switch
        {
            NotificationType.ApprovalEmail =>
                RequestDecisionEmail.BuildApprovalMessage(
                    notification.DamageRequest
                ),
            NotificationType.RejectionEmail =>
                RequestDecisionEmail.BuildRejectionMessage(
                    notification.DamageRequest
                ),
            _ => null,
        };
    }
}
