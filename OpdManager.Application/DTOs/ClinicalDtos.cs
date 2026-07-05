using OpdManager.Domain.Enums;

namespace OpdManager.Application.DTOs;

// ── Medical Record DTOs ─────────────────────────────────────────────────────

public class MedicalRecordDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    // Vitals
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int? OxygenSaturation { get; set; }

    // Clinical
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<PrescriptionDto> Prescriptions { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class PrescriptionDto
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public int? MedicalRecordId { get; set; }
    public int? ReferralId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public string? UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; }
}

// ── Department DTOs ─────────────────────────────────────────────────────────

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public bool IsActive { get; set; }
    public List<DepartmentServiceDto> Services { get; set; } = new();
}

public class DepartmentServiceDto
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}

// ── Referral DTOs ───────────────────────────────────────────────────────────

public class ReferralDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int ReferringDoctorId { get; set; }
    public string ReferringDoctorName { get; set; } = string.Empty;
    public int SourceAppointmentId { get; set; }

    public ReferralType ReferralType { get; set; }
    public int? TargetDoctorId { get; set; }
    public string? TargetDoctorName { get; set; }
    public int? TargetDepartmentId { get; set; }
    public string? TargetDepartmentName { get; set; }
    public int? TargetDepartmentServiceId { get; set; }
    public string? TargetServiceName { get; set; }

    public string? Reason { get; set; }
    public ReferralStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ResultNotes { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class DepartmentQueueEntryDto
{
    public int Id { get; set; }
    public int QueueNumber { get; set; }
    public QueueStatus Status { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int ReferralId { get; set; }
    public int DepartmentId { get; set; }

    // Denormalized for display
    public string PatientName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? ServiceName { get; set; }
    public string ReferringDoctorName { get; set; } = string.Empty;
}

// ── Patient Chart DTO ───────────────────────────────────────────────────────

public class PatientChartDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? KnownAllergies { get; set; }
    public List<MedicalRecordDto> MedicalRecords { get; set; } = new();
    public List<ReferralDto> Referrals { get; set; } = new();
}
