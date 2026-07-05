using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    public async Task<Department?> GetWithServicesAsync(int id)
    {
        return await DbSet
            .Include(d => d.Services)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Department>> GetAllActiveAsync()
    {
        return await DbSet
            .Include(d => d.Services)
            .Where(d => d.IsActive)
            .ToListAsync();
    }
}
