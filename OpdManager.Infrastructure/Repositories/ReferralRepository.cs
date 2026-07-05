using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class ReferralRepository : BaseRepository<Referral>, IReferralRepository
{
    public ReferralRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Referral>> GetByReferringDoctorAsync(int doctorId)
    {
        return await DbSet
            .Include(r => r.Patient)
            .Include(r => r.ReferringDoctor)
            .Include(r => r.TargetDoctor)
            .Include(r => r.TargetDepartment)
            .Include(r => r.TargetDepartmentService)
            .Include(r => r.Attachments)
            .Where(r => r.ReferringDoctorId == doctorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Referral>> GetByTargetDepartmentAsync(int departmentId)
    {
        return await DbSet
            .Include(r => r.Patient)
            .Include(r => r.ReferringDoctor)
            .Include(r => r.TargetDepartment)
            .Include(r => r.TargetDepartmentService)
            .Include(r => r.Attachments)
            .Where(r => r.TargetDepartmentId == departmentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Referral>> GetByTargetDoctorAsync(int doctorId)
    {
        return await DbSet
            .Include(r => r.Patient)
            .Include(r => r.ReferringDoctor)
            .Include(r => r.TargetDoctor)
            .Include(r => r.TargetDepartmentService)
            .Include(r => r.Attachments)
            .Where(r => r.TargetDoctorId == doctorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Referral>> GetByPatientAsync(int patientId)
    {
        return await DbSet
            .Include(r => r.Patient)
            .Include(r => r.ReferringDoctor)
            .Include(r => r.TargetDoctor)
            .Include(r => r.TargetDepartment)
            .Include(r => r.TargetDepartmentService)
            .Include(r => r.Attachments)
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Referral?> GetWithDetailsAsync(int id)
    {
        return await DbSet
            .Include(r => r.Patient)
            .Include(r => r.ReferringDoctor)
            .Include(r => r.TargetDoctor)
            .Include(r => r.TargetDepartment)
            .Include(r => r.TargetDepartmentService)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
