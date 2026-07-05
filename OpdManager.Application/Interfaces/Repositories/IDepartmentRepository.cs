using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IDepartmentRepository : IBaseRepository<Department>
{
    Task<Department?> GetWithServicesAsync(int id);
    Task<IEnumerable<Department>> GetAllActiveAsync();
}
