using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;

    public PatientsController(IPatientService patientService, IAppointmentService appointmentService)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
    }

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null) return NotFound();
        return Ok(patient);
    }

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

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _appointmentService.GetByPatientPagedAsync(id, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PatientRequest request)
    {
        var patient = new Patient
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _patientService.AddAsync(patient);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PatientRequest request)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null) return NotFound();

        patient.FullName = request.FullName;
        patient.PhoneNumber = request.PhoneNumber;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Notes = request.Notes;

        await _patientService.UpdateAsync(patient);
        return Ok(patient);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);
        return NoContent();
    }
}
