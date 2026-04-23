using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Application.DTOs;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IBaseRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
        Task<PagedResult<Appointment>> GetByDatePagedAsync(DateTime date, int page, int pageSize);
        Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime date);
        Task<bool> IsSlotAvailableAsync(int doctorId, DateTime date, TimeSpan timeSlot);
    }
}
