using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Interfaces;

public interface IIssueService
{
    Task<PagedResult<Issue>> SearchPagedAsync(
        string? title, string? category,
        IssueStatus? status, int page, int pageSize);
    Task<IEnumerable<Issue>> GetAllAsync();
    Task<IEnumerable<Issue>> GetOpenAsync();
    Task AddAsync(Issue issue);
    Task UpdateAsync(Issue issue);
    Task MarkInReviewAsync(int id);
    Task CloseAsync(int id, string resolution);
    Task DeleteAsync(int id);
}
