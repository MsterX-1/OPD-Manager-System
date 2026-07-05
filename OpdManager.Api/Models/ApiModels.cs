using OpdManager.Domain.Enums;

namespace OpdManager.Api.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = null!;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public int? DepartmentId { get; set; }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff";
    public int? DoctorId { get; set; }
    public int? DepartmentId { get; set; }
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    public string Role { get; set; } = "Staff";
    public int? DoctorId { get; set; }
    public int? DepartmentId { get; set; }
}

public class BookAppointmentRequest
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
}

public class DoctorRequest
{
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public int? AverageExaminationMinutes { get; set; }
    public List<ScheduleItem> Schedules { get; set; } = new();
}

public class ScheduleItem
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorking { get; set; }
}

public class PatientRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public string? NationalId { get; set; }
    public Gender Gender { get; set; }
    public string? KnownAllergies { get; set; }
}

public class IssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
}

public class CloseIssueRequest
{
    public string Resolution { get; set; } = string.Empty;
}

// ── Phase A: Clinical request models ────────────────────────────────────────

public class CreateMedicalRecordRequest
{
    public int AppointmentId { get; set; }
}

public class UpdateVitalsRequest
{
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int? OxygenSaturation { get; set; }
}

public class UpdateDiagnosisRequest
{
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalNotes { get; set; }
}

public class CreatePrescriptionRequest
{
    public int MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }
}

public class UpdatePrescriptionRequest
{
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int? DurationDays { get; set; }
    public string? Instructions { get; set; }
}

public class CreateReferralRequest
{
    public int SourceAppointmentId { get; set; }
    public ReferralType ReferralType { get; set; }
    public int? TargetDoctorId { get; set; }
    public int? TargetDepartmentId { get; set; }
    public int? TargetDepartmentServiceId { get; set; }
    public string? Reason { get; set; }
}

public class UpdateReferralStatusRequest
{
    public ReferralStatus Status { get; set; }
}

public class AttachReferralResultRequest
{
    public string? ResultNotes { get; set; }
}

public class DepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public DepartmentType Type { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DepartmentServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Fee { get; set; }
}
