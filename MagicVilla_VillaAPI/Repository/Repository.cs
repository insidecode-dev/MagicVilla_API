using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using MagicT_TAPI.Repository.IRepository;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T>:IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        internal readonly DbSet<T> _dbSet;
        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }
        public async Task CreateAsync(T villa)
        {
            await _dbSet.AddAsync(villa);
            await SaveAsync();
        }

        public async Task<T>? GetAsync(Expression<Func<T, bool>>? filter = null, bool tracked = true)
        {
            IQueryable<T>? query = _dbSet;
            if (filter is not null)
            {
                query = query.Where(filter);
            }
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbSet;
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            //query is executed in line below, ToListAsync() method executes the query, this is deferred execution
            return await query.ToListAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

            
    }
}
