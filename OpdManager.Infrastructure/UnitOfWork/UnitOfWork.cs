using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Application.Interfaces;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Infrastructure.Data;
using OpdManager.Infrastructure.Repositories;

namespace OpdManager.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly Data.AppDbContext _context;

        public IDoctorRepository Doctors { get; }
        public IPatientRepository Patients { get; }
        public IAppointmentRepository Appointments { get; }
        public IIssueRepository Issues { get; }
        public IQueueRepository Queue { get; }

        public UnitOfWork(
            AppDbContext context,
            IDoctorRepository doctors,
            IPatientRepository patients,
            IAppointmentRepository appointments,
            IIssueRepository issues,
            IQueueRepository queue)
        {
            _context = context;
            Doctors = doctors;
            Patients = patients;
            Appointments = appointments;
            Issues = issues;
            Queue = queue;
        }

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
