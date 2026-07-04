using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpdManager.Api.Hubs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Enums;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly IHubContext<QueueHub> _queueHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public QueueController(
        IQueueService queueService,
        IHubContext<QueueHub> queueHub,
        IHubContext<DashboardHub> dashboardHub)
    {
        _queueService = queueService;
        _queueHub = queueHub;
        _dashboardHub = dashboardHub;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayQueue()
    {
        var queue = await _queueService.GetTodayQueueAsync();
        return Ok(queue);
    }

    [HttpPost("check-in/{appointmentId}")]
    public async Task<IActionResult> CheckIn(int appointmentId)
    {
        try
        {
            var entry = await _queueService.CheckInAsync(appointmentId);

            // Push real-time updates
            await _queueHub.Clients.All.SendAsync("QueueUpdated");
            await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");

            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] QueueStatus status)
    {
        await _queueService.UpdateStatusAsync(id, status);

        // Push real-time updates
        await _queueHub.Clients.All.SendAsync("QueueUpdated");
        await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");

        return Ok(new { message = "Queue status updated." });
    }

    [HttpGet("performance")]
    public async Task<IActionResult> GetDoctorPerformance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var performance = await _queueService.GetDoctorPerformanceAsync(from, to);
        return Ok(performance);
    }
}
