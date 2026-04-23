using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities
{
    public class QueueEntry
    {
        public int Id { get; set; }
        public int QueueNumber { get; set; }
        public QueueStatus Status { get; set; } = QueueStatus.Waiting;
        public DateTime CheckInTime { get; set; } = DateTime.Now;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Computed (not stored)
        public int? WaitingMinutes => StartTime.HasValue
            ? (int)(StartTime.Value - CheckInTime).TotalMinutes
            : (int)(DateTime.Now - CheckInTime).TotalMinutes;

        // Foreign Key
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
    }
}
