using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICourseRepo:IRepo<Courses>
    {
        Task<IEnumerable<Courses>> GetCourseByCategoryIdAsync(int categoryId);
        Task<Courses?> GetCourseWithLessonsAsync(int courseId);



    }
}
