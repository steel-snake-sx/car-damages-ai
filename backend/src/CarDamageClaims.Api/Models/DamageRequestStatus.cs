namespace CarDamageClaims.Api.Models;

public enum DamageRequestStatus
{
    New = 1,
    AiProcessed = 2,
    Approved = 3,
    Rejected = 4,
    Notified = 5,
}
