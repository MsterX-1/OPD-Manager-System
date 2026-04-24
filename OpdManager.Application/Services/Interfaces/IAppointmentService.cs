using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetByDateAsync(DateTime date);
        Task<PagedResult<AppointmentDto>> GetByDatePagedAsync(DateTime date, int page, int pageSize);
        Task<AppointmentDto?> GetByIdAsync(int id);
        Task<(bool Success, string Error)> BookAsync(Appointment appointment);
        Task<bool> CancelAsync(int id);
        Task UpdateStatusAsync(int id, AppointmentStatus status);
        Task DeleteAsync(int id);
    }
}
