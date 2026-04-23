using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IQueueRepository
    {
        Task<IEnumerable<QueueEntry>> GetTodayQueueAsync();
        Task<QueueEntry?> GetByIdAsync(int id);
        Task<int> GetNextQueueNumberAsync();
        Task AddAsync(QueueEntry entry);
        void Update(QueueEntry entry);
    }
}
