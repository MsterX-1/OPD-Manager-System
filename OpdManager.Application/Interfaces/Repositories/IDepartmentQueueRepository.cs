using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IDepartmentQueueRepository : IBaseRepository<DepartmentQueueEntry>
{
    Task<IEnumerable<DepartmentQueueEntry>> GetByDepartmentAsync(int departmentId);
    Task<int> GetNextQueueNumberAsync(int departmentId);
}
