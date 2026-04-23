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
        private readonly AppDbContext _context;

        public IDoctorRepository Doctors { get; }
        public IPatientRepository Patients { get; }
        public IAppointmentRepository Appointments { get; }
        public IIssueRepository Issues { get; }
        public IQueueRepository Queue { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Doctors = new DoctorRepository(context);
            Patients = new PatientRepository(context);
            Appointments = new AppointmentRepository(context);
            Issues = new IssueRepository(context);
            Queue = new QueueRepository(context);
        }

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
