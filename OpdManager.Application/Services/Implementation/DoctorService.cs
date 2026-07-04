using OpdManager.Application.DTOs;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Implementations;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;
    public DoctorService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<Doctor>> GetAllAsync() =>
        await _uow.Doctors.GetAllAsync();

    public async Task<IEnumerable<Doctor>> GetActiveAsync() =>
        await _uow.Doctors.GetActiveAsync();

    public async Task<Doctor?> GetByIdAsync(int id) =>
        await _uow.Doctors.GetByIdAsync(id);

    public async Task AddAsync(Doctor doctor)
    {
        await _uow.Doctors.AddAsync(doctor);
        await _uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(Doctor doctor)
    {
        _uow.Doctors.Update(doctor);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(id);
        if (doctor is null) return;

        var appointments = await _uow.Appointments
            .GetWhereAsync(a => a.DoctorId == id);

        if (appointments.Any())
            throw new InvalidOperationException(
                $"Cannot delete Dr. {doctor.Name} — they have {appointments.Count()} appointment(s) on record.\n" +
                $"Please delete their appointments first.");

        _uow.Doctors.Delete(doctor);
        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<DoctorScheduleDto>> GetScheduleAsync(int doctorId)
    {
        var schedules = await _uow.DoctorSchedules.GetByDoctorIdAsync(doctorId);
        return schedules.Select(s => new DoctorScheduleDto
        {
            Id = s.Id,
            DoctorId = s.DoctorId,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsWorking = s.IsWorking
        });
    }

    public async Task SetScheduleAsync(int doctorId, IEnumerable<DoctorSchedule> schedules)
    {
        // Remove existing schedules
        var existing = await _uow.DoctorSchedules.GetByDoctorIdAsync(doctorId);
        foreach (var s in existing)
            _uow.DoctorSchedules.Delete(s);

        // Add new ones
        foreach (var s in schedules)
        {
            s.DoctorId = doctorId;
            await _uow.DoctorSchedules.AddAsync(s);
        }

        await _uow.SaveChangesAsync();
    }
}