using Microsoft.EntityFrameworkCore;
using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class IssueRepository : BaseRepository<Issue>, IIssueRepository
{
    public IssueRepository(AppDbContext context) : base(context) { }

    public new async Task<IEnumerable<Issue>> GetAllAsync() =>
        await DbSet.OrderByDescending(i => i.ReportedAt).ToListAsync();

    public async Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status) =>
        await DbSet.Where(i => i.Status == status)
                   .OrderByDescending(i => i.ReportedAt)
                   .ToListAsync();

    public async Task<PagedResult<Issue>> SearchPagedAsync(
        string? title, string? category,
        IssueStatus? status, int page, int pageSize)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(i => i.Title.Contains(title) ||
                                     i.Description.Contains(title));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.Category == category);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.ReportedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Issue>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
