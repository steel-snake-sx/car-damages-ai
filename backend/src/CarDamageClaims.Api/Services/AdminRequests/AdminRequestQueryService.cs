using CarDamageClaims.Api.Data;
using CarDamageClaims.Api.DTOs;
using CarDamageClaims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Services.AdminRequests;

public class AdminRequestQueryService(AppDbContext dbContext, AdminRequestPresenter adminRequestPresenter)
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public async Task<AdminRequestListResult> GetAllAsync(
        AdminRequestsQueryDto query,
        CancellationToken cancellationToken
    )
    {
        IQueryable<DamageRequest> requestsQuery = dbContext.DamageRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var searchPattern = $"%{search}%";

            requestsQuery = requestsQuery.Where(x =>
                EF.Functions.Like(x.FirstName, searchPattern)
                || EF.Functions.Like(x.LastName, searchPattern)
                || EF.Functions.Like(x.MiddleName, searchPattern)
                || EF.Functions.Like(
                    (x.LastName + " " + x.FirstName + " " + x.MiddleName).Trim(),
                    searchPattern
                )
                || EF.Functions.Like(x.Email, searchPattern)
                || EF.Functions.Like(x.Phone, searchPattern)
                || EF.Functions.Like(x.CarBrand, searchPattern)
                || EF.Functions.Like(x.CarModel, searchPattern)
                || EF.Functions.Like((x.CarBrand + " " + x.CarModel).Trim(), searchPattern)
            );
        }

        var sortBy = query.SortBy?.Trim().ToLowerInvariant();
        var desc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        requestsQuery = sortBy switch
        {
            "status" => desc
                ? requestsQuery.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedAt)
                : requestsQuery.OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAt),
            "customer" => desc
                ? requestsQuery
                    .OrderByDescending(x => x.LastName)
                    .ThenByDescending(x => x.FirstName)
                : requestsQuery.OrderBy(x => x.LastName).ThenBy(x => x.FirstName),
            "car" => desc
                ? requestsQuery.OrderByDescending(x => x.CarBrand).ThenByDescending(x => x.CarModel)
                : requestsQuery.OrderBy(x => x.CarBrand).ThenBy(x => x.CarModel),
            "createdat" => desc
                ? requestsQuery.OrderByDescending(x => x.CreatedAt)
                : requestsQuery.OrderBy(x => x.CreatedAt),
            _ => requestsQuery.OrderByDescending(x => x.CreatedAt),
        };

        var page = query.Page.GetValueOrDefault(DefaultPage);
        if (page < 1)
        {
            page = DefaultPage;
        }

        var pageSize = query.PageSize.GetValueOrDefault(DefaultPageSize);
        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        var totalCount = await requestsQuery.CountAsync(cancellationToken);
        var skip = (page - 1) * pageSize;

        var pageRequestIds = await requestsQuery
            .Skip(skip)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var items = await dbContext
            .DamageRequests.AsNoTracking()
            .Where(x => pageRequestIds.Contains(x.Id))
            .Include(x => x.Photos)
            .Include(x => x.EstimateItems)
            .Include(x => x.Notifications)
            .Include(x => x.ApprovedByUser)
            .ToListAsync(cancellationToken);

        var orderMap = pageRequestIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        items = items.OrderBy(x => orderMap[x.Id]).ToList();

        return new AdminRequestListResult
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(adminRequestPresenter.MapResponse).ToList(),
        };
    }

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await dbContext
            .DamageRequests.AsNoTracking()
            .Include(x => x.Photos)
            .Include(x => x.EstimateItems)
            .Include(x => x.Notifications)
            .Include(x => x.ApprovedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return request is null ? null : adminRequestPresenter.MapResponse(request);
    }

    public async Task<List<EmailHistoryItemDto>> GetEmailHistoryAsync(
        CancellationToken cancellationToken
    )
    {
        var historyRows = await dbContext
            .NotificationOutbox.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(item => new
            {
                id = item.Id,
                requestId = item.DamageRequestId,
                recipientEmail = item.RecipientEmail,
                subject = item.Subject,
                status = item.Status.ToString(),
                sentAt = item.SentAt,
                errorMessage = item.ErrorMessage,
                createdAt = item.CreatedAt,
                firstName = item.DamageRequest.FirstName,
                lastName = item.DamageRequest.LastName,
                middleName = item.DamageRequest.MiddleName,
            })
            .ToListAsync(cancellationToken);

        return historyRows
            .Select(item => new EmailHistoryItemDto
            {
                Id = item.id,
                RequestId = item.requestId,
                RecipientEmail = item.recipientEmail,
                Subject = item.subject,
                Status = item.status,
                SentAt = item.sentAt,
                ErrorMessage = item.errorMessage,
                CreatedAt = item.createdAt,
                FullName = adminRequestPresenter.FormatFullName(
                    item.lastName,
                    item.firstName,
                    item.middleName
                ),
            })
            .ToList();
    }
}

public sealed class AdminRequestListResult
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public IReadOnlyList<object> Items { get; init; } = [];
}
