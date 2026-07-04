using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations;

public class IssueService : IIssueService
{
    private readonly IUnitOfWork _uow;
    public IssueService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<Issue>> SearchPagedAsync(
        string? title, string? category,
        IssueStatus? status, int page, int pageSize) =>
        await _uow.Issues.SearchPagedAsync(title, category, status, page, pageSize);

    public async Task<IEnumerable<Issue>> GetAllAsync() =>
        await _uow.Issues.GetAllAsync();

    public async Task<IEnumerable<Issue>> GetOpenAsync() =>
        await _uow.Issues.GetByStatusAsync(IssueStatus.Open);

    public async Task AddAsync(Issue issue)
    {
        await _uow.Issues.AddAsync(issue);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Issue issue)
    {
        _uow.Issues.Update(issue);
        await _uow.SaveChangesAsync();
    }

    public async Task MarkInReviewAsync(int id)
    {
        var issue = await _uow.Issues.GetByIdAsync(id);
        if (issue is null) return;
        issue.Status = IssueStatus.InReview;
        _uow.Issues.Update(issue);
        await _uow.SaveChangesAsync();
    }

    public async Task CloseAsync(int id, string resolution)
    {
        var issue = await _uow.Issues.GetByIdAsync(id);
        if (issue is null) return;
        issue.Status     = IssueStatus.Closed;
        issue.Resolution = resolution;
        issue.ResolvedAt = DateTime.Now;
        _uow.Issues.Update(issue);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var issue = await _uow.Issues.GetByIdAsync(id);
        if (issue is null) return;
        _uow.Issues.Delete(issue);
        await _uow.SaveChangesAsync();
    }
}