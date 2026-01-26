

using Microsoft.EntityFrameworkCore;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IRepo<T> where T : class
    {
         Task<bool> CreateAsync(T entity);

         Task<bool> UdateAsync(T entity);
       
         Task<bool> Delete(T entity);

        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[]? includes);

        Task<T?> GetOneAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[]? includes);
        

    }
}
