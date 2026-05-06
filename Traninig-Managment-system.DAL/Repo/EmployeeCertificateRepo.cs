namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeCertificateRepo : Repo<EmployeeCertificate>, IEmployeeCertificateRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeCertificateRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EmployeeCertificate?> GetByEmployeeCourseAsync(int employeeId, int courseId)
        {
            return await CertificateQuery()
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.CourseId == courseId);
        }

        public async Task<EmployeeCertificate?> GetForUpdateByEmployeeCourseAsync(int employeeId, int courseId)
        {
            return await _context.EmployeeCertificates
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.CourseId == courseId);
        }

        public async Task<EmployeeCertificate?> GetForUpdateAsync(int companyId, int certificateId)
        {
            return await _context.EmployeeCertificates
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == certificateId);
        }

        public async Task<EmployeeCertificate?> GetIssuedForEmployeeCourseAsync(int employeeId, int courseId)
        {
            return await CertificateQuery()
                .FirstOrDefaultAsync(c =>
                    c.EmployeeId == employeeId &&
                    c.CourseId == courseId &&
                    c.Status == CertificateStatus.Issued);
        }

        public async Task<EmployeeCertificate?> GetCompanyCertificateAsync(int companyId, int certificateId)
        {
            return await CertificateQuery()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == certificateId);
        }

        public async Task<List<EmployeeCertificate>> GetCompanyCertificatesAsync(int companyId, CertificateStatus? status = null)
        {
            var query = CertificateQuery().Where(c => c.CompanyId == companyId);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderBy(c => c.Status == CertificateStatus.PendingCompanyApproval ? 0 : 1)
                .ThenByDescending(c => c.RequestedAt)
                .ToListAsync();
        }

        public async Task<List<EmployeeCertificate>> GetRecentCompanyCertificatesAsync(int companyId, int take)
        {
            return await CertificateQuery()
                .Where(c => c.CompanyId == companyId)
                .OrderByDescending(c => c.Status == CertificateStatus.Issued
                    ? c.IssuedAt ?? c.RequestedAt
                    : c.RequestedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountPendingAsync(int companyId)
        {
            return await _context.EmployeeCertificates
                .CountAsync(c => c.CompanyId == companyId && c.Status == CertificateStatus.PendingCompanyApproval);
        }

        public async Task<bool> DeleteByEmployeeCourseAsync(int employeeId, int courseId)
        {
            var certificate = await _context.EmployeeCertificates
                .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.CourseId == courseId);

            if (certificate == null)
            {
                return true;
            }

            _context.EmployeeCertificates.Remove(certificate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByEmployeeAsync(int employeeId)
        {
            var certificates = await _context.EmployeeCertificates
                .Where(c => c.EmployeeId == employeeId)
                .ToListAsync();

            if (!certificates.Any())
            {
                return true;
            }

            _context.EmployeeCertificates.RemoveRange(certificates);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByCourseAsync(int courseId)
        {
            var certificates = await _context.EmployeeCertificates
                .Where(c => c.CourseId == courseId)
                .ToListAsync();

            if (!certificates.Any())
            {
                return true;
            }

            _context.EmployeeCertificates.RemoveRange(certificates);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<EmployeeCertificate> CertificateQuery()
        {
            return _context.EmployeeCertificates
                .AsNoTracking()
                .Include(c => c.Employee)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(c => c.Course)
                    .ThenInclude(c => c.Category);
        }
    }
}
