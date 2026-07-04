using System.ComponentModel.DataAnnotations.Schema;
using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

public class QueueEntry
{
    public int Id { get; set; }
    public int QueueNumber { get; set; }
    public QueueStatus Status { get; set; } = QueueStatus.Waiting;
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Waiting time in minutes: CheckInTime → StartTime (or now if still waiting).
    /// </summary>
    [NotMapped]
    public int? WaitingMinutes => StartTime.HasValue
        ? (int)(StartTime.Value - CheckInTime).TotalMinutes
        : (int)(DateTime.UtcNow - CheckInTime).TotalMinutes;

    /// <summary>
    /// Examination time in minutes: StartTime → EndTime.
    /// Only meaningful for Done entries — null for Skipped or incomplete.
    /// </summary>
    [NotMapped]
    public int? ExaminationMinutes =>
        Status == QueueStatus.Done && StartTime.HasValue && EndTime.HasValue
            ? (int)(EndTime.Value - StartTime.Value).TotalMinutes
            : null;

    // Foreign Key
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
}
