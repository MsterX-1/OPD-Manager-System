using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/medical-records")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    private int? GetDoctorId()
    {
        var claim = User.FindFirstValue("DoctorId");
        return string.IsNullOrEmpty(claim) ? null : int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Create a medical record for an appointment — Doctor only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMedicalRecordRequest request)
    {
        var doctorId = GetDoctorId();
        if (doctorId is null)
            return Forbid("User is not linked to a doctor record.");

        try
        {
            var result = await _medicalRecordService.CreateAsync(request.AppointmentId, doctorId.Value);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update vitals — Staff or Doctor can do this.
    /// </summary>
    [HttpPut("{id}/vitals")]
    public async Task<IActionResult> UpdateVitals(int id, [FromBody] UpdateVitalsRequest request)
    {
        var result = await _medicalRecordService.UpdateVitalsAsync(
            id,
            request.BloodPressureSystolic,
            request.BloodPressureDiastolic,
            request.TemperatureC,
            request.PulseRate,
            request.RespiratoryRate,
            request.WeightKg,
            request.HeightCm,
            request.OxygenSaturation);

        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Update diagnosis — Doctor only, and only the assigned doctor.
    /// </summary>
    [HttpPut("{id}/diagnosis")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> UpdateDiagnosis(int id, [FromBody] UpdateDiagnosisRequest request)
    {
        var doctorId = GetDoctorId();
        var isAdmin = User.IsInRole("Admin");

        // Verify the doctor is assigned to this record (unless Admin)
        if (!isAdmin && doctorId.HasValue)
        {
            var existing = await _medicalRecordService.GetByIdAsync(id);
            if (existing is not null && existing.DoctorId != doctorId.Value)
                return Forbid("Only the assigned doctor can update diagnosis.");
        }

        var result = await _medicalRecordService.UpdateDiagnosisAsync(
            id, request.ChiefComplaint, request.Diagnosis, request.ClinicalNotes);

        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _medicalRecordService.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-appointment/{appointmentId}")]
    public async Task<IActionResult> GetByAppointment(int appointmentId)
    {
        var result = await _medicalRecordService.GetByAppointmentIdAsync(appointmentId);
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Full patient chart — any authenticated user can read (continuity of care).
    /// </summary>
    [HttpGet("~/api/patients/{patientId}/chart")]
    public async Task<IActionResult> GetPatientChart(int patientId)
    {
        try
        {
            var result = await _medicalRecordService.GetPatientChartAsync(patientId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
