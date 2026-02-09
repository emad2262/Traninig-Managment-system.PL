
using Microsoft.EntityFrameworkCore;

namespace Traninig_Managment_system.DAL.Repo
{
    public class Repo<T> :IRepo<T> where T : class
    {
        private readonly ApplicationDbContext context;
        private readonly DbSet<T> _dbset;

        public Repo(ApplicationDbContext applicationDbContext )
        {
            context = applicationDbContext;
            _dbset = context.Set<T>();
        }

        public async Task<bool> CreateAsync(T entity)
        {
            try
            {
                await _dbset.AddAsync(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> UdateAsync(T entity)
        {
            try
            {
                _dbset.Update(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<bool> Delete(T entity)
        {
            try
            {
                _dbset.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null,params Expression<Func<T, object>>[]? includes)
        {
            IQueryable<T> query = _dbset.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetOneAsync(Expression<Func<T, bool>> filter,params Expression<Func<T, object>>[]? includes)
        {
            IQueryable<T> query = _dbset.AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(filter);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbset.CountAsync(filter);
        }
    }
}

