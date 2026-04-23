using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _context;
        public DoctorRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Doctor>> GetAllAsync() =>
            await _context.Doctors.ToListAsync();

        public async Task<IEnumerable<Doctor>> GetActiveAsync() =>
            await _context.Doctors.Where(d => d.IsActive).ToListAsync();

        public async Task<Doctor?> GetByIdAsync(int id) =>
            await _context.Doctors.FindAsync(id);

        public async Task AddAsync(Doctor doctor) =>
            await _context.Doctors.AddAsync(doctor);

        public void Update(Doctor doctor) =>
            _context.Doctors.Update(doctor);

        public void Delete(Doctor doctor) =>
            _context.Doctors.Remove(doctor);
    }
}
