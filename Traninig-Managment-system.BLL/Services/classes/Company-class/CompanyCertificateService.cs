using Microsoft.AspNetCore.Identity.UI.Services;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CompanyCertificateService : ICompanyCertificateService
    {
        private readonly IEmployeeCertificateRepo _certificateRepo;
        private readonly ICompanyNotificationRepo _notificationRepo;
        private readonly ICompanyRepo _companyRepo;
        private readonly IEmailSender _emailSender;

        public CompanyCertificateService(
            IEmployeeCertificateRepo certificateRepo,
            ICompanyNotificationRepo notificationRepo,
            ICompanyRepo companyRepo,
            IEmailSender emailSender)
        {
            _certificateRepo = certificateRepo;
            _notificationRepo = notificationRepo;
            _companyRepo = companyRepo;
            _emailSender = emailSender;
        }

        public async Task<IReadOnlyList<CompanyCertificateListItemVm>> GetCertificatesAsync(int companyId, CertificateStatus? status = null)
        {
            var certificates = await _certificateRepo.GetCompanyCertificatesAsync(companyId, status);
            return certificates.Select(MapListItem).ToList();
        }

        public async Task<CompanyCertificateDetailsVm?> GetCertificateDetailsAsync(int companyId, int certificateId)
        {
            var certificate = await _certificateRepo.GetCompanyCertificateAsync(companyId, certificateId);
            if (certificate == null)
            {
                return null;
            }

            var company = await _companyRepo.GetOneAsync(c => c.Id == companyId);
            var item = MapListItem(certificate);
            return new CompanyCertificateDetailsVm
            {
                Id = item.Id,
                CourseId = item.CourseId,
                EmployeeName = item.EmployeeName,
                EmployeeEmail = item.EmployeeEmail,
                CourseTitle = item.CourseTitle,
                InstructorName = item.InstructorName,
                CertificateNumber = item.CertificateNumber,
                Status = item.Status,
                StatusText = item.StatusText,
                FinalScore = item.FinalScore,
                RequestedAt = item.RequestedAt,
                CompletedAt = item.CompletedAt,
                IssuedAt = item.IssuedAt,
                SentAt = item.SentAt,
                CompanyName = company?.Name ?? "Training Company",
                DurationInHours = certificate.Course?.DurationInHours ?? 0,
                CompanyNotes = certificate.CompanyNotes
            };
        }

        public async Task<ServiceResult<bool>> IssueCertificateAsync(
            int companyId,
            CompanyCertificateIssueVm model,
            string issuedByUserId,
            string? certificateUrl)
        {
            var certificate = await _certificateRepo.GetForUpdateAsync(companyId, model.CertificateId);
            if (certificate == null)
            {
                return Fail("Certificate request was not found.");
            }

            var certificateDetails = await _certificateRepo.GetCompanyCertificateAsync(companyId, model.CertificateId);
            if (certificateDetails == null)
            {
                return Fail("Certificate request details could not be loaded.");
            }

            if (certificate.Status == CertificateStatus.Revoked)
            {
                return Fail("This certificate request is revoked and cannot be issued.");
            }

            var now = DateTime.UtcNow;
            certificate.Status = CertificateStatus.Issued;
            certificate.IssuedAt ??= now;
            certificate.IssuedByUserId = issuedByUserId;
            certificate.CompanyNotes = model.CompanyNotes;

            if (string.IsNullOrWhiteSpace(certificate.CertificateNumber))
            {
                certificate.CertificateNumber = BuildCertificateNumber(certificate.CourseId, certificate.EmployeeId, certificate.CompletedAt);
            }

            var emailWasSent = false;
            string? emailError = null;
            if (model.SendEmail)
            {
                try
                {
                    certificateDetails.Status = CertificateStatus.Issued;
                    certificateDetails.IssuedAt = certificate.IssuedAt;
                    certificateDetails.CompanyNotes = model.CompanyNotes;
                    await SendCertificateEmailAsync(certificateDetails, certificateUrl);
                    certificate.SentAt = now;
                    emailWasSent = true;
                }
                catch (Exception ex)
                {
                    emailError = ex.Message;
                }
            }

            var updated = await _certificateRepo.UpdateAsync(certificate);
            if (!updated)
            {
                return Fail("Certificate could not be issued. Please try again.");
            }

            await _notificationRepo.CreateAsync(new CompanyNotification
            {
                CompanyId = companyId,
                Title = "Certificate issued",
                Message = $"{certificateDetails.Employee?.Name ?? "Employee"} certificate for {certificateDetails.Course?.Title ?? "course"} has been issued.",
                Type = CompanyNotificationType.CertificateIssued,
                ReferenceType = "Certificate",
                ReferenceId = certificate.Id,
                DeliveryChannel = model.SendEmail ? "Dashboard, Email" : "Dashboard",
                IsSent = true,
                SentAt = now,
                CreatedAt = now
            });

            if (model.SendEmail && !emailWasSent)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = $"Certificate issued, but email was not sent: {emailError}"
                };
            }

            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = model.SendEmail
                    ? "Certificate issued and sent to the employee."
                    : "Certificate issued successfully."
            };
        }

        private async Task SendCertificateEmailAsync(EmployeeCertificate certificate, string? certificateUrl)
        {
            if (string.IsNullOrWhiteSpace(certificate.Employee?.Email))
            {
                throw new InvalidOperationException("Employee email is missing.");
            }

            var urlPart = string.IsNullOrWhiteSpace(certificateUrl)
                ? "Please sign in to your employee dashboard to print it."
                : $"<a href=\"{certificateUrl}\">Open your certificate</a>";

            var body = $@"
                <h2>Your training certificate is ready</h2>
                <p>Congratulations {certificate.Employee.Name},</p>
                <p>Your company issued your certificate for <strong>{certificate.Course?.Title}</strong>.</p>
                <p>{urlPart}</p>";

            await _emailSender.SendEmailAsync(
                certificate.Employee.Email,
                "Your training certificate is ready",
                body);
        }

        private static CompanyCertificateListItemVm MapListItem(EmployeeCertificate certificate)
        {
            return new CompanyCertificateListItemVm
            {
                Id = certificate.Id,
                CourseId = certificate.CourseId,
                EmployeeName = certificate.Employee?.Name ?? "Employee",
                EmployeeEmail = certificate.Employee?.Email ?? string.Empty,
                CourseTitle = certificate.Course?.Title ?? "Course",
                InstructorName = certificate.Course?.Instructor?.FullName ?? "Instructor",
                CertificateNumber = certificate.CertificateNumber,
                Status = certificate.Status,
                StatusText = MapStatus(certificate.Status),
                FinalScore = certificate.FinalScore,
                RequestedAt = certificate.RequestedAt,
                CompletedAt = certificate.CompletedAt,
                IssuedAt = certificate.IssuedAt,
                SentAt = certificate.SentAt
            };
        }

        private static string MapStatus(CertificateStatus status)
        {
            return status switch
            {
                CertificateStatus.Issued => "Issued",
                CertificateStatus.Revoked => "Revoked",
                _ => "Pending company approval"
            };
        }

        private static string BuildCertificateNumber(int courseId, int employeeId, DateTime completedAt)
        {
            return $"CERT-{courseId:D4}-{employeeId:D4}-{completedAt:yyyyMMdd}";
        }

        private static ServiceResult<bool> Fail(string message)
        {
            return new ServiceResult<bool> { IsSuccess = false, Data = false, Message = message };
        }
    }
}
