using OpdManager.Application.Interfaces.Repositories;

namespace OpdManager.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDoctorRepository Doctors { get; }
    IPatientRepository Patients { get; }
    IAppointmentRepository Appointments { get; }
    IIssueRepository Issues { get; }
    IQueueRepository Queue { get; }
    IDoctorScheduleRepository DoctorSchedules { get; }

    Task<int> SaveChangesAsync();
}
