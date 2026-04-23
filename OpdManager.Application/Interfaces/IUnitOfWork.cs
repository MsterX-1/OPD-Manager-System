using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Application.Interfaces.Repositories;

namespace OpdManager.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDoctorRepository Doctors { get; }
        IPatientRepository Patients { get; }
        IAppointmentRepository Appointments { get; }
        IIssueRepository Issues { get; }
        IQueueRepository Queue { get; }

        Task<int> SaveChangesAsync();
    }
}
