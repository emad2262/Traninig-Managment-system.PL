namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICompanyRepo : IRepo<Company>
    {
        /// <summary>
        /// يجيب تاريخ انتهاء الاشتراك للشركة
        /// </summary>
        Task<DateTime?> GetCompanyExpirationDateAsync(int companyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// يجيب أفضل موظفين أداءً في الشركة
        /// </summary>
        Task<IReadOnlyList<Employee>> GetTopPerformersAsync(int companyId, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// يتحقق إذا كان الإيميل موجود بشركة تانية
        /// </summary>
        Task<bool> IsEmailTakenAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}
