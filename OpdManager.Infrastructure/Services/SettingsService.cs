using Microsoft.EntityFrameworkCore;
using OpdManager.Application.DTOs;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _context;

    public SettingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppSettingsDto> GetSettingsAsync()
    {
        var settings = await _context.AppSettings.ToListAsync();
        var dict = settings.ToDictionary(s => s.Key, s => s.Value);

        return new AppSettingsDto
        {
            HospitalName = dict.GetValueOrDefault("HospitalName", "OPD Manager"),
            WaitingAlertMinutes = int.TryParse(dict.GetValueOrDefault("WaitingAlertMinutes", "30"), out var wam) ? wam : 30,
            EnableDailyAppointmentCap = bool.TryParse(dict.GetValueOrDefault("EnableDailyAppointmentCap", "false"), out var cap) && cap,
            DefaultAverageExaminationMinutes = int.TryParse(dict.GetValueOrDefault("DefaultAverageExaminationMinutes", "15"), out var avg) ? avg : 15
        };
    }

    public async Task UpdateSettingsAsync(AppSettingsDto settings)
    {
        await UpsertAsync("HospitalName", settings.HospitalName);
        await UpsertAsync("WaitingAlertMinutes", settings.WaitingAlertMinutes.ToString());
        await UpsertAsync("EnableDailyAppointmentCap", settings.EnableDailyAppointmentCap.ToString());
        await UpsertAsync("DefaultAverageExaminationMinutes", settings.DefaultAverageExaminationMinutes.ToString());
        await _context.SaveChangesAsync();
    }

    private async Task UpsertAsync(string key, string value)
    {
        var existing = await _context.AppSettings.FindAsync(key);
        if (existing is null)
        {
            _context.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            existing.Value = value;
            _context.AppSettings.Update(existing);
        }
    }
}
