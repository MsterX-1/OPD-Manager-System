using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IReferralRepository : IBaseRepository<Referral>
{
    Task<IEnumerable<Referral>> GetByReferringDoctorAsync(int doctorId);
    Task<IEnumerable<Referral>> GetByTargetDepartmentAsync(int departmentId);
    Task<IEnumerable<Referral>> GetByTargetDoctorAsync(int doctorId);
    Task<IEnumerable<Referral>> GetByPatientAsync(int patientId);
    Task<Referral?> GetWithDetailsAsync(int id);
}
