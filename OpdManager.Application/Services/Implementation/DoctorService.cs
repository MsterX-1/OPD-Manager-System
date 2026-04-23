using OpdManager.Application.Interfaces;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _uow;
        public DoctorService(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<Doctor>> GetAllAsync() =>
            await _uow.Doctors.GetAllAsync();

        public async Task<IEnumerable<Doctor>> GetActiveAsync() =>
            await _uow.Doctors.GetActiveAsync();

        public async Task<Doctor?> GetByIdAsync(int id) =>
            await _uow.Doctors.GetByIdAsync(id);

        public async Task AddAsync(Doctor doctor)
        {
            await _uow.Doctors.AddAsync(doctor);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(Doctor doctor)
        {
            _uow.Doctors.Update(doctor);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var doctor = await _uow.Doctors.GetByIdAsync(id);
            if (doctor is null) return;
            _uow.Doctors.Delete(doctor);
            await _uow.SaveChangesAsync();
        }
    }
}