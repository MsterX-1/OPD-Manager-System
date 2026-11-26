using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Provides REST endpoints for patient management, including creation, updates, and history retrieval.
/// </summary>
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;

    public PatientsController(IPatientService patientService, IAppointmentService appointmentService)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Retrieves a paginated list of all patients.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list of patients.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var patients = (await _patientService.GetAllAsync()).ToList();
        var totalCount = patients.Count;
        var items = patients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            hasNext = page * pageSize < totalCount,
            hasPrevious = page > 1
        });
    }

    /// <summary>
    /// Retrieves a specific patient by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the patient.</param>
    /// <returns>The patient details if found, otherwise NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null) return NotFound();
        return Ok(patient);
    }

    /// <summary>
    /// Searches for patients based on a search term (e.g., name or phone).
    /// </summary>
    /// <param name="term">The search query.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list of matching patients.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var patients = (await _patientService.SearchAsync(term)).ToList();
        var totalCount = patients.Count;
        var items = patients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            hasNext = page * pageSize < totalCount,
            hasPrevious = page > 1
        });
    }

    /// <summary>
    /// Retrieves the appointment history for a specific patient.
    /// </summary>
    /// <param name="id">The patient ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated list of the patient's past appointments.</returns>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _appointmentService.GetByPatientPagedAsync(id, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new patient record.
    /// </summary>
    /// <param name="request">The patient creation payload.</param>
    /// <returns>The created patient object.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PatientRequest request)
    {
        var patient = new Patient
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes,
            NationalId = request.NationalId,
            Gender = request.Gender,
            KnownAllergies = request.KnownAllergies,
            CreatedAt = DateTime.UtcNow
        };

        await _patientService.AddAsync(patient);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    /// <summary>
    /// Updates an existing patient record.
    /// </summary>
    /// <param name="id">The ID of the patient to update.</param>
    /// <param name="request">The updated patient payload.</param>
    /// <returns>The updated patient if successful, otherwise NotFound.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PatientRequest request)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null) return NotFound();

        patient.FullName = request.FullName;
        patient.PhoneNumber = request.PhoneNumber;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Notes = request.Notes;
        patient.NationalId = request.NationalId;
        patient.Gender = request.Gender;
        patient.KnownAllergies = request.KnownAllergies;

        await _patientService.UpdateAsync(patient);
        return Ok(patient);
    }

    /// <summary>
    /// Deletes a patient from the system. Ensure the patient has no associated records before deleting.
    /// </summary>
    /// <param name="id">The patient ID to delete.</param>
    /// <returns>NoContent if successful.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);
        return NoContent();
    }
}
