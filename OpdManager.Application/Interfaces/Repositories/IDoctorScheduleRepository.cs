using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IDoctorScheduleRepository : IBaseRepository<DoctorSchedule>
{
    Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);
    Task<DoctorSchedule?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek day);
}
