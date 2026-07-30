namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IPlanRepo : IRepo<Plan>
    {
        /// <summary>
        /// يجيب كل الباقات النشطة بس
        /// </summary>
        Task<IEnumerable<Plan>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    }
}
