using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

/// <summary>
/// Mirrors QueueEntry shape but is deliberately separate to avoid touching
/// the existing Doctor-Appointment queue logic.
/// </summary>
public class DepartmentQueueEntry
{
    public int Id { get; set; }
    public int QueueNumber { get; set; }
    public QueueStatus Status { get; set; } = QueueStatus.Waiting;
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    // Foreign Keys
    public int ReferralId { get; set; }
    public int DepartmentId { get; set; }

    // Navigation
    public Referral Referral { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
