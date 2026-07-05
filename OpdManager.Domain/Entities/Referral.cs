using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

/// <summary>
/// Represents an order or transfer of care from a doctor to a department (e.g. Lab test) or another doctor.
/// </summary>
public class Referral
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int ReferringDoctorId { get; set; }
    public int SourceAppointmentId { get; set; }

    public ReferralType ReferralType { get; set; }

    /// <summary>
    /// Set when ReferralType == ToDoctor.
    /// </summary>
    public int? TargetDoctorId { get; set; }

    /// <summary>
    /// Set when ReferralType == ToDepartment.
    /// </summary>
    public int? TargetDepartmentId { get; set; }

    /// <summary>
    /// Specific service within the department (e.g., "CBC" in Lab).
    /// </summary>
    public int? TargetDepartmentServiceId { get; set; }

    public string? Reason { get; set; }
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public string? ResultNotes { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
    public Doctor ReferringDoctor { get; set; } = null!;
    public Appointment SourceAppointment { get; set; } = null!;
    public Doctor? TargetDoctor { get; set; }
    public Department? TargetDepartment { get; set; }
    public DepartmentService? TargetDepartmentService { get; set; }
    public DepartmentQueueEntry? DepartmentQueueEntry { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
