using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Interfaces;

public interface IReportService
{
    void ExportAppointmentsToPdf(List<Appointment> data, string filePath);
    void ExportAppointmentsToExcel(List<Appointment> data, string filePath);
}