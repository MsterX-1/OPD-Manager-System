using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Interfaces;

public interface IReportService
{
    byte[] ExportAppointmentsToPdf(List<Appointment> data, string hospitalName);
    byte[] ExportAppointmentsToExcel(List<Appointment> data);
    byte[] ExportDoctorPerformanceToPdf(List<DTOs.DoctorPerformanceDto> data, DateTime from, DateTime to, string hospitalName);
    byte[] ExportDoctorPerformanceToExcel(List<DTOs.DoctorPerformanceDto> data, DateTime from, DateTime to);
}