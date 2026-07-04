using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<IEnumerable<Doctor>> GetActiveAsync();
    Task<Doctor?> GetByIdAsync(int id);
    Task AddAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
    Task DeleteAsync(int id);
    Task<IEnumerable<DoctorScheduleDto>> GetScheduleAsync(int doctorId);
    Task SetScheduleAsync(int doctorId, IEnumerable<DoctorSchedule> schedules);
}
