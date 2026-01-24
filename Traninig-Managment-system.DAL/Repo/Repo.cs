using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.DAL.Data;
using Traninig_Managment_system.DAL.Repo.Irepo;

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
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T,bool>>? expression=null , Expression<Func<T, object>>[]? includes = null)
        {
            IQueryable<T> entity = _dbset;
            if(expression is not null)
            {
                entity = entity.Where(expression);
            }
            if (includes != null)
            {
                foreach (var item in includes)
                {
                    entity = entity.Include(item);
                }
            }
            return await entity.ToListAsync();
        }
        public async Task<T?> GetOne( Expression<Func<T, bool>> expression,Expression<Func<T, object>>[]? includes = null)
        {
            IQueryable<T> entity = _dbset;

            if (includes != null)
            {
                foreach (var item in includes)
                    entity = entity.Include(item);
            }

            return await entity.FirstOrDefaultAsync(expression);
        }
    }
}
