using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IIssueRepository : IBaseRepository<Issue>
    {
        Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status);
    }
}
