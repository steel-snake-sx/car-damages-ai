using System.ComponentModel.DataAnnotations;

namespace CarDamageClaims.Api.DTOs;

public class UpdateUserDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? Password { get; set; }
}
