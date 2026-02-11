using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface IEmployeeLessonServices
    {
        Task<EmployeeDashboardVm> GetEmployeeDashboardAsync(string userId);
        Task<CourseLessonsVm> GetCourseLessonsForEmployeeAsync(string userId, int courseId);
        Task<LessonDetailsVm> GetLessonDetailsForEmployeeAsync(string userId, int lessonId);
        Task MarkLessonAsCompletedAsync(string userId, int lessonId);
    }
}
