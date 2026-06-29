namespace CarDamageClaims.Api.Models;

public class DamageRequest
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DamageRequestStatus Status { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string CarBrand { get; set; } = string.Empty;

    public string CarModel { get; set; } = string.Empty;

    public int CarYear { get; set; }

    public bool AiIsCar { get; set; }

    public string AiSummary { get; set; } = string.Empty;

    public decimal AiEstimatedTotalCost { get; set; }

    public string? AdminDecisionComment { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public User? ApprovedByUser { get; set; }

    public ICollection<DamageRequestPhoto> Photos { get; set; } = new List<DamageRequestPhoto>();

    public ICollection<DamageEstimateItem> EstimateItems { get; set; } =
        new List<DamageEstimateItem>();

    public ICollection<NotificationOutbox> Notifications { get; set; } =
        new List<NotificationOutbox>();
}
