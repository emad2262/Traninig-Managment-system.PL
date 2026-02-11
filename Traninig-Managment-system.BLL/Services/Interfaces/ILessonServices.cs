using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Services.Interfaces
{
    public interface ILessonServices
    {
        Task AddLessonToCourseAsync(int companyId,int courseId, CreateLessonVm model, string instructorUserId);
        Task<bool> EditLessonToCourseAsync(int LessonId,int courseId, EditLessonVm model, string instructorUserId);
        Task<List<LessonDisplayVm>> GetLessonsByCourseAsync(int companyId,int courseId);
        Task<EditLessonVm?> GetLessonForEditAsync(int lessonId, int courseId, string instructorUserId);


    }
}
