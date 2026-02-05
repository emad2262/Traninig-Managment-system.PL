using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface ICourseRepo:IRepo<Courses>
    {
        Task<IEnumerable<Courses>> GetByCategoryIdAsync(int categoryId);

        Task<IEnumerable<Courses>> GetByInstructorIdAsync(int instructorId);

        Task<Courses?> GetByIdWithDetailsAsync(int courseId);
    }
}
