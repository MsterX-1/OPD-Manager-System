using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

/// <summary>
/// Doctor-scoped endpoints — all resolved from JWT claims, no doctorId in URL.
/// </summary>
[ApiController]
[Route("api/doctors/me")]
[Authorize(Roles = "Doctor")]
public class DoctorPortalController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IQueueService _queueService;
    private readonly IReferralService _referralService;

    public DoctorPortalController(
        IAppointmentService appointmentService,
        IQueueService queueService,
        IReferralService referralService)
    {
        _appointmentService = appointmentService;
        _queueService = queueService;
        _referralService = referralService;
    }

    private int GetDoctorId()
    {
        var claim = User.FindFirstValue("DoctorId");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var doctorId))
            throw new UnauthorizedAccessException("User is not linked to a doctor record.");
        return doctorId;
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetMyAppointments(
        [FromQuery] DateTime? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var doctorId = GetDoctorId();
        var targetDate = date ?? DateTime.Today;
        var result = await _appointmentService.GetByDatePagedAsync(targetDate, page, pageSize, doctorId);
        return Ok(result);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetMyQueue()
    {
        var doctorId = GetDoctorId();
        // Get today's queue filtered by this doctor
        var allQueue = await _queueService.GetTodayQueueAsync();
        var myQueue = allQueue.Where(q => q.DoctorId == doctorId);
        return Ok(myQueue);
    }

    [HttpGet("referrals")]
    public async Task<IActionResult> GetMyReferrals()
    {
        var doctorId = GetDoctorId();
        var created = await _referralService.GetByReferringDoctorAsync(doctorId);
        var received = await _referralService.GetByTargetDoctorAsync(doctorId);
        return Ok(new { created, received });
    }

    [HttpGet("performance")]
    public async Task<IActionResult> GetMyPerformance([FromQuery] DateTime? date)
    {
        var doctorId = GetDoctorId();
        var targetDate = date ?? DateTime.Today;
        var performance = await _queueService.GetDoctorPerformanceAsync(targetDate, targetDate, doctorId);
        return Ok(performance);
    }
}
