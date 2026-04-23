using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace OpdManager.Application.Interfaces.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> Query();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
