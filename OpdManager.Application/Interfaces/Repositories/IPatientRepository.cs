using System;
using System.Collections.Generic;
using System.Text;
using OpdManager.Domain.Entities;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IPatientRepository : IBaseRepository<Patient>
    {
        Task<IEnumerable<Patient>> SearchAsync(string term);
    }
}
