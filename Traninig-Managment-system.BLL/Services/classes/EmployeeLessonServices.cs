using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.DAL.Data;
using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class EmployeeLessonServices : IEmployeeLessonServices
    {
        private readonly ApplicationDbContext _context;

        public EmployeeLessonServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<EmployeeDashboardVm> GetEmployeeDashboardAsync(string userId)
        {
            var employee = await _context.employees
       .Include(e => e.EmployeeCourses)
       .ThenInclude(ec => ec.Course)
       .ThenInclude(c => c.Lessons)
       .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
            {
                return new EmployeeDashboardVm
                {
                    Courses = new List<CourseProgressVm>()
                };
            }

            var vm = new EmployeeDashboardVm();

            vm.TotalCourses = employee.EmployeeCourses.Count;
            vm.CompletedCourses = employee.EmployeeCourses.Count(ec => ec.IsCompleted);
            vm.TotalPoints = employee.Points;

            vm.Courses = new List<CourseProgressVm>();

            foreach (var ec in employee.EmployeeCourses)
            {
                var totalLessons = ec.Course.Lessons.Count;

                var completedLessons = await _context.EmployeeLessons
                    .CountAsync(el =>
                        el.EmployeeId == employee.Id &&
                        el.Lesson.CourseId == ec.CourseId &&
                        el.IsCompleted);

                var percentage = totalLessons == 0
                    ? 0
                    : (completedLessons * 100) / totalLessons;

                vm.TotalCompletedLessons += completedLessons;

                vm.Courses.Add(new CourseProgressVm
                {
                    CourseId = ec.CourseId,
                    Title = ec.Course.Title,
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    ProgressPercentage = percentage
                });
            }

            return vm;
        }

        public async Task<CourseLessons?> GetCourseWithLessonsAsync(string userId, int courseId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;


            var employeeId = await _context.employees
                .AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();

            if (employeeId is null)
                return null;

            var isEnrolled = await _context.EmployeeCourses
                .AsNoTracking()
                .AnyAsync(ec => ec.EmployeeId == employeeId.Value && ec.CourseId == courseId);

            if (!isEnrolled)
                return null;

            // 3) Get course + lessons (ممكن نعمل Projection بدل Include لتقليل الداتا)
            var course = await _context.courses
                .AsNoTracking()
                .Where(c => c.Id == courseId)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    InstructorName = c.Instructor != null ? c.Instructor.FullName : "N/A",
                    CategoryName = c.Category != null ? c.Category.Name : "N/A",
                    Lessons = c.Lessons
                        .OrderBy(l => l.Order)
                        .Select(l => new
                        {
                            l.Id,
                            l.Title,
                            l.Content,
                            l.VideoUrl,
                            l.PdfUrl,
                            l.Order
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (course == null)
                return null;

            // 4) Completed lessons for this employee IN THIS COURSE فقط ✅
            var completedLessonIds = await _context.EmployeeLessons
                .AsNoTracking()
                .Where(el =>
                    el.EmployeeId == employeeId.Value &&
                    el.IsCompleted &&
                    _context.lessons.Any(l => l.Id == el.LessonId && l.CourseId == courseId))
                .Select(el => el.LessonId)
                .ToListAsync();

            var completedSet = completedLessonIds.ToHashSet();

            // 5) Build lessons with lock logic
            var lessonsVm = new List<LessonVm>(course.Lessons.Count);

            bool canOpen = true; // أول درس مفتوح

            foreach (var lesson in course.Lessons)
            {
                bool isCompleted = completedSet.Contains(lesson.Id);

                lessonsVm.Add(new LessonVm
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Content,
                    VideoUrl = lesson.VideoUrl,
                    PdfUrl = lesson.PdfUrl,
                    Order = lesson.Order,
                    IsCompleted = isCompleted,
                    IsLocked = !canOpen
                });

                // اللي بعده يتفتح لو الحالي مكتمل
                canOpen = isCompleted;
            }

            int total = lessonsVm.Count;
            int completed = lessonsVm.Count(x => x.IsCompleted);
            int percentage = total > 0 ? (int)Math.Round((completed * 100.0) / total) : 0;

            return new CourseLessons
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                InstructorName = course.InstructorName,
                CategoryName = course.CategoryName,
                TotalLessons = total,
                CompletedLessons = completed,
                ProgressPercentage = percentage,
                Lessons = lessonsVm
            };
        }



        public async Task MarkLessonAsCompletedAsync(string userId, int lessonId)
        {
            var employee = await _context.employees
                .FirstOrDefaultAsync(e => e.UserId == userId);

            var record = await _context.EmployeeLessons
                .FirstOrDefaultAsync(el =>
                    el.EmployeeId == employee.Id &&
                    el.LessonId == lessonId);

            if (record == null)
            {
                record = new EmployeeLesson
                {
                    EmployeeId = employee.Id,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                _context.EmployeeLessons.Add(record);
            }
            else
            {
                record.IsCompleted = true;
                record.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

    }
}
