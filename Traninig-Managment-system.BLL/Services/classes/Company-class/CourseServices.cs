using Microsoft.AspNetCore.Components.Forms;
using Traninig_Managment_system.BLL.Dtos;
using Traninig_Managment_system.BLL.Dtos.Course;
using Traninig_Managment_system.BLL.Dtos.Lessons;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class CourseServices : ICourseServices
    {
        private readonly ICourseRepo _courseRepo;

        public CourseServices(ICourseRepo courseRepo)
        {
            _courseRepo = courseRepo;
        }
        public  async Task<IEnumerable<ListCourseDto>> GetAllCoursesInCategoryAsync(int companyId, int categoryId)
        {
            var categorycourses =await _courseRepo.GetAllAsync(e=>e.CategoryId==categoryId && e.Category.CompanyId==companyId);
            return categorycourses.Select(e => new ListCourseDto
            {
                Id = e.Id,
                Title = e.Title,
                DurationInHours = e.DurationInHours,
                IsPublished = e.IsPublished,
                logo=e.logo,
                LessonCount = e.Lessons.Count

            }).ToList();
        }

        public async Task<IEnumerable<ListCourseDto>> GetCompanyCoursesAsync(int companyId)
        {
            var allcourses = await _courseRepo.GetAllAsync(e=>e.Category.CompanyId==companyId);
            return allcourses.Select(e => new ListCourseDto
            {
                Id = e.Id,
                Title = e.Title,
                DurationInHours = e.DurationInHours,
                IsPublished = e.IsPublished,
                logo = e.logo,
                LessonCount = e.Lessons.Count

            }).ToList();

        }

        public async Task<CourseDetailsDto> GetCourseDetailsAsync(int companyId, int id)
        {

        var course= await _courseRepo.GetOneAsync(e=>e.Id==id && e.Category.CompanyId==companyId);
            return new CourseDetailsDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                logo = course.logo,
                DurationInHours = course.DurationInHours,
                IsPublished = course.IsPublished,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                InstructorName = course.Instructor.FullName,
                CategoryName = course.Category.Name,
                LessonsList = course.Lessons.Select(e => new LessonListDto
                {

                }).ToList()
            };
           
        }
        public  async Task<ServiceResult<int>> CreateCourseAsync(CreateCourseDto model, int companyId)
        {
            var createcourse= new Course
            {
                Title=model.Title,
                Description=model.Description,
                logo= model.logo,
                DurationInHours= model.DurationInHours,
                IsPublished = model.IsPublished,
                StartDate= model.StartDate,
                EndDate= model.EndDate,

            };
            return new ServiceResult<int>
            {
                Message="courses created",
                Data=createcourse.Id,
                IsSuccess=true
            };

        }
        public async Task EditCourse(UpdateCourseDto model, int companyId,int Id)
        {
            var course = await _courseRepo.GetOneAsync(e=>e.Id==Id&& e.Category.CompanyId==companyId);

            course.Id = model.Id;
            course.Title = model.Title;
            course.DurationInHours = model.DurationInHours;
            course.IsPublished = model.IsPublish;
            await _courseRepo.Update(course);
            await _courseRepo.SaveChangesAsync();
           

        }
        public async Task DeleteCourse(int Id,int CompanyId)
        {

        }
        public async Task<ServiceResult<bool>> TogglePublishAsync(int id, int companyId)
        {
            var course = await _courseRepo.GetOneAsync(
                c => c.Id == id &&
                     c.Category.CompanyId == companyId);

            if (course == null)
            {
                return new ServiceResult<bool>
                {
                    IsSuccess = false,
                    Message = "Course not found."
                };
            }

            course.IsPublished = !course.IsPublished;

           await _courseRepo.Update(course);

            var affectedRows =  _courseRepo.SaveChangesAsync();

           
            return new ServiceResult<bool>
            {
                IsSuccess = true,
                Message = course.IsPublished
                    ? "Course published successfully."
                    : "Course moved back to draft."
            };
        }

       
    }
}
