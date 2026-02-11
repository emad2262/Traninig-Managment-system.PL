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
        public async Task<CourseLessonsVm> GetCourseLessonsForEmployeeAsync(string userId, int courseId)
        {
            var employee = await _context.employees
                .Include(e => e.EmployeeCourses)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
                return null;

            var course = await _context.courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return null;

            // تأكد إن الموظف مشترك في الكورس
            if (!employee.EmployeeCourses.Any(ec => ec.CourseId == courseId))
                return null;

            var vm = new CourseLessonsVm
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Lessons = course.Lessons.Select(l => new LessonDisplayVm
                {
                    Id = l.Id,
                    Title = l.Title,
                    
                    
                }).ToList()
            };

            return vm;
        }
        /////////////////////////////////////
        ///
        public async Task<LessonDetailsVm> GetLessonDetailsForEmployeeAsync(string userId, int lessonId)
        {
            var employee = await _context.employees
                .Include(e => e.EmployeeCourses)
                .ThenInclude(ec => ec.Course)
                .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee == null)
                return null;

            var lesson = employee.EmployeeCourses
                .SelectMany(ec => ec.Course.Lessons)
                .FirstOrDefault(l => l.Id == lessonId);

            if (lesson == null)
                return null;

            // 🔥 بدل lesson.EmployeeLessons.Any()
            var isCompleted = await _context.EmployeeLessons
                .AnyAsync(el =>
                    el.EmployeeId == employee.Id &&
                    el.LessonId == lessonId &&
                    el.IsCompleted);

            return new LessonDetailsVm
            {
                LessonId = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Content,
                VideoUrl = lesson.VideoUrl,
                PdfUrl = lesson.PdfUrl,
                IsCompleted = isCompleted
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
