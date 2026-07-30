using System;
using System.Collections.Generic;
using System.Text;
using Traninig_Managment_system.BLL.Dtos;

namespace Traninig_Managment_system.BLL.Services.classes.Company_class
{
    public class CompanyDashBoardServices:ICompanyDashboardService
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly ICourseRepo _courseRepo;
        private readonly IInstructorRepo _instructorRepo;

        public CompanyDashBoardServices(IEmployeeRepo employeeRepo,ICategoryRepo categoryRepo,ICourseRepo courseRepo,IInstructorRepo instructorRepo)
        {
            _employeeRepo = employeeRepo;
            _categoryRepo = categoryRepo;
            _courseRepo = courseRepo;
            _instructorRepo = instructorRepo;
        }
        public async Task<CompanyDashboardDto> GetDashboardAsync(int companyId)
        {
            return new CompanyDashboardDto
            {
                EmployeeCount = await _employeeRepo.CountAsync(e => e.CompanyId == companyId),

                CourseCount = await _courseRepo.CountAsync(c => c.Category.CompanyId == companyId),

                CategoryCount = await _categoryRepo.CountAsync(c => c.CompanyId == companyId ),

                InstructorCount = await _instructorRepo.CountAsync(i => i.CompanyId == companyId),

            };
        }
    }
}
