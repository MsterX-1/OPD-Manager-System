using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class DepartmentQueueRepository : BaseRepository<DepartmentQueueEntry>, IDepartmentQueueRepository
{
    public DepartmentQueueRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DepartmentQueueEntry>> GetByDepartmentAsync(int departmentId)
    {
        return await DbSet
            .Include(q => q.Referral)
                .ThenInclude(r => r.Patient)
            .Include(q => q.Referral)
                .ThenInclude(r => r.ReferringDoctor)
            .Include(q => q.Referral)
                .ThenInclude(r => r.TargetDepartmentService)
            .Where(q => q.DepartmentId == departmentId)
            .OrderBy(q => q.QueueNumber)
            .ToListAsync();
    }

    public async Task<int> GetNextQueueNumberAsync(int departmentId)
    {
        var today = DateTime.UtcNow.Date;
        var maxNumber = await DbSet
            .Where(q => q.DepartmentId == departmentId && q.CheckInTime.Date == today)
            .MaxAsync(q => (int?)q.QueueNumber) ?? 0;
        return maxNumber + 1;
    }
}
