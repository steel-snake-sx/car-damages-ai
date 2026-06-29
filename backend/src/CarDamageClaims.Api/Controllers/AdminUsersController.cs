using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Localization;
using CarDamageClaims.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserDto request,
        CancellationToken cancellationToken
    )
    {
        var lang = LanguageResolver.Resolve(HttpContext);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var email = request.Email.Trim();
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            return Conflict(new { message = LocalizedMessages.UserExists(lang) });
        }

        if (!TryParseRole(request.Role, out var role))
        {
            return BadRequest(new { message = LocalizedMessages.RoleInvalid(lang) });
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Created($"/api/admin/users/{user.Id}", new { id = user.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserDto request,
        CancellationToken cancellationToken
    )
    {
        var lang = LanguageResolver.Resolve(HttpContext);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = LocalizedMessages.UserNotFound(lang) });
        }

        var email = request.Email.Trim();
        var emailTaken = await dbContext.Users.AnyAsync(
            x => x.Email == email && x.Id != id,
            cancellationToken
        );
        if (emailTaken)
        {
            return Conflict(new { message = LocalizedMessages.UserExists(lang) });
        }

        if (!TryParseRole(request.Role, out var role))
        {
            return BadRequest(new { message = LocalizedMessages.RoleInvalid(lang) });
        }

        user.FirstName = request.FirstName.Trim();
        user.MiddleName = request.MiddleName?.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.Role = role;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { id = user.Id, updatedAt = user.UpdatedAt });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await dbContext
            .Users.AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

        var response = users.Select(user => new
        {
            id = user.Id,
            firstName = user.FirstName,
            middleName = user.MiddleName,
            lastName = user.LastName,
            fullName = string.Join(
                ' ',
                new[] { user.LastName, user.FirstName, user.MiddleName }.Where(part =>
                    !string.IsNullOrWhiteSpace(part)
                )
            ),
            email = user.Email,
            role = user.Role.ToString(),
            isActive = user.IsActive,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt,
        });

        return Ok(response);
    }

    private static bool TryParseRole(string role, out UserRole parsedRole)
    {
        return Enum.TryParse(role, ignoreCase: true, out parsedRole)
            && (parsedRole == UserRole.Admin || parsedRole == UserRole.Manager);
    }
}
