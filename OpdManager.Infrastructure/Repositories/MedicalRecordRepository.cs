using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Domain.Entities;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories;

public class MedicalRecordRepository : BaseRepository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(AppDbContext context) : base(context) { }

    public async Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId)
    {
        return await DbSet
            .Include(r => r.Doctor)
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
    }

    public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId)
    {
        return await DbSet
            .Include(r => r.Doctor)
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .Include(r => r.Attachments)
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<MedicalRecord?> GetWithDetailsAsync(int id)
    {
        return await DbSet
            .Include(r => r.Doctor)
            .Include(r => r.Patient)
            .Include(r => r.Prescriptions)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
