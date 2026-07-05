using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpdManager.Api.Models;
using OpdManager.Infrastructure.Identity;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    private static readonly string[] ValidRoles = ["Admin", "Staff", "Doctor", "DepartmentStaff"];

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new
            {
                u.Id,
                u.Email,
                u.FullName,
                Role = roles.FirstOrDefault() ?? "Staff",
                u.DoctorId,
                u.DepartmentId
            });
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            Role = roles.FirstOrDefault() ?? "Staff",
            user.DoctorId,
            user.DepartmentId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return BadRequest(new { message = "A user with this email already exists." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true,
            DoctorId = request.DoctorId,
            DepartmentId = request.DepartmentId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

        var validRole = ValidRoles.Contains(request.Role) ? request.Role : "Staff";
        await _userManager.AddToRoleAsync(user, validRole);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new
        {
            user.Id,
            user.Email,
            user.FullName,
            Role = validRole,
            user.DoctorId,
            user.DepartmentId
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.FullName = request.FullName;
        user.DoctorId = request.DoctorId;
        user.DepartmentId = request.DepartmentId;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!passResult.Succeeded)
                return BadRequest(new { message = string.Join(", ", passResult.Errors.Select(e => e.Description)) });
        }

        await _userManager.UpdateAsync(user);

        // Update role
        var currentRoles = await _userManager.GetRolesAsync(user);
        var validRole = ValidRoles.Contains(request.Role) ? request.Role : "Staff";
        if (!currentRoles.Contains(validRole))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, validRole);
        }

        return Ok(new { user.Id, user.Email, user.FullName, Role = validRole, user.DoctorId, user.DepartmentId });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        // Prevent deleting yourself
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (user.Id == currentUserId)
            return BadRequest(new { message = "You cannot delete your own account." });

        await _userManager.DeleteAsync(user);
        return NoContent();
    }
}
