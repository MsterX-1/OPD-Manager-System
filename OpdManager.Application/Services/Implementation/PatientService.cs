using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _uow;
        public PatientService(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<Patient>> GetAllAsync() =>
            await _uow.Patients.GetAllAsync();

        public async Task<Patient?> GetByIdAsync(int id) =>
            await _uow.Patients.GetByIdAsync(id);

        public async Task<IEnumerable<Patient>> SearchAsync(string term) =>
            await _uow.Patients.SearchAsync(term);

        public async Task AddAsync(Patient patient)
        {
            await _uow.Patients.AddAsync(patient);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(Patient patient)
        {
            _uow.Patients.Update(patient);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var patient = await _uow.Patients.GetByIdAsync(id);
            if (patient is null) return;

            var appointments = await _uow.Appointments.GetByPatientPagedAsync(id, 1, 1);
            if (appointments.TotalCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete Patient '{patient.FullName}' because they have existing clinical history.");
            }

            _uow.Patients.Delete(patient);
            await _uow.SaveChangesAsync();
        }
    }
}