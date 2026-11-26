using OpdManager.Domain.Enums;

namespace OpdManager.Application.DTOs;

public class QueueEntryDto
{
    public int Id { get; set; }
    public int QueueNumber { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialty { get; set; } = string.Empty;
    public QueueStatus Status { get; set; }
    public DateTime CheckInTime { get; set; }
    public int WaitingMinutes { get; set; }
    public int? ExaminationMinutes { get; set; }
    public int AccumulatedExaminationSeconds { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
