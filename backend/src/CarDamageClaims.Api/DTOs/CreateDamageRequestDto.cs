using System.ComponentModel.DataAnnotations;

namespace CarDamageClaims.Api.DTOs;

public class CreateDamageRequestDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string MiddleName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string CarBrand { get; set; } = string.Empty;

    [Required]
    public string CarModel { get; set; } = string.Empty;

    [Range(1901, 2100)]
    public int CarYear { get; set; }
}
