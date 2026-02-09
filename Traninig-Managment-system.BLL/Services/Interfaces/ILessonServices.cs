using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ILessonServices
    {
        Task AddLessonToCourseAsync(int companyId,int courseId, CreateLessonVm model);
        Task<List<LessonDisplayVm>> GetLessonsByCourseAsync(int companyId,int courseId);
    }
}
