using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private ApplicationDbContext _db;
        internal DbSet<T> _dbSet;

        public Repository(ApplicationDbContext db) { 
            _db = db;
            this._dbSet = _db.Set<T>();
        }

        public async Task CreateAsync(T entity)//Create
        {
            await _dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true)
        {
            IQueryable<T> query = _dbSet;
            if (!tracked)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);
            
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)//GetAll
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
                query.Where(filter);
            return await query.ToListAsync();//At this point  the query will be executed . This is defferd execution.ToList() causes immediate execution.
        }

        public async Task RemoveAsync(T entity)//Remove
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()//Save
        {
            await _db.SaveChangesAsync();
        }
        /*
        public async Task UpdateAsync(T entity)
        {
            _db.Villas.Update(entity);
            await SaveAsync();
        }
        */
    }
}
