using OpdManager.Application.DTOs;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Interfaces;

/// <summary>
/// Handles the live patient queue and performance metrics.
/// Transitions appointments into active waitlist entries.
/// </summary>
public interface IQueueService
{
    Task<IEnumerable<QueueEntryDto>> GetTodayQueueAsync();
    Task<QueueEntryDto> CheckInAsync(int appointmentId);
    Task UpdateStatusAsync(int queueEntryId, QueueStatus status);
    Task<DashboardDto> GetDashboardStatsAsync(int? doctorId = null);
    Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceAsync(DateTime from, DateTime to, int? doctorId = null);
    Task<IEnumerable<QueueEntryDto>> GetDoctorPatientDetailsAsync(DateTime from, DateTime to, int doctorId);
}
