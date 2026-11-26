using System.ComponentModel.DataAnnotations.Schema;
using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

/// <summary>
/// Tracks the live status of a patient waiting to see a doctor.
/// Generated when a scheduled appointment is checked in by staff.
/// </summary>
public class QueueEntry
{
    public int Id { get; set; }
    public int QueueNumber { get; set; }
    public QueueStatus Status { get; set; } = QueueStatus.Waiting;
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int AccumulatedExaminationSeconds { get; set; } = 0;

    /// <summary>
    /// Waiting time in minutes: CheckInTime → StartTime (or now if still waiting).
    /// </summary>
    [NotMapped]
    public int? WaitingMinutes => StartTime.HasValue
        ? (int)(StartTime.Value - CheckInTime).TotalMinutes
        : (int)(DateTime.UtcNow - CheckInTime).TotalMinutes;

    /// <summary>
    /// Examination time in minutes. Uses accumulated time if present, plus any active time.
    /// Only meaningful for Done entries, or live calculation.
    /// </summary>
    [NotMapped]
    public int? ExaminationMinutes
    {
        get
        {
            if (Status == QueueStatus.Done && StartTime.HasValue && EndTime.HasValue)
            {
                var totalSeconds = AccumulatedExaminationSeconds + (EndTime.Value - StartTime.Value).TotalSeconds;
                return (int)(totalSeconds / 60);
            }
            if (Status == QueueStatus.Done && AccumulatedExaminationSeconds > 0)
            {
                return AccumulatedExaminationSeconds / 60;
            }
            return null;
        }
    }

    // Foreign Key
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
}
