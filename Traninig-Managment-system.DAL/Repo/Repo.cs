
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Traninig_Managment_system.DAL.Repo
{

    public class Repo<T> : IRepo<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbset;

        public Repo(ApplicationDbContext applicationDbContext )
        {
            _context = applicationDbContext;
            _dbset = _context.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
           await _dbset.AddAsync(entity); //عشان عملية الادد async
        }

        public  Task Update(T entity, CancellationToken cancellationToken = default)
        {
            _dbset.Update(entity);
            return Task.CompletedTask;
        }

        public Task Delete(T entity, CancellationToken cancellationToken = default)
        {
            _dbset.Remove(entity);
            return Task.CompletedTask; // دى لو مافيش await , ولو مافيش داتا هترجع,
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter= null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbset;

            if (filter != null)
                query = query.Where(filter);
            return await query.ToListAsync(cancellationToken);

        }
        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbset;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }


        public async Task SaveChangesAsync(CancellationToken cancellationToken=default)
        {
             await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = _dbset;

            if (filter != null)
                query = query.Where(filter);

            return await query.CountAsync();
        }
    }
}

