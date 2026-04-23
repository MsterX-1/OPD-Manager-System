using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public QueueEntry? QueueEntry { get; set; }
    }
}
