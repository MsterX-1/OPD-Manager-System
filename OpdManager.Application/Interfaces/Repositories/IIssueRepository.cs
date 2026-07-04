using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IIssueRepository : IBaseRepository<Issue>
{
    Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status);
    Task<PagedResult<Issue>> SearchPagedAsync(
        string? title, string? category,
        IssueStatus? status, int page, int pageSize);
}
