using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;

namespace OpdManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AppSettingsDto settings)
    {
        await _settingsService.UpdateSettingsAsync(settings);
        return Ok(settings);
    }
}
