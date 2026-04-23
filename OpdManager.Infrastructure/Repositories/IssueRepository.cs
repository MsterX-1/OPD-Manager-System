using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class IssueRepository : IIssueRepository
    {
        private readonly AppDbContext _context;
        public IssueRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Issue>> GetAllAsync() =>
            await _context.Issues.OrderByDescending(i => i.ReportedAt).ToListAsync();

        public async Task<IEnumerable<Issue>> GetByStatusAsync(IssueStatus status) =>
            await _context.Issues.Where(i => i.Status == status).ToListAsync();

        public async Task<Issue?> GetByIdAsync(int id) =>
            await _context.Issues.FindAsync(id);

        public async Task AddAsync(Issue issue) =>
            await _context.Issues.AddAsync(issue);

        public void Update(Issue issue) =>
            _context.Issues.Update(issue);

        public void Delete(Issue issue) =>
            _context.Issues.Remove(issue);
    }
}
