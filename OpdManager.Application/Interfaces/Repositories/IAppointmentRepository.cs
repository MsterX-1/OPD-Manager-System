using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IAppointmentRepository : IBaseRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
    Task<PagedResult<Appointment>> GetByDatePagedAsync(
        DateTime date, int page, int pageSize, int doctorId = 0);
    Task<PagedResult<Appointment>> SearchPagedAsync(
        string? patientName, int doctorId, DateTime? fromDate,
        DateTime? toDate, int page, int pageSize);
    Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime date);
    Task<PagedResult<Appointment>> GetByPatientPagedAsync(
        int patientId, int page, int pageSize);
    Task<bool> IsSlotAvailableAsync(
        int doctorId, DateTime date, TimeSpan timeSlot, int excludeAppointmentId = 0);
    Task<IEnumerable<Appointment>> GetAllByDoctorAsync(int doctorId);
}