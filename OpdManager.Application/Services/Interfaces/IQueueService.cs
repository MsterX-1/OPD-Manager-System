using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Application.DTOs;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Interfaces
{
    public interface IQueueService
    {
        Task<IEnumerable<QueueEntryDto>> GetTodayQueueAsync();
        Task<QueueEntryDto> CheckInAsync(int appointmentId);
        Task UpdateStatusAsync(int queueEntryId, QueueStatus status);
        Task<DashboardDto> GetDashboardStatsAsync();
    }
}
