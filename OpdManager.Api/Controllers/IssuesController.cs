using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _issueService;

    public IssuesController(IIssueService issueService)
    {
        _issueService = issueService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? title,
        [FromQuery] string? category,
        [FromQuery] IssueStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _issueService.SearchPagedAsync(title, category, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen()
    {
        var issues = await _issueService.GetOpenAsync();
        return Ok(issues);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var issues = await _issueService.GetAllAsync();
        var issue = issues.FirstOrDefault(i => i.Id == id);
        if (issue is null) return NotFound();
        return Ok(issue);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IssueRequest request)
    {
        var issue = new Issue
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            AppointmentId = request.AppointmentId,
            ReportedAt = DateTime.UtcNow
        };

        await _issueService.AddAsync(issue);
        return CreatedAtAction(nameof(GetById), new { id = issue.Id }, issue);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] IssueRequest request)
    {
        var issues = await _issueService.GetAllAsync();
        var issue = issues.FirstOrDefault(i => i.Id == id);
        if (issue is null) return NotFound();

        issue.Title = request.Title;
        issue.Description = request.Description;
        issue.Category = request.Category;
        issue.AppointmentId = request.AppointmentId;

        await _issueService.UpdateAsync(issue);
        return Ok(issue);
    }

    [HttpPatch("{id}/in-review")]
    public async Task<IActionResult> MarkInReview(int id)
    {
        await _issueService.MarkInReviewAsync(id);
        return Ok(new { message = "Issue marked as In Review." });
    }

    [HttpPatch("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseIssueRequest request)
    {
        await _issueService.CloseAsync(id, request.Resolution);
        return Ok(new { message = "Issue closed." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _issueService.DeleteAsync(id);
        return NoContent();
    }
}
