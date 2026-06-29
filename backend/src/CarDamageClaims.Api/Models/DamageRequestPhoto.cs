namespace CarDamageClaims.Api.Models;

public class DamageRequestPhoto
{
    public Guid Id { get; set; }

    public Guid DamageRequestId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DamageRequest DamageRequest { get; set; } = null!;
}
