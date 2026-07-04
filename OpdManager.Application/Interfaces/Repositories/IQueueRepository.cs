using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IQueueRepository : IBaseRepository<QueueEntry>
    {
        Task<IEnumerable<QueueEntry>> GetTodayQueueAsync();
        Task<int> GetNextQueueNumberAsync(int doctorId);
        Task<IEnumerable<QueueEntry>> GetPerformanceQueueAsync(DateTime from, DateTime to);
    }
}
