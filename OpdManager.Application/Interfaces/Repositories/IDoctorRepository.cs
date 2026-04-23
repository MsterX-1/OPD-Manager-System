using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IDoctorRepository : IBaseRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetActiveAsync();
    }
}
