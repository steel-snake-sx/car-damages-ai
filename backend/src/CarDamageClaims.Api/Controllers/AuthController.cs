using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CarDamageClaims.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration, AppDbContext dbContext) : ControllerBase
{
    private const int MaxLoginAttemptsPerWindow = 5;
    private static readonly TimeSpan LoginAttemptWindow = TimeSpan.FromMinutes(1);
    private static readonly ConcurrentDictionary<
        string,
        ConcurrentQueue<DateTime>
    > LoginAttemptsByIp = new();

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var lang = LanguageResolver.Resolve(HttpContext);
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (IsLoginRateLimited(clientIp))
        {
            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                new
                {
                    message = lang == AppLanguage.En
                        ? "Too many login attempts. Please try again later."
                        : "Слишком много попыток входа. Повторите позже.",
                }
            );
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = LocalizedMessages.EmailAndPasswordRequired(lang) });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(
            x => x.Email.ToLower() == normalizedEmail,
            cancellationToken
        );
        if (user is null)
        {
            return Unauthorized(new { message = LocalizedMessages.InvalidEmailOrPassword(lang) });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = LocalizedMessages.UserInactive(lang) });
        }

        if (
            string.IsNullOrWhiteSpace(user.PasswordHash)
            || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)
        )
        {
            return Unauthorized(new { message = LocalizedMessages.InvalidEmailOrPassword(lang) });
        }

        var role = user.Role.ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = configuration["Jwt:Key"];
        var expiresMinutes = int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes)
            ? minutes
            : 60;

        if (
            string.IsNullOrWhiteSpace(issuer)
            || string.IsNullOrWhiteSpace(audience)
            || string.IsNullOrWhiteSpace(key)
        )
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = LocalizedMessages.JwtInvalid(lang) }
            );
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return Ok(
            new
            {
                accessToken = token,
                tokenType = "Bearer",
                expiresAtUtc = expires,
                role,
            }
        );
    }

    private static bool IsLoginRateLimited(string clientIp)
    {
        var now = DateTime.UtcNow;
        var attempts = LoginAttemptsByIp.GetOrAdd(clientIp, _ => new ConcurrentQueue<DateTime>());

        while (attempts.TryPeek(out var attemptTime) && now - attemptTime > LoginAttemptWindow)
        {
            attempts.TryDequeue(out _);
        }

        if (attempts.Count >= MaxLoginAttemptsPerWindow)
        {
            return true;
        }

        attempts.Enqueue(now);
        return false;
    }
}
