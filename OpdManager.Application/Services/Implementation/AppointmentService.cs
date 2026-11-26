using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Domain.Enums;

namespace OpdManager.Application.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;
    private readonly ISettingsService _settings;

    public AppointmentService(IUnitOfWork uow, ISettingsService settings)
    {
        _uow = uow;
        _settings = settings;
    }

    public async Task<IEnumerable<AppointmentDto>> GetByDateAsync(DateTime date)
    {
        var appointments = await _uow.Appointments.GetByDateAsync(date);
        return appointments.Select(MapToDto);
    }

    public async Task<PagedResult<AppointmentDto>> GetByDatePagedAsync(
        DateTime date, int page, int pageSize, int doctorId = 0)
    {
        var paged = await _uow.Appointments.GetByDatePagedAsync(date, page, pageSize, doctorId);
        return MapPaged(paged);
    }

    public async Task<PagedResult<AppointmentDto>> SearchPagedAsync(
        string? patientName, int doctorId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize)
    {
        var paged = await _uow.Appointments.SearchPagedAsync(
            patientName, doctorId, fromDate, toDate, page, pageSize);
        return MapPaged(paged);
    }

    public async Task<PagedResult<AppointmentDto>> GetByPatientPagedAsync(
        int patientId, int page, int pageSize)
    {
        var paged = await _uow.Appointments.GetByPatientPagedAsync(patientId, page, pageSize);
        return MapPaged(paged);
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var a = await _uow.Appointments.GetByIdAsync(id);
        return a is null ? null : MapToDto(a);
    }

    public async Task<(bool Success, string Error)> BookAsync(Appointment appointment)
    {
        // Validate doctor exists
        var doctor = await _uow.Doctors.GetByIdAsync(appointment.DoctorId);
        if (doctor is null)
            return (false, "Doctor not found.");

        // Validate patient exists
        var patient = await _uow.Patients.GetByIdAsync(appointment.PatientId);
        if (patient is null)
            return (false, "Patient not found.");

        // Prevent duplicate same-day bookings
        var existingSameDay = await _uow.Appointments.GetWhereAsync(a => 
            a.PatientId == appointment.PatientId && 
            a.DoctorId == appointment.DoctorId && 
            a.AppointmentDate.Date == appointment.AppointmentDate.Date && 
            a.Status != AppointmentStatus.Cancelled &&
            a.Id != appointment.Id);

        if (existingSameDay.Any())
            return (false, $"Patient is already booked with Dr. {doctor.Name} on {appointment.AppointmentDate:d}.");

        // Validate doctor works on that weekday
        var dayOfWeek = appointment.AppointmentDate.DayOfWeek;
        var schedule = await _uow.DoctorSchedules.GetByDoctorAndDayAsync(appointment.DoctorId, dayOfWeek);
        if (schedule is null || !schedule.IsWorking)
            return (false, $"Dr. {doctor.Name} does not work on {dayOfWeek}s.");

        Appointment? existing = null;
        if (appointment.Id != 0)
        {
            existing = await _uow.Appointments.GetByIdAsync(appointment.Id);
            if (existing is null) return (false, "Appointment not found.");

            // Prevent structural changes if patient is already in the queue
            if (existing.QueueEntry != null)
            {
                if (existing.DoctorId != appointment.DoctorId || existing.AppointmentDate != appointment.AppointmentDate)
                {
                    return (false, "Cannot change the Doctor or Date for an appointment that is already checked in.");
                }
            }
        }

        // Check optional daily appointment cap
        var settings = await _settings.GetSettingsAsync();
        if (settings.EnableDailyAppointmentCap)
        {
            var avgMinutes = doctor.AverageExaminationMinutes ?? settings.DefaultAverageExaminationMinutes;
            var workingMinutes = (schedule.EndTime - schedule.StartTime).TotalMinutes;
            var cap = (int)(workingMinutes / avgMinutes);

            var currentCount = await _uow.Appointments.CountByDoctorAndDateAsync(
                appointment.DoctorId, appointment.AppointmentDate);

            // If we are editing an appointment that already exists on this same date and doctor,
            // it is already included in currentCount. We must subtract it so we don't count it twice.
            if (existing != null && 
                existing.DoctorId == appointment.DoctorId && 
                existing.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                existing.Status != AppointmentStatus.Cancelled)
            {
                currentCount--;
            }

            if (currentCount >= cap)
                return (false, $"Dr. {doctor.Name} has reached the daily appointment cap ({cap}) for this date.");
        }

        if (appointment.Id == 0)
        {
            await _uow.Appointments.AddAsync(appointment);
        }
        else
        {
            existing!.DoctorId = appointment.DoctorId;
            existing.PatientId = appointment.PatientId;
            existing.AppointmentDate = appointment.AppointmentDate;
            existing.Notes = appointment.Notes;

            _uow.Appointments.Update(existing);
        }

        await _uow.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<bool> CancelAsync(int id)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id);
        if (appointment is null) return false;

        appointment.Status = AppointmentStatus.Cancelled;
        _uow.Appointments.Update(appointment);

        if (appointment.QueueEntry != null)
        {
            appointment.QueueEntry.Status = QueueStatus.Skipped;
            _uow.Queue.Update(appointment.QueueEntry);
        }

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

    public async Task DeleteAsync(int id)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id);
        if (appointment is null) return;

        if (appointment.QueueEntry != null || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot delete an appointment that is already checked in or completed.");
        }

        _uow.Appointments.Delete(appointment);
        await _uow.SaveChangesAsync();
    }

    public async Task<AvailabilityDto?> GetAvailabilityAsync(int doctorId, DateTime date)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(doctorId);
        if (doctor is null) return null;

        var dayOfWeek = date.DayOfWeek;
        var schedule = await _uow.DoctorSchedules.GetByDoctorAndDayAsync(doctorId, dayOfWeek);
        if (schedule is null || !schedule.IsWorking) return null;

        var settings = await _settings.GetSettingsAsync();
        if (!settings.EnableDailyAppointmentCap)
        {
            return new AvailabilityDto { IsCapEnabled = false };
        }

        var avgMinutes = doctor.AverageExaminationMinutes ?? settings.DefaultAverageExaminationMinutes;
        var workingMinutes = (schedule.EndTime - schedule.StartTime).TotalMinutes;
        var cap = (int)(workingMinutes / avgMinutes);
        
        var currentCount = await _uow.Appointments.CountByDoctorAndDateAsync(doctorId, date);

        return new AvailabilityDto 
        { 
            IsCapEnabled = true, 
            MaxCap = cap, 
            CurrentCount = currentCount 
        };
    }

    private static PagedResult<AppointmentDto> MapPaged(PagedResult<Appointment> paged) =>
        new()
        {
            Items = paged.Items.Select(MapToDto),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        DoctorId = a.DoctorId,
        PatientName = a.Patient.FullName,
        DoctorName = a.Doctor.Name,
        DoctorSpecialty = a.Doctor.Specialty,
        AppointmentDate = a.AppointmentDate,
        Status = a.Status,
        QueueNumber = a.QueueEntry?.QueueNumber,
        WaitingMinutes = a.QueueEntry?.WaitingMinutes,
        Notes = a.Notes
    };
}