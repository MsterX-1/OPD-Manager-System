using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime date);
        Task<Appointment?> GetByIdAsync(int id);
        Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, TimeSpan timeSlot);
        Task AddAsync(Appointment appointment);
        void Update(Appointment appointment);
        void Delete(Appointment appointment);
    }
}
