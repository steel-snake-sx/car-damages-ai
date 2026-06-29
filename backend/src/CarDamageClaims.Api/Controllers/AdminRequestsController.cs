using System.Security.Claims;
using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Services.AdminRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarDamageClaims.Api.Controllers;

[ApiController]
[Route("api/admin/requests")]
[Authorize(Policy = "AdminOrManager")]
public class AdminRequestsController(
    AdminRequestQueryService adminRequestQueryService,
    AdminRequestUpdateService adminRequestUpdateService,
    AdminRequestDecisionService adminRequestDecisionService,
    AdminRequestReanalysisService adminRequestReanalysisService,
    AdminRequestExportService adminRequestExportService,
    ILogger<AdminRequestsController> logger
) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll(
        [FromQuery] AdminRequestsQueryDto query,
        CancellationToken cancellationToken
    )
    {
        var result = await adminRequestQueryService.GetAllAsync(query, cancellationToken);

        return Ok(
            new
            {
                page = result.Page,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                items = result.Items,
            }
        );
    }

    [HttpGet("notifications/history")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetEmailHistory(CancellationToken cancellationToken)
    {
        var history = await adminRequestQueryService.GetEmailHistoryAsync(cancellationToken);
        return Ok(history);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var request = await adminRequestQueryService.GetByIdAsync(id, cancellationToken);

        if (request is null)
        {
            return NotFound(new { message = LocalizedMessages.DamageRequestNotFound(lang) });
        }

        return Ok(request);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var approverEmail =
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value;

        var result = await adminRequestDecisionService.ApproveAsync(
            id,
            approverEmail,
            cancellationToken
        );

        if (result.Status == ApproveRequestStatus.NotFound)
        {
            return NotFound(new { message = LocalizedMessages.DamageRequestNotFound(lang) });
        }

        if (result.Status == ApproveRequestStatus.InvalidStatus)
        {
            return Conflict(new { message = LocalizedMessages.OnlyAiProcessedApprove(lang) });
        }

        if (result.Status == ApproveRequestStatus.ApproverMissing)
        {
            return Unauthorized(new { message = LocalizedMessages.ApproverMissing(lang) });
        }

        return Ok(
            new
            {
                id = result.RequestId,
                status = result.RequestStatus.ToString(),
                approvedByUserId = result.ApprovedByUserId,
                notificationStatus = result.NotificationStatus.ToString(),
                updatedAt = result.UpdatedAt,
            }
        );
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var result = await adminRequestDecisionService.RejectAsync(id, cancellationToken);
        if (result.Status == RejectRequestStatus.NotFound)
        {
            return NotFound(new { message = LocalizedMessages.DamageRequestNotFound(lang) });
        }

        if (result.Status == RejectRequestStatus.InvalidStatus)
        {
            return Conflict(new { message = LocalizedMessages.OnlyAiProcessedReject(lang) });
        }

        return Ok(
            new
            {
                id = result.RequestId,
                status = result.RequestStatus.ToString(),
                updatedAt = result.UpdatedAt,
            }
        );
    }

    [HttpPost("{id:guid}/reanalyze")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reanalyze(Guid id, CancellationToken cancellationToken)
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var result = await adminRequestReanalysisService.ReanalyzeAsync(id, cancellationToken);

        if (result.Status == ReanalyzeRequestStatus.NotFound)
        {
            return NotFound(new { message = LocalizedMessages.DamageRequestNotFound(lang) });
        }

        if (result.Status == ReanalyzeRequestStatus.NoValidImages)
        {
            return BadRequest(new { message = LocalizedMessages.NoValidImagesForReanalysis(lang) });
        }

        if (result.Status == ReanalyzeRequestStatus.NotCarDetected)
        {
            return BadRequest(new { message = LocalizedMessages.NotCarDetected(lang) });
        }

        if (result.Status == ReanalyzeRequestStatus.AiUnavailable)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = result.IsPricingIssue
                        ? LocalizedMessages.AiPricingUnavailable(lang)
                        : LocalizedMessages.AiTemporarilyUnavailable(lang),
                }
            );
        }

        if (result.Status == ReanalyzeRequestStatus.AiInvalidResponse)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = LocalizedMessages.AiInvalidResponse(lang) }
            );
        }

        return Ok(
            new
            {
                id = result.RequestId,
                status = result.RequestStatus.ToString(),
                aiSummary = result.AiSummary,
                aiEstimatedTotalCost = result.AiEstimatedTotalCost,
                updatedAt = result.UpdatedAt,
            }
        );
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDamageRequestDto update,
        CancellationToken cancellationToken
    )
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var approverEmail =
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirst("email")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value;

        var result = await adminRequestUpdateService.UpdateAsync(
            id,
            update,
            approverEmail,
            cancellationToken
        );

        if (result.Status == UpdateRequestStatus.InvalidStatusValue)
        {
            return BadRequest(new { message = LocalizedMessages.StatusInvalid(lang) });
        }

        if (result.Status == UpdateRequestStatus.NotFound)
        {
            return NotFound(new { message = LocalizedMessages.DamageRequestNotFound(lang) });
        }

        if (result.Status == UpdateRequestStatus.NotifiedNotAllowed)
        {
            return BadRequest(
                new
                {
                    message = "Status 'Notified' cannot be set via update endpoint. It is set only by approve/email flow.",
                    currentStatus = result.CurrentStatus.ToString(),
                    attemptedStatus = result.AttemptedStatus.ToString(),
                }
            );
        }

        if (result.Status == UpdateRequestStatus.InvalidTransition)
        {
            return BadRequest(
                new
                {
                    message = "Invalid status transition.",
                    currentStatus = result.CurrentStatus.ToString(),
                    attemptedStatus = result.AttemptedStatus.ToString(),
                    allowedTransitions = new[]
                    {
                        "New -> AiProcessed",
                        "AiProcessed -> Approved",
                        "AiProcessed -> Rejected",
                    },
                }
            );
        }

        if (result.Status == UpdateRequestStatus.ApproverMissing)
        {
            return Unauthorized(new { message = LocalizedMessages.ApproverMissing(lang) });
        }

        return Ok(
            new
            {
                id = result.RequestId,
                status = result.RequestStatus.ToString(),
                updatedAt = result.UpdatedAt,
            }
        );
    }

    [HttpGet("{id:guid}/export")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Export(Guid id, CancellationToken cancellationToken)
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var exportResult = await adminRequestExportService.ExportRequestAsync(
            id,
            lang,
            cancellationToken
        );

        if (exportResult is null)
        {
            return NotFound(new { message = LocalizedMessages.DamageRequestNotFound(lang) });
        }

        if (exportResult.FileBytes.Length == 0 || !exportResult.HasZipSignature)
        {
            logger.LogError(
                "DOCX export failed integrity check. RequestId={RequestId}, Length={Length}, ZipSignature={ZipSignature}",
                exportResult.RequestId,
                exportResult.FileBytes.Length,
                exportResult.HasZipSignature
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = LocalizedMessages.DocxIntegrityFailed(lang) }
            );
        }

        return File(exportResult.FileBytes, exportResult.ContentType, exportResult.FileName);
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ExportAll(CancellationToken cancellationToken)
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var exportResult = await adminRequestExportService.ExportAllAsync(lang, cancellationToken);

        return File(exportResult.FileBytes, exportResult.ContentType, exportResult.FileName);
    }
}
