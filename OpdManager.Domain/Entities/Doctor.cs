namespace OpdManager.Domain.Entities;

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

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
