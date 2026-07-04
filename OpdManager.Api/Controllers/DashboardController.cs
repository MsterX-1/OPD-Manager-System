using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IQueueService _queueService;

    public DashboardController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _queueService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
