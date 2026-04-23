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
    public class DoctorRepository : BaseRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Doctor>> GetActiveAsync() =>
            await DbSet.Where(d => d.IsActive).ToListAsync();
    }
}
