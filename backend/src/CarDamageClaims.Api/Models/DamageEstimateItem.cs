namespace CarDamageClaims.Api.Models;

public class DamageEstimateItem
{
    public Guid Id { get; set; }

    public Guid DamageRequestId { get; set; }

    public string PartName { get; set; } = string.Empty;

    public string DamageDescription { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public decimal EstimatedCost { get; set; }

    public double Confidence { get; set; }

    public DamageRequest DamageRequest { get; set; } = null!;
}
