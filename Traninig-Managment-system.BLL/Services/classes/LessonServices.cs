using Traninig_Managment_system.DAL.Repo;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class LessonServices : ILessonServices
    {
        private readonly ILessonRepo _lessonRepo;
        private readonly ICourseRepo _courseRepo;

        public LessonServices(ILessonRepo lessonRepo, ICourseRepo courseRepo)
        {
            _lessonRepo = lessonRepo;
            _courseRepo = courseRepo;
        }

        public async Task AddLessonToCourseAsync( int companyId,int courseId, CreateLessonVm model,string instructorUserId)
        {
            // 1️⃣ تأكد إن الكورس تابع لشركة عن طريق الكاتيجوري
            var course = await _courseRepo.GetOneAsync(
            c => c.Id == courseId,
            c => c.Category,
            c => c.Instructor );

            if (course == null || course.Category.CompanyId != companyId)
                throw new Exception("Course not found or not authorized");

            if (course == null)
                throw new Exception("Course not found or not authorized");

            // 2️⃣ إنشاء الليسون
            var lesson = new Lesson
            {
                Title = model.Title,
                Content = model.Description,
                VideoUrl = model.ContentUrl,
                Order = model.Order,
                CourseId = courseId
            };

            // 3️⃣ Save
            await _lessonRepo.CreateAsync(lesson);
        }

        public async Task<bool> EditLessonToCourseAsync(int lessonId,int courseId,EditLessonVm model,string instructorUserId)
        {
            var lesson = await _lessonRepo.GetOneAsync(
                l => l.Id == lessonId
                     && l.CourseId == courseId
                     && l.Courses.Instructor.UserId == instructorUserId,
                l => l.Courses);

            if (lesson == null)
                return false;

            lesson.Title = model.Title;
            lesson.Content = model.Content;
            lesson.Order = model.Order;

            await _lessonRepo.UdateAsync(lesson);

            return true;
        }

        public async Task<EditLessonVm?> GetLessonForEditAsync(int lessonId,int courseId,string instructorUserId)
        {
            var lesson = await _lessonRepo.GetOneAsync(
                l => l.Id == lessonId
                     && l.CourseId == courseId
                     && l.Courses.Instructor.UserId == instructorUserId,
                l => l.Courses);

            if (lesson == null)
                return null;

            return new EditLessonVm
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,   // 👈 مهم جدًا
                Title = lesson.Title,
                Content = lesson.Content,
                Order = lesson.Order
            };
        }




















        /// <summary>
        /// /company
        /// </summary>

        public async Task<List<LessonDisplayVm>> GetLessonsByCourseAsync(int companyId,int courseId)
        {
            // تأكد إن الكورس تابع للشركة عن طريق الكاتيجوري
            var course = await _courseRepo.GetOneAsync(
                c => c.Id == courseId && c.Category.CompanyId == companyId,
                c => c.Lessons,
                c => c.Category
            );

            if (course == null)
                throw new Exception("Course not found or not authorized");

            return course.Lessons
                .OrderBy(l => l.Order)
                .Select(l => new LessonDisplayVm
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Content,
                    ContentUrl = l.VideoUrl,
                    Order = l.Order
                })
                .ToList();
        }
    }

}
