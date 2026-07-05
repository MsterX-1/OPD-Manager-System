using OpdManager.Application.Interfaces;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.UnitOfWork;

/// <summary>
/// Coordinates transactions across multiple repositories.
/// Ensures domain models are committed atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IDoctorRepository Doctors { get; }
    public IPatientRepository Patients { get; }
    public IAppointmentRepository Appointments { get; }
    public IIssueRepository Issues { get; }
    public IQueueRepository Queue { get; }
    public IDoctorScheduleRepository DoctorSchedules { get; }
    public IMedicalRecordRepository MedicalRecords { get; }
    public IDepartmentRepository Departments { get; }
    public IReferralRepository Referrals { get; }
    public IDepartmentQueueRepository DepartmentQueue { get; }

    public UnitOfWork(
        AppDbContext context,
        IDoctorRepository doctors,
        IPatientRepository patients,
        IAppointmentRepository appointments,
        IIssueRepository issues,
        IQueueRepository queue,
        IDoctorScheduleRepository doctorSchedules,
        IMedicalRecordRepository medicalRecords,
        IDepartmentRepository departments,
        IReferralRepository referrals,
        IDepartmentQueueRepository departmentQueue)
    {
        _context = context;
        Doctors = doctors;
        Patients = patients;
        Appointments = appointments;
        Issues = issues;
        Queue = queue;
        DoctorSchedules = doctorSchedules;
        MedicalRecords = medicalRecords;
        Departments = departments;
        Referrals = referrals;
        DepartmentQueue = departmentQueue;
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
