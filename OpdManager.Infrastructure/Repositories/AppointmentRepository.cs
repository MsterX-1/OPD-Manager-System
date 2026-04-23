using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;
        public AppointmentRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Appointment>> GetAllAsync() =>
            await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.QueueEntry)
                .ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date) =>
            await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.QueueEntry)
                .Where(a => a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.TimeSlot)
                .ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime date) =>
            await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.TimeSlot)
                .ToListAsync();

        public async Task<Appointment?> GetByIdAsync(int id) =>
            await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.QueueEntry)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, TimeSpan timeSlot) =>
            !await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate.Date == date.Date &&
                a.TimeSlot == timeSlot &&
                a.Status != Domain.Enums.AppointmentStatus.Cancelled);

        public async Task AddAsync(Appointment appointment) =>
            await _context.Appointments.AddAsync(appointment);

        public void Update(Appointment appointment) =>
            _context.Appointments.Update(appointment);

        public void Delete(Appointment appointment) =>
            _context.Appointments.Remove(appointment);
    }
}
