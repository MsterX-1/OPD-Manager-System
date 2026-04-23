using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context) : base(context) { }

        // Override base — appointments always need includes
        public override async Task<Appointment?> GetByIdAsync(int id) =>
            await DbSet
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.QueueEntry)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
        {
            var list = await DbSet
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.QueueEntry)
                .Where(a => a.AppointmentDate.Date == date.Date)
                .ToListAsync();

            return list.OrderBy(a => a.TimeSlot); // in-memory — TimeSpan SQLite fix
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime date)
        {
            var list = await DbSet
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .ToListAsync();

            return list.OrderBy(a => a.TimeSlot);
        }

        public async Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, TimeSpan timeSlot)
        {
            var ticks = timeSlot.Ticks;
            return !await DbSet.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate.Date == date.Date &&
                a.TimeSlotTicks == ticks &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled);
        }

        public async Task<PagedResult<Appointment>> GetByDatePagedAsync(
            DateTime date, int page, int pageSize)
        {
            var query = DbSet
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.QueueEntry)
                .Where(a => a.AppointmentDate.Date == date.Date);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.TimeSlotTicks)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Appointment>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}