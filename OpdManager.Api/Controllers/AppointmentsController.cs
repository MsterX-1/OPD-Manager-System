using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

using OpdManager.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Provides REST endpoints for booking, viewing, and managing patient appointments.
/// </summary>
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IHubContext<QueueHub> _queueHub;
    private readonly IHubContext<DashboardHub> _dashboardHub;

    public AppointmentsController(
        IAppointmentService appointmentService, 
        IHubContext<QueueHub> queueHub,
        IHubContext<DashboardHub> dashboardHub)
    {
        _appointmentService = appointmentService;
        _queueHub = queueHub;
        _dashboardHub = dashboardHub;
    }

    /// <summary>
    /// Retrieves a paginated list of appointments for a specific date.
    /// Optionally filters by doctor ID.
    /// </summary>
    /// <param name="date">The date of the appointments.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="doctorId">Optional doctor ID to filter appointments.</param>
    /// <returns>A paginated list of appointments.</returns>
    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDate(
        [FromQuery] DateTime date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int doctorId = 0)
    {
        if (User.IsInRole("Doctor"))
        {
            var doctorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "DoctorId")?.Value;
            if (int.TryParse(doctorIdClaim, out int did))
            {
                doctorId = did;
            }
        }
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
        if (User.IsInRole("Doctor"))
        {
            var doctorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "DoctorId")?.Value;
            if (int.TryParse(doctorIdClaim, out int did))
            {
                doctorId = did;
            }
        }
        var result = await _appointmentService.SearchPagedAsync(
            patientName, doctorId, fromDate, toDate, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific appointment by its unique identifier.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <returns>The appointment details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null) return NotFound();
        return Ok(appointment);
    }

    /// <summary>
    /// Books a new appointment for a patient with a specific doctor.
    /// Validates availability and broadcasts an update via SignalR if successful.
    /// </summary>
    /// <param name="request">The booking request payload.</param>
    /// <returns>Success message and appointment ID, or BadRequest with error message.</returns>
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

        await _queueHub.Clients.All.SendAsync("QueueUpdated");
        await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");
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

        await _queueHub.Clients.All.SendAsync("QueueUpdated");
        await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");
        return Ok(new { message = "Appointment updated successfully." });
    }

    /// <summary>
    /// Cancels an existing appointment.
    /// Notifies connected clients of the queue change.
    /// </summary>
    /// <param name="id">The appointment ID to cancel.</param>
    /// <returns>Ok if cancelled successfully, otherwise NotFound.</returns>
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _appointmentService.CancelAsync(id);
        if (!result) return NotFound();
        
        await _queueHub.Clients.All.SendAsync("QueueUpdated");
        await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");
        return Ok(new { message = "Appointment cancelled." });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] AppointmentStatus status)
    {
        await _appointmentService.UpdateStatusAsync(id, status);
        await _queueHub.Clients.All.SendAsync("QueueUpdated");
        await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");
        return Ok(new { message = "Status updated." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _appointmentService.DeleteAsync(id);
        await _queueHub.Clients.All.SendAsync("QueueUpdated");
        await _dashboardHub.Clients.All.SendAsync("DashboardUpdated");
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
