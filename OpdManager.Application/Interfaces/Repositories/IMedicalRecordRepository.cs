using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories;

public interface IMedicalRecordRepository : IBaseRepository<MedicalRecord>
{
    Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);
    Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
    Task<MedicalRecord?> GetWithDetailsAsync(int id);
}
