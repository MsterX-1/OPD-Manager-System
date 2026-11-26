using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations;

public class DepartmentManagementService : IDepartmentManagementService
{
    private readonly IUnitOfWork _uow;
    public DepartmentManagementService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        var departments = await _uow.Departments.GetAllAsync();
        var result = new List<DepartmentDto>();
        foreach (var d in departments)
        {
            var full = await _uow.Departments.GetWithServicesAsync(d.Id);
            if (full is not null)
                result.Add(MapToDto(full));
        }
        return result;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllActiveAsync()
    {
        var departments = await _uow.Departments.GetAllActiveAsync();
        var result = new List<DepartmentDto>();
        foreach (var d in departments)
        {
            var full = await _uow.Departments.GetWithServicesAsync(d.Id);
            if (full is not null)
                result.Add(MapToDto(full));
        }
        return result;
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _uow.Departments.GetWithServicesAsync(id);
        return department is null ? null : MapToDto(department);
    }

    public async Task<DepartmentDto> CreateAsync(string name, DepartmentType type)
    {
        var department = new Department
        {
            Name = name,
            Type = type,
            IsActive = true
        };

        await _uow.Departments.AddAsync(department);
        await _uow.SaveChangesAsync();

        return MapToDto(department);
    }

    public async Task<DepartmentDto?> UpdateAsync(int id, string name, DepartmentType type, bool isActive)
    {
        var department = await _uow.Departments.GetWithServicesAsync(id);
        if (department is null) return null;

        department.Name = name;
        department.Type = type;
        department.IsActive = isActive;

        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();

        return MapToDto(department);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _uow.Departments.GetByIdAsync(id);
        if (department is null) return false;

        var referrals = await _uow.Referrals.GetWhereAsync(r => r.TargetDepartmentId == id);
        if (referrals.Any())
        {
            throw new InvalidOperationException($"Cannot delete Department '{department.Name}' because it has existing referrals. Please mark it as inactive instead.");
        }

        _uow.Departments.Delete(department);
        await _uow.SaveChangesAsync();
        return true;
    }

    // ── Department Services ─────────────────────────────────────────────────

    public async Task<DepartmentServiceDto> AddServiceAsync(int departmentId, string name, decimal fee)
    {
        var department = await _uow.Departments.GetWithServicesAsync(departmentId)
            ?? throw new InvalidOperationException("Department not found.");

        var service = new DepartmentService
        {
            DepartmentId = departmentId,
            Name = name,
            Fee = fee
        };

        department.Services.Add(service);
        await _uow.SaveChangesAsync();

        return new DepartmentServiceDto
        {
            Id = service.Id,
            DepartmentId = service.DepartmentId,
            Name = service.Name,
            Fee = service.Fee
        };
    }

    public async Task<DepartmentServiceDto?> UpdateServiceAsync(int id, string name, decimal fee)
    {
        // Find service through department
        var departments = await _uow.Departments.GetAllAsync();
        foreach (var dept in departments)
        {
            var full = await _uow.Departments.GetWithServicesAsync(dept.Id);
            if (full is null) continue;
            var service = full.Services.FirstOrDefault(s => s.Id == id);
            if (service is not null)
            {
                service.Name = name;
                service.Fee = fee;
                await _uow.SaveChangesAsync();
                return new DepartmentServiceDto
                {
                    Id = service.Id,
                    DepartmentId = service.DepartmentId,
                    Name = service.Name,
                    Fee = service.Fee
                };
            }
        }
        return null;
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        var departments = await _uow.Departments.GetAllAsync();
        foreach (var dept in departments)
        {
            var full = await _uow.Departments.GetWithServicesAsync(dept.Id);
            if (full is null) continue;
            var service = full.Services.FirstOrDefault(s => s.Id == id);
            if (service is not null)
            {
                full.Services.Remove(service);
                await _uow.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    private static DepartmentDto MapToDto(Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Type = d.Type,
        IsActive = d.IsActive,
        Services = d.Services?.Select(s => new DepartmentServiceDto
        {
            Id = s.Id,
            DepartmentId = s.DepartmentId,
            Name = s.Name,
            Fee = s.Fee
        }).ToList() ?? new()
    };
}
