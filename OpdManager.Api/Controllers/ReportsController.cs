using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IAppointmentService _appointmentService;
    private readonly IQueueService _queueService;
    private readonly ISettingsService _settingsService;

    public ReportsController(
        IReportService reportService,
        IAppointmentService appointmentService,
        IQueueService queueService,
        ISettingsService settingsService)
    {
        _reportService = reportService;
        _appointmentService = appointmentService;
        _queueService = queueService;
        _settingsService = settingsService;
    }

    [HttpGet("appointments/pdf")]
    public async Task<IActionResult> AppointmentsPdf([FromQuery] DateTime date)
    {
        var appointments = (await _appointmentService.GetByDateAsync(date)).ToList();
        var settings = await _settingsService.GetSettingsAsync();

        // We need the actual entities for the report service — fetch from raw data
        var rawAppointments = appointments.Select(a => new Domain.Entities.Appointment
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            Status = a.Status,
            Notes = a.Notes,
            Patient = new Domain.Entities.Patient { FullName = a.PatientName },
            Doctor = new Domain.Entities.Doctor { Name = a.DoctorName, Specialty = a.DoctorSpecialty }
        }).ToList();

        var pdf = _reportService.ExportAppointmentsToPdf(rawAppointments, settings.HospitalName);
        return File(pdf, "application/pdf", $"appointments_{date:yyyy-MM-dd}.pdf");
    }

    [HttpGet("appointments/excel")]
    public async Task<IActionResult> AppointmentsExcel([FromQuery] DateTime date)
    {
        var appointments = (await _appointmentService.GetByDateAsync(date)).ToList();

        var rawAppointments = appointments.Select(a => new Domain.Entities.Appointment
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            Status = a.Status,
            Notes = a.Notes,
            Patient = new Domain.Entities.Patient { FullName = a.PatientName },
            Doctor = new Domain.Entities.Doctor { Name = a.DoctorName, Specialty = a.DoctorSpecialty }
        }).ToList();

        var excel = _reportService.ExportAppointmentsToExcel(rawAppointments);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"appointments_{date:yyyy-MM-dd}.xlsx");
    }

    [HttpGet("performance/pdf")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PerformancePdf(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? doctorId)
    {
        var performance = (await _queueService.GetDoctorPerformanceAsync(from, to, doctorId)).ToList();
        var settings = await _settingsService.GetSettingsAsync();
        var pdf = _reportService.ExportDoctorPerformanceToPdf(performance, from, to, settings.HospitalName);
        return File(pdf, "application/pdf", $"doctor_performance_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
    }

    [HttpGet("performance/excel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PerformanceExcel(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? doctorId)
    {
        var performance = (await _queueService.GetDoctorPerformanceAsync(from, to, doctorId)).ToList();
        var excel = _reportService.ExportDoctorPerformanceToExcel(performance, from, to);
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"doctor_performance_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Analytics(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var performance = await _queueService.GetDoctorPerformanceAsync(from, to);
        return Ok(performance);
    }

    [HttpGet("analytics/details")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AnalyticsDetails(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int doctorId)
    {
        var details = await _queueService.GetDoctorPatientDetailsAsync(from, to, doctorId);
        return Ok(details);
    }
}
