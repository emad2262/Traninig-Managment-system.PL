using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.DAL.Data;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo
{
    public class EmployeeRepo : Repo<Employee>, IEmployeeRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<Employee?> GetWithCoursesAndInstructors(int employeeId)
        {
            return await _context.employees
                    .AsNoTracking()
                .Include(e => e.EmployeeCourses)
                    .ThenInclude(ec => ec.Course)
                        .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(e => e.Id == employeeId);
        }
        //        | السطر | الشرح |
        //|-------|-------|
        //| `_context.employees` | ابدأ من جدول الموظفين |
        //| `.Include(e => e.EmployeeCourses)` | هات الكورسات المسجل فيها الموظف(المستوى 1) |
        //| `.ThenInclude(ec => ec.Course)` | ومن كل EmployeeCourse هات بيانات الكورس(المستوى 2) |
        //| `.ThenInclude(c => c.Instructor)` | ومن كل Course هات الـ Instructor(المستوى 3) |
        //| `.FirstOrDefaultAsync(e => e.Id == employeeId)` | هات أول موظف الـ Id بتاعه = employeeId |
        public async Task<IEnumerable<Employee>> GetEmployeesForInstructorCoursesAsync(int companyId, string instructorUserId)
        {
            return await _context.employees
                .Where(e => e.CompanyId == companyId &&
                       e.EmployeeCourses.Any(ec => ec.Course.Instructor.UserId == instructorUserId))
                .AsNoTracking() 
                .ToListAsync();
        }
    }

}
