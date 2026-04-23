using System;
using System.Collections.Generic;
using System.Text;

namespace OpdManager.Application.DTOs
{
    public class DashboardDto
    {
        public int TotalPatientsToday { get; set; }
        public int WaitingPatients { get; set; }
        public int InProgressPatients { get; set; }
        public int CompletedPatients { get; set; }
        public double AverageWaitingMinutes { get; set; }
        public int ActiveDoctors { get; set; }
        public int OpenIssues { get; set; }
    }
}
