using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<IEnumerable<Doctor>> GetActiveAsync();
        Task<Doctor?> GetByIdAsync(int id);
        Task AddAsync(Doctor doctor);
        void Update(Doctor doctor);
        void Delete(Doctor doctor);
    }
}
