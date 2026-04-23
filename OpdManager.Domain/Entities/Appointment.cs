using System.ComponentModel.DataAnnotations.Schema;
using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // TimeSlot stored as long (ticks) — SQLite can't order/compare TimeSpan
    public long TimeSlotTicks { get; set; }

    [NotMapped]
    public TimeSpan TimeSlot
    {
        get => TimeSpan.FromTicks(TimeSlotTicks);
        set => TimeSlotTicks = value.Ticks;
    }

    // Foreign Keys
    public int DoctorId { get; set; }
    public int PatientId { get; set; }

    // Navigation
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public QueueEntry? QueueEntry { get; set; }
}