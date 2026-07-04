using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class QueueRepository : BaseRepository<QueueEntry>, IQueueRepository
    {
        public QueueRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<QueueEntry>> GetTodayQueueAsync() =>
            await DbSet
                .Include(q => q.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(q => q.CheckInTime.Date == DateTime.Today)
                .OrderBy(q => q.QueueNumber)
                .ToListAsync();

        public async Task<int> GetNextQueueNumberAsync(int doctorId)
        {
            var todayEntries = await DbSet
                .Include(q => q.Appointment)
                .Where(q => q.CheckInTime.Date == DateTime.Today && q.Appointment.DoctorId == doctorId)
                .ToListAsync();
            return todayEntries.Any() ? todayEntries.Max(q => q.QueueNumber) + 1 : 1;
        }

        public async Task<IEnumerable<QueueEntry>> GetPerformanceQueueAsync(DateTime from, DateTime to) =>
            await DbSet
                .Include(q => q.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a.Patient)
                .Where(q => q.Status == Domain.Enums.QueueStatus.Done &&
                            q.CheckInTime.Date >= from.Date &&
                            q.CheckInTime.Date <= to.Date)
                .ToListAsync();

        public new void Delete(QueueEntry entry) => DbSet.Remove(entry); //override Delete to remove the entry from the DbSet
    }
}
