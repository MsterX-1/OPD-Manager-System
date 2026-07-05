using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public PrescriptionsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request)
    {
        try
        {
            var result = await _medicalRecordService.AddPrescriptionAsync(
                request.MedicalRecordId,
                request.MedicationName,
                request.Dosage,
                request.Frequency,
                request.DurationDays,
                request.Instructions);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePrescriptionRequest request)
    {
        var result = await _medicalRecordService.UpdatePrescriptionAsync(
            id,
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.DurationDays,
            request.Instructions);

        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _medicalRecordService.DeletePrescriptionAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
