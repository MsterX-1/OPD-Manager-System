using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpdManager.Api.Hubs;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Exposes endpoints for cross-department/doctor patient referrals and laboratory results tracking.
/// </summary>
public class ReferralsController : ControllerBase
{
    private readonly IReferralService _referralService;
    private readonly IHubContext<DepartmentHub> _departmentHub;

    public ReferralsController(
        IReferralService referralService,
        IHubContext<DepartmentHub> departmentHub)
    {
        _referralService = referralService;
        _departmentHub = departmentHub;
    }

    private int? GetDoctorId()
    {
        var claim = User.FindFirstValue("DoctorId");
        return string.IsNullOrEmpty(claim) ? null : int.TryParse(claim, out var id) ? id : null;
    }

    private int? GetDepartmentId()
    {
        var claim = User.FindFirstValue("DepartmentId");
        return string.IsNullOrEmpty(claim) ? null : int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Create a referral — Doctor only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateReferralRequest request)
    {
        var doctorId = GetDoctorId();
        if (doctorId is null && !User.IsInRole("Admin"))
            return Forbid("User is not linked to a doctor record.");

        try
        {
            var result = await _referralService.CreateAsync(
                request.SourceAppointmentId,
                doctorId ?? 0,
                request.ReferralType,
                request.TargetDoctorId,
                request.TargetDepartmentId,
                request.TargetDepartmentServiceId,
                request.Reason);

            // Notify the target department via SignalR
            if (request.TargetDepartmentId.HasValue)
            {
                await _departmentHub.Clients
                    .Group($"department-{request.TargetDepartmentId.Value}")
                    .SendAsync("ReferralReceived", result);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// My referrals — created by the calling doctor.
    /// </summary>
    [HttpGet("my-referrals")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyReferrals()
    {
        var doctorId = GetDoctorId();
        if (doctorId is null) return Forbid();

        var result = await _referralService.GetByReferringDoctorAsync(doctorId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Department queue — referrals pending for the calling user's department.
    /// </summary>
    [HttpGet("department-queue")]
    [Authorize(Roles = "DepartmentStaff,Admin")]
    public async Task<IActionResult> GetDepartmentQueue()
    {
        var departmentId = GetDepartmentId();
        if (departmentId is null && !User.IsInRole("Admin"))
            return Forbid("User is not linked to a department.");

        // Admin can specify departmentId via query
        var deptId = departmentId ?? 0;
        var result = await _referralService.GetDepartmentQueueAsync(deptId);
        return Ok(result);
    }

    /// <summary>
    /// Get department queue by department ID — Admin can query any department.
    /// </summary>
    [HttpGet("department-queue/{departmentId}")]
    [Authorize(Roles = "DepartmentStaff,Admin")]
    public async Task<IActionResult> GetDepartmentQueueById(int departmentId)
    {
        var result = await _referralService.GetDepartmentQueueAsync(departmentId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _referralService.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Update referral status — DepartmentStaff or target Doctor.
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Doctor,DepartmentStaff,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReferralStatusRequest request)
    {
        var result = await _referralService.UpdateStatusAsync(id, request.Status);
        if (result is null) return NotFound();

        await _departmentHub.Clients.All.SendAsync("ReferralUpdated", result);

        // Notify referring doctor when completed
        if (request.Status == Domain.Enums.ReferralStatus.Completed)
        {
            await _departmentHub.Clients.All.SendAsync("ReferralCompleted", result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Attach result — DepartmentStaff.
    /// </summary>
    [HttpPut("{id}/result")]
    [Authorize(Roles = "DepartmentStaff,Admin")]
    public async Task<IActionResult> AttachResult(int id, [FromBody] AttachReferralResultRequest request)
    {
        var result = await _referralService.AttachResultAsync(id, request.ResultNotes);
        if (result is null) return NotFound();

        await _departmentHub.Clients.All.SendAsync("ReferralUpdated", result);
        // Notify referring doctor
        await _departmentHub.Clients.All.SendAsync("ReferralCompleted", result);

        return Ok(result);
    }
}
