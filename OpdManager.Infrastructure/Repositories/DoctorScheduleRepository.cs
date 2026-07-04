using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class DoctorScheduleRepository : BaseRepository<DoctorSchedule>, IDoctorScheduleRepository
{
    public DoctorScheduleRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId) =>
        await DbSet
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();

    public async Task<DoctorSchedule?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek day) =>
        await DbSet.FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == day);
}
