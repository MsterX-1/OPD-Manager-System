using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan TimeSlot { get; set; }
        public AppointmentStatus Status { get; set; }
        public int? QueueNumber { get; set; }
        public int? WaitingMinutes { get; set; }
        public string? Notes { get; set; }
    }
}
