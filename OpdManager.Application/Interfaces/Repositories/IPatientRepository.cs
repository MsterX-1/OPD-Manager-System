using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int id);
        Task<IEnumerable<Patient>> SearchAsync(string term);
        Task AddAsync(Patient patient);
        void Update(Patient patient);
        void Delete(Patient patient);
    }
}
