using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpdManager.Application.Interfaces.Repositories;
using OpdManager.Infrastructure.Data;

namespace OpdManager.Infrastructure.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<T> DbSet;

        protected BaseRepository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public IQueryable<T> Query() => DbSet.AsQueryable();

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await DbSet.ToListAsync();

        public virtual async Task<T?> GetByIdAsync(int id) =>
            await DbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetWhereAsync(
            Expression<Func<T, bool>> predicate) =>
            await DbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) =>
            await DbSet.AddAsync(entity);

        public void Update(T entity) =>
            DbSet.Update(entity);

        public void Delete(T entity) =>
            DbSet.Remove(entity);
    }
}