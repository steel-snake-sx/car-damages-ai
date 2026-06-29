using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarDamageClaims.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController(RequestSubmissionService requestSubmissionService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateDamageRequestDto request,
        [FromForm] List<IFormFile>? files
    )
    {
        var lang = LanguageResolver.Resolve(HttpContext);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await requestSubmissionService.SubmitAsync(
            request,
            files,
            lang,
            HttpContext.RequestAborted
        );

        if (result.Status == RequestSubmissionStatus.BadRequest)
        {
            return BadRequest(new { message = result.Message });
        }

        if (result.Status == RequestSubmissionStatus.PayloadTooLarge)
        {
            return StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new { message = result.Message }
            );
        }

        if (result.Status == RequestSubmissionStatus.ServiceUnavailable)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = result.Message }
            );
        }

        return Created(
            $"/api/requests/{result.RequestId}",
            new
            {
                id = result.RequestId,
                status = result.RequestStatus,
                createdAt = result.CreatedAt,
            }
        );
    }
}
