using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class DoctorRepository : BaseRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Doctor>> GetActiveAsync() =>
        await DbSet
            .Include(d => d.Schedules)
            .Where(d => d.IsActive)
            .ToListAsync();

    public override async Task<Doctor?> GetByIdAsync(int id) =>
        await DbSet
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.Id == id);

    public new async Task<IEnumerable<Doctor>> GetAllAsync() =>
        await DbSet
            .Include(d => d.Schedules)
            .ToListAsync();
}
