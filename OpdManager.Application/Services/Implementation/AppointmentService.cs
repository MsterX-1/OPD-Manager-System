using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        public AppointmentService(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<AppointmentDto>> GetByDateAsync(DateTime date)
        {
            var appointments = await _uow.Appointments.GetByDateAsync(date);
            return appointments.Select(MapToDto);
        }

        public async Task<PagedResult<AppointmentDto>> GetByDatePagedAsync(
            DateTime date, int page, int pageSize)
        {
            var paged = await _uow.Appointments.GetByDatePagedAsync(date, page, pageSize);
            return new PagedResult<AppointmentDto>
            {
                Items = paged.Items.Select(MapToDto),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var a = await _uow.Appointments.GetByIdAsync(id);
            return a is null ? null : MapToDto(a);
        }

        public async Task<(bool Success, string Error)> BookAsync(Appointment appointment)
        {
            var available = await _uow.Appointments.IsSlotAvailableAsync(
                appointment.DoctorId,
                appointment.AppointmentDate,
                appointment.TimeSlot);

            if (!available)
                return (false, "This time slot is already booked for the selected doctor.");

            await _uow.Appointments.AddAsync(appointment);
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<bool> CancelAsync(int id)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(id);
            if (appointment is null) return false;

            appointment.Status = AppointmentStatus.Cancelled;
            _uow.Appointments.Update(appointment);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task UpdateStatusAsync(int id, AppointmentStatus status)
        {
            var appointment = await _uow.Appointments.GetByIdAsync(id);
            if (appointment is null) return;

            appointment.Status = status;
            _uow.Appointments.Update(appointment);
            await _uow.SaveChangesAsync();
        }

        private static AppointmentDto MapToDto(Appointment a) => new()
        {
            Id = a.Id,
            PatientName = a.Patient.FullName,
            DoctorName = a.Doctor.Name,
            DoctorSpecialty = a.Doctor.Specialty,
            AppointmentDate = a.AppointmentDate,
            TimeSlot = a.TimeSlot,
            Status = a.Status,
            QueueNumber = a.QueueEntry?.QueueNumber,
            WaitingMinutes = a.QueueEntry?.WaitingMinutes
        };
    }
}