using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Enums;

namespace OpdManager.Domain.Entities
{
    public class Issue
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IssueStatus Status { get; set; } = IssueStatus.Open;
        public string Category { get; set; } = string.Empty; // "Delay", "Complaint", "Other"
        public DateTime ReportedAt { get; set; } = DateTime.Now;
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }

        // Optional link to appointment
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
