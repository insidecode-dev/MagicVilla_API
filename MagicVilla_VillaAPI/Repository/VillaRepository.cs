using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class VillaRepository : IVillaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VillaRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateAsync(Villa villa)
        {
            await _dbContext.Villas.AddAsync(villa);
            await SaveAsync();
        }

        public async Task<Villa>? GetAsync(Expression<Func<Villa, bool>>? filter = null, bool tracked = true)
        {
            IQueryable<Villa>? query = _dbContext.Villas;
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

        public async Task<List<Villa>> GetAllAsync(Expression<Func<Villa, bool>>? filter = null)
        {
            IQueryable<Villa> query = _dbContext.Villas;
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            //query is executed in line below, ToListAsync() method executes the query, this is deferred execution
            return await query.ToListAsync();
        }

        public async Task RemoveAsync(Villa villa)
        {
            _dbContext.Villas.Remove(villa);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Villa villa)
        {
            _dbContext.Villas.Update(villa);
            await SaveAsync();
        }
    }
}
