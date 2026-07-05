namespace OpdManager.Domain.Entities;

/// <summary>
/// Represents a medical professional in the system.
/// Doctors have their own independent queues and schedules.
/// </summary>
public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Per-doctor override for average examination minutes (used in daily cap calculation).
    /// When null, the global default from AppSettings is used.
    /// </summary>
    public int? AverageExaminationMinutes { get; set; }

    /// <summary>
    /// Optional FK to ApplicationUser — links a Doctor record to a login account.
    /// Not every doctor needs a login on day one, so this stays optional.
    /// </summary>
    public string? UserId { get; set; }

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Referral> ReferralsCreated { get; set; } = new List<Referral>();
    public ICollection<Referral> ReferralsReceived { get; set; } = new List<Referral>();
}
