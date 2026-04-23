using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Interfaces
{
    public interface IIssueService
    {
        Task<IEnumerable<Issue>> GetAllAsync();
        Task<IEnumerable<Issue>> GetOpenAsync();
        Task AddAsync(Issue issue);
        Task UpdateAsync(Issue issue);
        Task CloseAsync(int id, string resolution);
        Task DeleteAsync(int id);
    }
}
