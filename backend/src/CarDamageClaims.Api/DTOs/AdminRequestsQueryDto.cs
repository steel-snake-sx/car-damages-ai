namespace CarDamageClaims.Api.DTOs;

public class AdminRequestsQueryDto
{
    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    public int? Page { get; set; }

    public int? PageSize { get; set; }
}
