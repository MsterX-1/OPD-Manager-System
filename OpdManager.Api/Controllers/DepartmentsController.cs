using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Api.Models;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Provides REST endpoints for CRUD operations on departments and their nested services.
/// </summary>
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentManagementService _departmentService;

    public DepartmentsController(IDepartmentManagementService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// List all active departments — any authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _departmentService.GetAllActiveAsync();
        return Ok(result);
    }

    /// <summary>
    /// List all departments (including inactive) — Admin only.
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllIncludingInactive()
    {
        var result = await _departmentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] DepartmentRequest request)
    {
        var result = await _departmentService.CreateAsync(request.Name, request.Type);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentRequest request)
    {
        var result = await _departmentService.UpdateAsync(id, request.Name, request.Type, request.IsActive);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _departmentService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    // ── Department Services (price list) ────────────────────────────────────

    [HttpPost("{departmentId}/services")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddService(int departmentId, [FromBody] DepartmentServiceRequest request)
    {
        try
        {
            var result = await _departmentService.AddServiceAsync(departmentId, request.Name, request.Fee);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("services/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] DepartmentServiceRequest request)
    {
        var result = await _departmentService.UpdateServiceAsync(id, request.Name, request.Fee);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("services/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var result = await _departmentService.DeleteServiceAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
