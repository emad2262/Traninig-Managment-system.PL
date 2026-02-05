namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CategoryServices : ICategoryCourseServices
    {
        private readonly ICategoryCoursesRepo _categoryCourses;

        public CategoryServices(ICategoryCoursesRepo categoryCourses)
        {
            _categoryCourses = categoryCourses;
        }

       
        public async Task<HomeCompanyVm> GetCompanyHomeDataAsync(int companyId)
        {
            
            if (companyId <= 0)
                throw new ArgumentException("Invalid Company Id");

            var categories = await _categoryCourses.GetCategoriesWithCoursesAndInstructorsAsync(companyId);

            var homeVm = new HomeCompanyVm
            {
                Categories = categories.Select(category => new CategoryVm
                {
                    Id = category.Id,
                    Name = category.Name,

                    Courses = category.Courses.Select(course => new CourseVm
                    {
                        Id = course.Id,
                        Title = course.Title,
                        Description = course.Description,
                        InstructorName = course.Instructor?.FullName ?? string.Empty
                    }).ToList()

                }).ToList()
            };

            return homeVm;
        }
    }
}

//public async Task<HomeCompanyVm> GetCompanyHomeDataAsync(int companyId)
//{
//    if (companyId <= 0)
//        throw new ArgumentException("Invalid Company Id");

//    var categories = await _categoryCourses
//        .GetCategoriesWithCoursesAndInstructorsAsync(companyId);

//    // ✅ Home Container
//    var homeVm = new HomeCompanyVm();

//    foreach (var category in categories)
//    {
//        var categoryVm = new CategoryVm
//        {
//            Id = category.Id,
//            Name = category.Name
//        };

//        foreach (var course in category.Courses)
//        {
//            var courseVm = new CourseVm
//            {
//                Title = course.Title,
//                Description = course.Description,
//                InstructorName = course.Instructor?.FullName ?? string.Empty
//            };

//            // ✅ اربط الكورس بالكاتيجوري
//            categoryVm.Courses.Add(courseVm);
//        }

//        // ✅ اربط الكاتيجوري بالهوم
//        homeVm.Categories.Add(categoryVm);
//    }

//    return homeVm;
//}
