using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

/// <summary>
/// Core entity representing a registered individual receiving care.
/// Maintains a single source of demographic and identifying information.
/// </summary>
public class Patient
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// National ID or legal identifier used by the hospital (e.g., Egyptian National ID).
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// Patient gender — clinically relevant for medical records and insurance.
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Free-text field for known allergies. Displayed prominently to doctors before prescribing.
    /// </summary>
    public string? KnownAllergies { get; set; }

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}
