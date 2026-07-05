using OpdManager.Application.DTOs;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Interfaces;

/// <summary>
/// Orchestrates patient referrals between departments and doctors.
/// Ensures seamless handover of care and tracking of pending services.
/// </summary>
public interface IReferralService
{
    /// <summary>
    /// Create a referral — only the assigned doctor for the source appointment can refer.
    /// </summary>
    Task<ReferralDto> CreateAsync(int sourceAppointmentId, int referringDoctorId,
        ReferralType type, int? targetDoctorId, int? targetDepartmentId,
        int? targetDepartmentServiceId, string? reason);

    Task<IEnumerable<ReferralDto>> GetByReferringDoctorAsync(int doctorId);
    Task<IEnumerable<ReferralDto>> GetByTargetDoctorAsync(int doctorId);
    Task<IEnumerable<ReferralDto>> GetByPatientAsync(int patientId);

    /// <summary>
    /// Get the department queue — pending/in-progress referrals for a department.
    /// </summary>
    Task<IEnumerable<DepartmentQueueEntryDto>> GetDepartmentQueueAsync(int departmentId);

    /// <summary>
    /// Update referral status (DepartmentStaff or target Doctor).
    /// </summary>
    Task<ReferralDto?> UpdateStatusAsync(int referralId, ReferralStatus status);

    /// <summary>
    /// Attach result notes to a completed referral (DepartmentStaff).
    /// </summary>
    Task<ReferralDto?> AttachResultAsync(int referralId, string? resultNotes);

    Task<ReferralDto?> GetByIdAsync(int id);
}
