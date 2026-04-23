using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces
{
    public interface IReportService
    {
        void ExportAppointmentsToPdf(List<Appointment> data, string filePath);
        void ExportAppointmentsToExcel(List<Appointment> data, string filePath);
        void PrintAppointments(List<Appointment> data);
    }
}
