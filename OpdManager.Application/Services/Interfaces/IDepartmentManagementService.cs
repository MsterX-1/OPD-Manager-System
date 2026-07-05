using OpdManager.Application.DTOs;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Interfaces;

/// <summary>
/// Manages hospital departments and their respective services.
/// Used by admins to structure the hospital hierarchy.
/// </summary>
public interface IDepartmentManagementService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<IEnumerable<DepartmentDto>> GetAllActiveAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(string name, DepartmentType type);
    Task<DepartmentDto?> UpdateAsync(int id, string name, DepartmentType type, bool isActive);
    Task<bool> DeleteAsync(int id);

    // Department Services (price list)
    Task<DepartmentServiceDto> AddServiceAsync(int departmentId, string name, decimal fee);
    Task<DepartmentServiceDto?> UpdateServiceAsync(int id, string name, decimal fee);
    Task<bool> DeleteServiceAsync(int id);
}
