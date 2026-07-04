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
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDate(
        [FromQuery] DateTime date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int doctorId = 0)
    {
        var result = await _appointmentService.GetByDatePagedAsync(date, page, pageSize, doctorId);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? patientName,
        [FromQuery] int doctorId = 0,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _appointmentService.SearchPagedAsync(
            patientName, doctorId, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null) return NotFound();
        return Ok(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookAppointmentRequest request)
    {
        var appointment = new Appointment
        {
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            AppointmentDate = request.AppointmentDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var (success, error) = await _appointmentService.BookAsync(appointment);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Appointment booked successfully.", appointmentId = appointment.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentRequest request)
    {
        var appointment = new Appointment
        {
            Id = id,
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            AppointmentDate = request.AppointmentDate,
            Notes = request.Notes
        };

        var (success, error) = await _appointmentService.BookAsync(appointment);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Appointment updated successfully." });
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _appointmentService.CancelAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Appointment cancelled." });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] AppointmentStatus status)
    {
        await _appointmentService.UpdateStatusAsync(id, status);
        return Ok(new { message = "Status updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] int doctorId, [FromQuery] DateTime date)
    {
        var availability = await _appointmentService.GetAvailabilityAsync(doctorId, date);
        if (availability is null) return Ok(new { isCapEnabled = false });
        return Ok(availability);
    }
}
