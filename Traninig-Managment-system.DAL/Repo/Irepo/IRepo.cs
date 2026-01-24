
using System.Linq.Expressions;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IRepo<T> where T : class
    {
         Task<bool> CreateAsync(T entity);

         Task<bool> UdateAsync(T entity);
       
         Task<bool> Delete(T entity);
        
         Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null);
        
         Task<T?> GetOne(Expression<Func<T, bool>> expression, Expression<Func<T, object>>[]? includes = null);
        
    }
}
