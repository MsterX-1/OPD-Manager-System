using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class IssueRepository : BaseRepository<Issue>, IIssueRepository
    {
        public IssueRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Issue>> GetAllAsync() =>
            await DbSet.OrderByDescending(i => i.ReportedAt).ToListAsync(); // override base for ordering

        public async Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status) =>
            await DbSet.Where(i => i.Status == status).ToListAsync();
    }
}
