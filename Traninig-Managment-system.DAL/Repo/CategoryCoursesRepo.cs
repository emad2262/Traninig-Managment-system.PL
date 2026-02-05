using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Repo
{
    public class CategoryCoursesRepo : Repo<CourseCategory>, ICategoryCoursesRepo
    {
        private readonly ApplicationDbContext _Context;

        public CategoryCoursesRepo(ApplicationDbContext applicationDbContext): base(applicationDbContext)
        {
            _Context = applicationDbContext;
        }


        public async Task<IEnumerable<CourseCategory>> GetCategoriesWithCoursesAndInstructorsAsync(int CompanyId)
        {
          return await _Context.CourseCategories.Where(e=>e.CompanyId==CompanyId)
                .Include (c => c.Courses)
                .ThenInclude(e=>e.Instructor)
                .ToListAsync();
            
        }
    }
}
