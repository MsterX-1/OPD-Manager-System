using OpdManager.Application.DTOs;

namespace OpdManager.Application.Services.Interfaces;

public interface ISettingsService
{
    Task<AppSettingsDto> GetSettingsAsync();
    Task UpdateSettingsAsync(AppSettingsDto settings);
}
