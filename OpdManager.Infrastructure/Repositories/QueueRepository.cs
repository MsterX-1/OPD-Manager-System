using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class QueueRepository : IQueueRepository
    {
        private readonly AppDbContext _context;
        public QueueRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<QueueEntry>> GetTodayQueueAsync() =>
            await _context.QueueEntries
                .Include(q => q.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(q => q.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(q => q.CheckInTime.Date == DateTime.Today)
                .OrderBy(q => q.QueueNumber)
                .ToListAsync();

        public async Task<QueueEntry?> GetByIdAsync(int id) =>
            await _context.QueueEntries.FindAsync(id);

        public async Task<int> GetNextQueueNumberAsync()
        {
            var todayEntries = await _context.QueueEntries
                .Where(q => q.CheckInTime.Date == DateTime.Today)
                .ToListAsync();
            return todayEntries.Any() ? todayEntries.Max(q => q.QueueNumber) + 1 : 1;
        }

        public async Task AddAsync(QueueEntry entry) =>
            await _context.QueueEntries.AddAsync(entry);

        public void Update(QueueEntry entry) =>
            _context.QueueEntries.Update(entry);
    }
}
