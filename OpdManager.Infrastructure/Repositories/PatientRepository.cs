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
    public class PatientRepository : BaseRepository<Patient>, IPatientRepository
    {
        public PatientRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Patient>> SearchAsync(string term) =>
            await DbSet
                .Where(p => p.FullName.Contains(term) ||
                           (p.PhoneNumber != null && p.PhoneNumber.Contains(term)))
                .ToListAsync();
    }
}
