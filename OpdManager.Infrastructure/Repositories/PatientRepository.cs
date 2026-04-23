using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;
        public PatientRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Patient>> GetAllAsync() =>
            await _context.Patients.ToListAsync();

        public async Task<Patient?> GetByIdAsync(int id) =>
            await _context.Patients.FindAsync(id);

        public async Task<IEnumerable<Patient>> SearchAsync(string term) =>
            await _context.Patients
                .Where(p => p.FullName.Contains(term) || (p.PhoneNumber != null && p.PhoneNumber.Contains(term)))
                .ToListAsync();

        public async Task AddAsync(Patient patient) =>
            await _context.Patients.AddAsync(patient);

        public void Update(Patient patient) =>
            _context.Patients.Update(patient);

        public void Delete(Patient patient) =>
            _context.Patients.Remove(patient);
    }
}
