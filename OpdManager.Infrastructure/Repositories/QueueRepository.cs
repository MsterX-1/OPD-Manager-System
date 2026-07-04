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

        public async Task<int> GetNextQueueNumberAsync()
        {
            var todayEntries = await DbSet
                .Where(q => q.CheckInTime.Date == DateTime.Today)
                .ToListAsync();
            return todayEntries.Any() ? todayEntries.Max(q => q.QueueNumber) + 1 : 1;
        }

        public new void Delete(QueueEntry entry) => DbSet.Remove(entry); //override Delete to remove the entry from the DbSet
    }
}
