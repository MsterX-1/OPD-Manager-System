using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IIssueRepository
    {
        Task<IEnumerable<Issue>> GetAllAsync();
        Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status);
        Task<Issue?> GetByIdAsync(int id);
        Task AddAsync(Issue issue);
        void Update(Issue issue);
        void Delete(Issue issue);
    }
}
