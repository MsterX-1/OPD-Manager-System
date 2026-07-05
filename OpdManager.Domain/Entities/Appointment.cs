using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

/// <summary>
/// Represents a scheduled visit between a Patient and a Doctor.
/// Acts as the precursor to a QueueEntry (when the patient arrives) and a MedicalRecord (when the examination occurs).
/// </summary>
public class Appointment
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int DoctorId { get; set; }
    public int PatientId { get; set; }

    // Navigation
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public QueueEntry? QueueEntry { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
}