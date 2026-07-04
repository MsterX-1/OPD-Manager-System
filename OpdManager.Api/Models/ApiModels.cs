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
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff";
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    public string Role { get; set; } = "Staff";
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
