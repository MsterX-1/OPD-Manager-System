using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using System.Security.Claims;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Provides REST endpoints for managing doctor profiles, schedules, and active queues.
/// </summary>
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors.Select(MapToResponse));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var doctors = await _doctorService.GetActiveAsync();
        return Ok(doctors.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor is null) return NotFound();
        return Ok(MapToResponse(doctor));
    }

    [HttpGet("me/queue")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyQueue([FromServices] IQueueService queueService)
    {
        var claim = User.FindFirstValue("DoctorId");
        if (!int.TryParse(claim, out var doctorId)) return Forbid();

        var queue = await queueService.GetTodayQueueAsync();
        return Ok(queue.Where(q => q.DoctorId == doctorId));
    }

    [HttpGet("me/referrals")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyReferrals([FromServices] IReferralService referralService)
    {
        var claim = User.FindFirstValue("DoctorId");
        if (!int.TryParse(claim, out var doctorId)) return Forbid();

        var sent = await referralService.GetByReferringDoctorAsync(doctorId);
        var received = await referralService.GetByTargetDoctorAsync(doctorId);

        return Ok(new { sent, received });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] DoctorRequest request)
    {
        var doctor = new Doctor
        {
            Name = request.Name,
            Specialty = request.Specialty,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive,
            AverageExaminationMinutes = request.AverageExaminationMinutes
        };

        await _doctorService.AddAsync(doctor);

        // Set schedule if provided
        if (request.Schedules.Any())
        {
            var schedules = request.Schedules.Select(s => new DoctorSchedule
            {
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsWorking = s.IsWorking
            });
            await _doctorService.SetScheduleAsync(doctor.Id, schedules);
        }

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, MapToResponse(doctor));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] DoctorRequest request)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor is null) return NotFound();

        doctor.Name = request.Name;
        doctor.Specialty = request.Specialty;
        doctor.PhoneNumber = request.PhoneNumber;
        doctor.IsActive = request.IsActive;
        doctor.AverageExaminationMinutes = request.AverageExaminationMinutes;

        await _doctorService.UpdateAsync(doctor);

        // Update schedule
        var schedules = request.Schedules.Select(s => new DoctorSchedule
        {
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsWorking = s.IsWorking
        });
        await _doctorService.SetScheduleAsync(id, schedules);

        return Ok(MapToResponse(doctor));
    }

    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor is null) return NotFound();

        doctor.IsActive = !doctor.IsActive;
        await _doctorService.UpdateAsync(doctor);
        return Ok(MapToResponse(doctor));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _doctorService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/schedule")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var schedule = await _doctorService.GetScheduleAsync(id);
        return Ok(schedule);
    }

    private static object MapToResponse(Doctor d) => new
    {
        d.Id,
        d.Name,
        d.Specialty,
        d.PhoneNumber,
        d.IsActive,
        d.AverageExaminationMinutes,
        Schedules = d.Schedules.Select(s => new
        {
            s.Id,
            DayOfWeek = (int)s.DayOfWeek,
            StartTime = s.StartTime.ToString(@"hh\:mm"),
            EndTime = s.EndTime.ToString(@"hh\:mm"),
            s.IsWorking
        })
    };
}
