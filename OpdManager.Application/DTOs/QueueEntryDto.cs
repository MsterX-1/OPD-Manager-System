using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.DTOs
{
    public class QueueEntryDto
    {
        public int Id { get; set; }
        public int QueueNumber { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public QueueStatus Status { get; set; }
        public DateTime CheckInTime { get; set; }
        public int WaitingMinutes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
