
namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IRepo<T> where T : class
    {
         Task CreateAsync(T entity);

         Task Update(T entity, CancellationToken cancellationToken = default);

         Task Delete(T entity, CancellationToken cancellationToken = default);

         Task<T?> GetOneAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);

         Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, CancellationToken cancellationToken = default);

         Task SaveChangesAsync(CancellationToken cancellationToken = default);
         Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);


    }
}
