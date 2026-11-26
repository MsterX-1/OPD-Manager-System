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

    /// <summary>
    /// Retrieves all doctors, including inactive ones.
    /// Restricted to Admin role.
    /// </summary>
    /// <returns>A list of all doctors.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors.Select(MapToResponse));
    }

    /// <summary>
    /// Retrieves a list of active doctors.
    /// </summary>
    /// <returns>A list of active doctors.</returns>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var doctors = await _doctorService.GetActiveAsync();
        return Ok(doctors.Select(MapToResponse));
    }

    /// <summary>
    /// Retrieves detailed information for a specific doctor.
    /// </summary>
    /// <param name="id">The doctor ID.</param>
    /// <returns>The doctor details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor is null) return NotFound();
        return Ok(MapToResponse(doctor));
    }


    /// <summary>
    /// Creates a new doctor profile and optionally assigns a schedule.
    /// </summary>
    /// <param name="request">The doctor creation payload.</param>
    /// <returns>The created doctor profile.</returns>
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

    /// <summary>
    /// Updates an existing doctor's profile and schedule.
    /// </summary>
    /// <param name="id">The doctor ID to update.</param>
    /// <param name="request">The updated payload.</param>
    /// <returns>The updated doctor profile.</returns>
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

    /// <summary>
    /// Deletes a doctor from the system.
    /// </summary>
    /// <param name="id">The doctor ID to delete.</param>
    /// <returns>NoContent if successful, BadRequest if the doctor has dependencies.</returns>
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
