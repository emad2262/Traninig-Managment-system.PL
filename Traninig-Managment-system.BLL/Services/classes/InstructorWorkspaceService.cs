using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.BLL.Services.Interfaces;

namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorWorkspaceService : IInstructorWorkspaceService
    {
        private readonly ApplicationDbContext _context;

        public InstructorWorkspaceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InstructorDashboardVm?> GetDashboardAsync(string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var courses = await _context.courses
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Category)
                .Include(c => c.Chapters)
                .Include(c => c.Lessons)
                .Include(c => c.EmployeeCourses)
                .Include(c => c.Exams)
                .Where(c => c.InstructorId == instructor.Id && c.Category.CompanyId == instructor.CompanyId)
                .OrderBy(c => c.Category.Name)
                .ThenBy(c => c.Title)
                .ToListAsync();

            var courseCards = courses.Select(MapCourseCard).ToList();
            var employeeRows = (await GetEmployeeProgressAsync(userId)).Take(8).ToList();

            return new InstructorDashboardVm
            {
                InstructorId = instructor.Id,
                InstructorName = instructor.FullName,
                TotalCourses = courses.Count,
                PublishedCourses = courses.Count(c => c.IsPublished),
                TotalChapters = courses.Sum(c => c.Chapters.Count),
                TotalLessons = courses.Sum(c => c.Lessons.Count),
                TotalExams = courses.Sum(c => c.Exams.Count),
                TotalAssignedEmployees = courses
                    .SelectMany(c => c.EmployeeCourses)
                    .Select(ec => ec.EmployeeId)
                    .Distinct()
                    .Count(),
                AverageProgress = courses.SelectMany(c => c.EmployeeCourses).Any()
                    ? Math.Round(courses.SelectMany(c => c.EmployeeCourses).Average(ec => ec.Progress), 1)
                    : 0,
                Courses = courseCards,
                Categories = courseCards
                    .GroupBy(c => c.CategoryName)
                    .Select(g => new InstructorCategoryVm
                    {
                        CategoryName = g.Key,
                        Courses = g.ToList()
                    })
                    .ToList(),
                RecentProgress = employeeRows
            };
        }

        public async Task<InstructorCourseDetailsVm?> GetCourseDetailsAsync(int courseId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var course = await _context.courses
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Category)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
                        .ThenInclude(l => l.EmployeeLessons)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Exams)
                        .ThenInclude(e => e.Questions)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.EmployeeLessons)
                .Include(c => c.EmployeeCourses)
                    .ThenInclude(ec => ec.Employee)
                        .ThenInclude(e => e.EmployeeBadges)
                .Include(c => c.Exams)
                    .ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(c =>
                    c.Id == courseId &&
                    c.InstructorId == instructor.Id &&
                    c.Category.CompanyId == instructor.CompanyId);

            if (course == null) return null;

            var employees = await GetEmployeeProgressAsync(userId, courseId);

            return new InstructorCourseDetailsVm
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Logo = string.IsNullOrWhiteSpace(course.logo) ? null : course.logo,
                CategoryName = course.Category.Name,
                IsPublished = course.IsPublished,
                DurationInHours = course.DurationInHours,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                ChapterCount = course.Chapters.Count,
                LessonCount = course.Lessons.Count,
                ExamCount = course.Exams.Count,
                AssignedEmployees = course.EmployeeCourses.Count,
                AverageProgress = course.EmployeeCourses.Any()
                    ? Math.Round(course.EmployeeCourses.Average(ec => ec.Progress), 1)
                    : 0,
                Chapters = course.Chapters
                    .OrderBy(ch => ch.Order)
                    .ThenBy(ch => ch.Title)
                    .Select(MapChapter)
                    .ToList(),
                Lessons = course.Lessons
                    .Where(l => !l.ChapterId.HasValue)
                    .OrderBy(l => l.Order)
                    .Select(l => MapLesson(l))
                    .ToList(),
                Employees = employees.ToList(),
                Exams = course.Exams
                    .Where(e => !e.ChapterId.HasValue)
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => MapExam(e))
                    .ToList()
            };
        }

        public async Task<InstructorChapterFormVm?> BuildChapterCreateModelAsync(int courseId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var course = await GetOwnedCourseAsync(courseId, instructor);
            if (course == null) return null;

            var nextOrder = await _context.CourseChapters
                .Where(ch => ch.CourseId == courseId)
                .Select(ch => (int?)ch.Order)
                .MaxAsync() ?? 0;

            return new InstructorChapterFormVm
            {
                CourseId = courseId,
                Order = nextOrder + 1
            };
        }

        public async Task<InstructorChapterFormVm?> GetChapterForEditAsync(int chapterId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var chapter = await GetOwnedChapterAsync(chapterId, instructor, asNoTracking: true);
            if (chapter == null) return null;

            return new InstructorChapterFormVm
            {
                Id = chapter.Id,
                CourseId = chapter.CourseId,
                Title = chapter.Title,
                Description = chapter.Description,
                Order = chapter.Order
            };
        }

        public async Task<ServiceResult<int>> CreateChapterAsync(InstructorChapterFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail<int>("لم يتم العثور على بيانات المدرب.");

            var course = await GetOwnedCourseAsync(model.CourseId, instructor);
            if (course == null)
                return Fail<int>("الكورس غير موجود أو لا يتبع هذا المدرب.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return Fail<int>("عنوان الشابتر مطلوب.");

            var nextOrder = await _context.CourseChapters
                .Where(ch => ch.CourseId == model.CourseId)
                .Select(ch => (int?)ch.Order)
                .MaxAsync() ?? 0;

            var chapter = new CourseChapter
            {
                CourseId = model.CourseId,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                Order = model.Order > 0 ? model.Order : nextOrder + 1,
                CreatedAt = DateTime.UtcNow
            };

            await _context.CourseChapters.AddAsync(chapter);
            await _context.SaveChangesAsync();

            return Ok(chapter.Id, "تم إضافة الشابتر بنجاح.");
        }

        public async Task<ServiceResult<bool>> UpdateChapterAsync(InstructorChapterFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail(false, "لم يتم العثور على بيانات المدرب.");

            var chapter = await GetOwnedChapterAsync(model.Id, instructor);
            if (chapter == null || chapter.CourseId != model.CourseId)
                return Fail(false, "الشابتر غير موجود أو لا يتبع هذا المدرب.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return Fail(false, "عنوان الشابتر مطلوب.");

            chapter.Title = model.Title.Trim();
            chapter.Description = model.Description?.Trim() ?? string.Empty;
            chapter.Order = model.Order > 0 ? model.Order : chapter.Order;

            await _context.SaveChangesAsync();
            return Ok(true, "تم تعديل الشابتر بنجاح.");
        }

        public async Task<ServiceResult<bool>> DeleteChapterAsync(int chapterId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail(false, "لم يتم العثور على بيانات المدرب.");

            var chapter = await _context.CourseChapters
                .Include(ch => ch.Course)
                    .ThenInclude(c => c.Category)
                .Include(ch => ch.Lessons)
                .Include(ch => ch.Exams)
                .FirstOrDefaultAsync(ch =>
                    ch.Id == chapterId &&
                    ch.Course.InstructorId == instructor.Id &&
                    ch.Course.Category.CompanyId == instructor.CompanyId);

            if (chapter == null)
                return Fail(false, "الشابتر غير موجود.");

            if (chapter.Lessons.Any() || chapter.Exams.Any())
                return Fail(false, "لا يمكن حذف الشابتر قبل نقل أو حذف الدروس والامتحان المرتبط به.");

            _context.CourseChapters.Remove(chapter);
            await _context.SaveChangesAsync();

            return Ok(true, "تم حذف الشابتر بنجاح.");
        }

        public async Task<InstructorLessonFormVm?> BuildLessonCreateModelAsync(int courseId, string userId, int? chapterId = null)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var course = await GetOwnedCourseAsync(courseId, instructor);
            if (course == null) return null;

            CourseChapter? selectedChapter = null;
            if (chapterId.HasValue)
            {
                selectedChapter = await GetOwnedChapterAsync(chapterId.Value, instructor, asNoTracking: true);
                if (selectedChapter == null || selectedChapter.CourseId != courseId) return null;
            }

            var nextOrder = await _context.lessons
                .Where(l => l.CourseId == courseId && l.ChapterId == chapterId)
                .Select(l => (int?)l.Order)
                .MaxAsync() ?? 0;

            return new InstructorLessonFormVm
            {
                CourseId = courseId,
                ChapterId = selectedChapter?.Id,
                ChapterOptions = await BuildChapterOptionsAsync(courseId),
                Order = nextOrder + 1
            };
        }

        public async Task<InstructorLessonFormVm?> GetLessonForEditAsync(int lessonId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var lesson = await _context.lessons
                .AsNoTracking()
                .Include(l => l.Courses)
                    .ThenInclude(c => c.Category)
                .Include(l => l.Chapter)
                .FirstOrDefaultAsync(l =>
                    l.Id == lessonId &&
                    l.Courses.InstructorId == instructor.Id &&
                    l.Courses.Category.CompanyId == instructor.CompanyId);

            if (lesson == null) return null;

            return new InstructorLessonFormVm
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                ChapterId = lesson.ChapterId,
                ChapterOptions = await BuildChapterOptionsAsync(lesson.CourseId),
                Title = lesson.Title,
                Description = lesson.Content,
                Order = lesson.Order,
                ExistingContentUrl = GetLessonContentUrl(lesson)
            };
        }

        public async Task<ServiceResult<int>> CreateLessonAsync(InstructorLessonFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail<int>("لم يتم العثور على بيانات المدرب.");

            var course = await GetOwnedCourseAsync(model.CourseId, instructor);
            if (course == null)
                return Fail<int>("الكورس غير موجود أو لا يتبع هذا المدرب.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return Fail<int>("عنوان الدرس مطلوب.");

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
                return Fail<int>(chapterValidation.Message);

            var nextOrder = await _context.lessons
                .Where(l => l.CourseId == model.CourseId && l.ChapterId == model.ChapterId)
                .Select(l => (int?)l.Order)
                .MaxAsync() ?? 0;

            var lesson = new Lesson
            {
                Title = model.Title.Trim(),
                Content = model.Description?.Trim() ?? string.Empty,
                Order = model.Order > 0 ? model.Order : nextOrder + 1,
                CourseId = model.CourseId,
                ChapterId = model.ChapterId,
                CreatedAt = DateTime.UtcNow,
                EmployeeLessons = new List<EmployeeLesson>()
            };

            SetLessonContentUrl(lesson, model.ExistingContentUrl);

            await _context.lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            return Ok(lesson.Id, "تم إضافة الدرس بنجاح.");
        }

        public async Task<ServiceResult<bool>> UpdateLessonAsync(InstructorLessonFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail(false, "لم يتم العثور على بيانات المدرب.");

            var lesson = await _context.lessons
                .Include(l => l.Courses)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(l =>
                    l.Id == model.Id &&
                    l.CourseId == model.CourseId &&
                    l.Courses.InstructorId == instructor.Id &&
                    l.Courses.Category.CompanyId == instructor.CompanyId);

            if (lesson == null)
                return Fail(false, "الدرس غير موجود أو لا يتبع هذا المدرب.");

            if (string.IsNullOrWhiteSpace(model.Title))
                return Fail(false, "عنوان الدرس مطلوب.");

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
                return Fail(false, chapterValidation.Message);

            lesson.Title = model.Title.Trim();
            lesson.Content = model.Description?.Trim() ?? string.Empty;
            lesson.Order = model.Order > 0 ? model.Order : lesson.Order;
            lesson.ChapterId = model.ChapterId;

            if (!string.IsNullOrWhiteSpace(model.ExistingContentUrl))
            {
                SetLessonContentUrl(lesson, model.ExistingContentUrl);
            }

            await _context.SaveChangesAsync();
            return Ok(true, "تم تعديل الدرس بنجاح.");
        }

        public async Task<ServiceResult<string?>> DeleteLessonAsync(int lessonId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return new ServiceResult<string?> { IsSuccess = false, Message = "لم يتم العثور على بيانات المدرب." };

            var lesson = await _context.lessons
                .Include(l => l.Courses)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(l =>
                    l.Id == lessonId &&
                    l.Courses.InstructorId == instructor.Id &&
                    l.Courses.Category.CompanyId == instructor.CompanyId);

            if (lesson == null)
                return new ServiceResult<string?> { IsSuccess = false, Message = "الدرس غير موجود." };

            if (await _context.EmployeeLessons.AnyAsync(el => el.LessonId == lessonId))
                return new ServiceResult<string?> { IsSuccess = false, Message = "لا يمكن حذف الدرس لأنه مرتبط بتقدم موظفين." };

            var contentUrl = GetLessonContentUrl(lesson);
            _context.lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return new ServiceResult<string?> { IsSuccess = true, Data = contentUrl, Message = "تم حذف الدرس بنجاح." };
        }

        public async Task<IEnumerable<InstructorEmployeeProgressVm>> GetEmployeeProgressAsync(string userId, int? courseId = null)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return Enumerable.Empty<InstructorEmployeeProgressVm>();

            var rows = await _context.EmployeeCourses
                .AsNoTracking()
                .Where(ec =>
                    ec.Course.InstructorId == instructor.Id &&
                    ec.Course.Category.CompanyId == instructor.CompanyId &&
                    (!courseId.HasValue || ec.CourseId == courseId.Value))
                .Select(ec => new InstructorEmployeeProgressVm
                {
                    EmployeeId = ec.EmployeeId,
                    Name = ec.Employee.Name,
                    Email = ec.Employee.Email,
                    JobTitle = ec.Employee.JobTitle,
                    IsActive = ec.Employee.IsActive,
                    CourseId = ec.CourseId,
                    CourseName = ec.Course.Title,
                    Progress = Math.Round(ec.Progress, 1),
                    Status = ec.Status.ToString(),
                    FinalScore = ec.FinalScore,
                    AssignedAt = ec.AssignedAt,
                    LastAccessedAt = ec.LastAccessedAt,
                    CompletedAt = ec.CompletedAt,
                    TotalLessons = ec.Course.Lessons.Count,
                    CompletedLessons = _context.EmployeeLessons.Count(el =>
                        el.EmployeeId == ec.EmployeeId &&
                        el.Lesson.CourseId == ec.CourseId &&
                        el.IsCompleted),
                    BadgeCount = ec.Employee.EmployeeBadges.Count
                })
                .OrderByDescending(ec => ec.LastAccessedAt ?? ec.AssignedAt)
                .ToListAsync();

            return rows;
        }

        public async Task<InstructorEmployeeDetailsVm?> GetEmployeeDetailsAsync(int employeeId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var employee = await _context.employees
                .AsNoTracking()
                .Include(e => e.EmployeeBadges)
                    .ThenInclude(eb => eb.Badge)
                .FirstOrDefaultAsync(e =>
                    e.Id == employeeId &&
                    e.CompanyId == instructor.CompanyId &&
                    e.EmployeeCourses.Any(ec => ec.Course.InstructorId == instructor.Id));

            if (employee == null) return null;

            var progress = await GetEmployeeProgressAsync(userId);

            return new InstructorEmployeeDetailsVm
            {
                EmployeeId = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                IsActive = employee.IsActive,
                Points = employee.Points,
                Courses = progress.Where(p => p.EmployeeId == employee.Id).ToList(),
                Badges = employee.EmployeeBadges
                    .OrderByDescending(b => b.EarnedAt)
                    .Select(b => new InstructorBadgeVm
                    {
                        Name = b.Badge.Name,
                        Tier = b.Badge.Tier,
                        Points = b.Badge.Points,
                        EarnedAt = b.EarnedAt
                    })
                    .ToList()
            };
        }

        public async Task<InstructorExamFormVm?> BuildExamCreateModelAsync(int courseId, string userId, int? chapterId = null)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var course = await GetOwnedCourseAsync(courseId, instructor);
            if (course == null) return null;

            CourseChapter? selectedChapter = null;
            if (chapterId.HasValue)
            {
                selectedChapter = await GetOwnedChapterAsync(chapterId.Value, instructor, asNoTracking: true);
                if (selectedChapter == null || selectedChapter.CourseId != courseId) return null;
            }

            return new InstructorExamFormVm
            {
                CourseId = courseId,
                ChapterId = selectedChapter?.Id,
                ChapterTitle = selectedChapter?.Title,
                ChapterOptions = await BuildChapterOptionsAsync(courseId),
                Questions = BuildEmptyQuestions()
            };
        }

        public async Task<InstructorExamFormVm?> GetExamForEditAsync(int examId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null) return null;

            var exam = await _context.Exams
                .AsNoTracking()
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Chapter)
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e =>
                    e.Id == examId &&
                    e.Course.InstructorId == instructor.Id &&
                    e.Course.Category.CompanyId == instructor.CompanyId);

            if (exam == null) return null;

            var questions = exam.Questions
                .OrderBy(q => q.Id)
                .Select(q => new InstructorExamQuestionFormVm
                {
                    Id = q.Id,
                    Text = q.Text,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectOption = q.CorrectOption,
                    Points = q.Points
                })
                .ToList();

            while (questions.Count < 3)
            {
                questions.Add(new InstructorExamQuestionFormVm());
            }

            return new InstructorExamFormVm
            {
                Id = exam.Id,
                CourseId = exam.CourseId,
                ChapterId = exam.ChapterId,
                ChapterTitle = exam.Chapter?.Title,
                ChapterOptions = await BuildChapterOptionsAsync(exam.CourseId),
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                PassingScore = exam.PassingScore,
                IsPublished = exam.IsPublished,
                Questions = questions
            };
        }

        public async Task<ServiceResult<int>> CreateExamAsync(InstructorExamFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail<int>("لم يتم العثور على بيانات المدرب.");

            var course = await GetOwnedCourseAsync(model.CourseId, instructor);
            if (course == null)
                return Fail<int>("الكورس غير موجود أو لا يتبع هذا المدرب.");

            if (!model.ChapterId.HasValue)
                return Fail<int>("اختار الشابتر الذي سيظهر بعده الامتحان.");

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
                return Fail<int>(chapterValidation.Message);

            if (await _context.Exams.AnyAsync(e => e.ChapterId == model.ChapterId.Value))
                return Fail<int>("هذا الشابتر عليه امتحان بالفعل. عدل الامتحان الموجود أو احذفه أولاً.");

            var questions = NormalizeQuestions(model.Questions);
            var validation = ValidateExam(model, questions);
            if (!validation.IsSuccess)
                return Fail<int>(validation.Message);

            var exam = new Exam
            {
                CourseId = model.CourseId,
                ChapterId = model.ChapterId,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                DurationMinutes = model.DurationMinutes,
                PassingScore = model.PassingScore,
                IsPublished = model.IsPublished,
                CreatedAt = DateTime.UtcNow,
                Questions = questions
            };

            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();

            return Ok(exam.Id, "تم إضافة الامتحان للشابتر بنجاح.");
        }

        public async Task<ServiceResult<bool>> UpdateExamAsync(InstructorExamFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail(false, "لم يتم العثور على بيانات المدرب.");

            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e =>
                    e.Id == model.Id &&
                    e.CourseId == model.CourseId &&
                    e.Course.InstructorId == instructor.Id &&
                    e.Course.Category.CompanyId == instructor.CompanyId);

            if (exam == null)
                return Fail(false, "الامتحان غير موجود أو لا يتبع هذا المدرب.");

            if (!model.ChapterId.HasValue)
                return Fail(false, "اختار الشابتر الذي سيظهر بعده الامتحان.");

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
                return Fail(false, chapterValidation.Message);

            if (await _context.Exams.AnyAsync(e => e.Id != model.Id && e.ChapterId == model.ChapterId.Value))
                return Fail(false, "هذا الشابتر عليه امتحان بالفعل.");

            var questions = NormalizeQuestions(model.Questions);
            var validation = ValidateExam(model, questions);
            if (!validation.IsSuccess)
                return Fail(false, validation.Message);

            exam.ChapterId = model.ChapterId;
            exam.Title = model.Title.Trim();
            exam.Description = model.Description?.Trim() ?? string.Empty;
            exam.DurationMinutes = model.DurationMinutes;
            exam.PassingScore = model.PassingScore;
            exam.IsPublished = model.IsPublished;

            _context.ExamQuestions.RemoveRange(exam.Questions);
            exam.Questions = questions;

            await _context.SaveChangesAsync();
            return Ok(true, "تم تعديل الامتحان بنجاح.");
        }

        public async Task<ServiceResult<bool>> DeleteExamAsync(int examId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail(false, "لم يتم العثور على بيانات المدرب.");

            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(e =>
                    e.Id == examId &&
                    e.Course.InstructorId == instructor.Id &&
                    e.Course.Category.CompanyId == instructor.CompanyId);

            if (exam == null)
                return Fail(false, "الامتحان غير موجود.");

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return Ok(true, "تم حذف الامتحان بنجاح.");
        }

        public async Task<ServiceResult<bool>> ToggleExamPublishAsync(int examId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
                return Fail(false, "لم يتم العثور على بيانات المدرب.");

            var exam = await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(e =>
                    e.Id == examId &&
                    e.Course.InstructorId == instructor.Id &&
                    e.Course.Category.CompanyId == instructor.CompanyId);

            if (exam == null)
                return Fail(false, "الامتحان غير موجود.");

            exam.IsPublished = !exam.IsPublished;
            await _context.SaveChangesAsync();

            return Ok(
                exam.IsPublished,
                exam.IsPublished ? "تم نشر الامتحان." : "تم تحويل الامتحان إلى مسودة.");
        }

        private async Task<Instructor?> ResolveInstructorAsync(string userId)
        {
            return await _context.instructors
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.UserId == userId && i.IsActive);
        }

        private async Task<Course?> GetOwnedCourseAsync(int courseId, Instructor instructor)
        {
            return await _context.courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c =>
                    c.Id == courseId &&
                    c.InstructorId == instructor.Id &&
                    c.Category.CompanyId == instructor.CompanyId);
        }

        private async Task<CourseChapter?> GetOwnedChapterAsync(int chapterId, Instructor instructor, bool asNoTracking = false)
        {
            var query = _context.CourseChapters
                .Include(ch => ch.Course)
                    .ThenInclude(c => c.Category)
                .Where(ch =>
                    ch.Id == chapterId &&
                    ch.Course.InstructorId == instructor.Id &&
                    ch.Course.Category.CompanyId == instructor.CompanyId);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

        private async Task<ServiceResult<CourseChapter?>> ValidateChapterForCourseAsync(int courseId, int? chapterId, Instructor instructor)
        {
            if (!chapterId.HasValue)
                return Ok<CourseChapter?>(null, string.Empty);

            var chapter = await GetOwnedChapterAsync(chapterId.Value, instructor, asNoTracking: true);
            if (chapter == null || chapter.CourseId != courseId)
                return new ServiceResult<CourseChapter?> { IsSuccess = false, Message = "الشابتر غير موجود أو لا يتبع هذا الكورس." };

            return Ok<CourseChapter?>(chapter, string.Empty);
        }

        private async Task<List<InstructorChapterOptionVm>> BuildChapterOptionsAsync(int courseId)
        {
            return await _context.CourseChapters
                .AsNoTracking()
                .Where(ch => ch.CourseId == courseId)
                .OrderBy(ch => ch.Order)
                .ThenBy(ch => ch.Title)
                .Select(ch => new InstructorChapterOptionVm
                {
                    Id = ch.Id,
                    Title = ch.Title,
                    Order = ch.Order
                })
                .ToListAsync();
        }

        private static InstructorCourseCardVm MapCourseCard(Course course)
        {
            return new InstructorCourseCardVm
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Logo = string.IsNullOrWhiteSpace(course.logo) ? null : course.logo,
                CategoryName = course.Category?.Name ?? "Training",
                IsPublished = course.IsPublished,
                DurationInHours = course.DurationInHours,
                ChapterCount = course.Chapters.Count,
                LessonCount = course.Lessons.Count,
                ExamCount = course.Exams.Count,
                AssignedEmployees = course.EmployeeCourses.Count,
                AverageProgress = course.EmployeeCourses.Any()
                    ? Math.Round(course.EmployeeCourses.Average(ec => ec.Progress), 1)
                    : 0
            };
        }

        private static InstructorChapterVm MapChapter(CourseChapter chapter)
        {
            var lessons = chapter.Lessons
                .OrderBy(l => l.Order)
                .Select(l => MapLesson(l, chapter.Title))
                .ToList();

            var exams = chapter.Exams
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => MapExam(e, chapter.Title))
                .ToList();

            return new InstructorChapterVm
            {
                Id = chapter.Id,
                CourseId = chapter.CourseId,
                Title = chapter.Title,
                Description = chapter.Description,
                Order = chapter.Order,
                CreatedAt = chapter.CreatedAt,
                LessonCount = lessons.Count,
                ExamCount = exams.Count,
                AverageProgress = lessons.Any()
                    ? Math.Round(lessons.Average(l => l.AverageWatchedPercentage), 1)
                    : 0,
                Lessons = lessons,
                Exams = exams
            };
        }

        private static InstructorLessonVm MapLesson(Lesson lesson, string? chapterTitle = null)
        {
            return new InstructorLessonVm
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                ChapterId = lesson.ChapterId,
                ChapterTitle = chapterTitle ?? lesson.Chapter?.Title,
                Title = lesson.Title,
                Description = lesson.Content,
                ContentUrl = GetLessonContentUrl(lesson),
                Order = lesson.Order,
                CreatedAt = lesson.CreatedAt,
                CompletedEmployees = lesson.EmployeeLessons?.Count(el => el.IsCompleted) ?? 0,
                AverageWatchedPercentage = lesson.EmployeeLessons != null && lesson.EmployeeLessons.Any()
                    ? Math.Round(lesson.EmployeeLessons.Average(el => el.WatchedPercentage), 1)
                    : 0
            };
        }

        private static InstructorExamVm MapExam(Exam exam, string? chapterTitle = null)
        {
            return new InstructorExamVm
            {
                Id = exam.Id,
                CourseId = exam.CourseId,
                ChapterId = exam.ChapterId,
                ChapterTitle = chapterTitle ?? exam.Chapter?.Title,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                PassingScore = exam.PassingScore,
                IsPublished = exam.IsPublished,
                CreatedAt = exam.CreatedAt,
                QuestionCount = exam.Questions.Count
            };
        }

        private static string? GetLessonContentUrl(Lesson lesson)
        {
            if (!string.IsNullOrWhiteSpace(lesson.VideoUrl)) return lesson.VideoUrl;
            if (!string.IsNullOrWhiteSpace(lesson.PdfUrl)) return lesson.PdfUrl;
            return null;
        }

        private static void SetLessonContentUrl(Lesson lesson, string? contentUrl)
        {
            if (string.IsNullOrWhiteSpace(contentUrl)) return;

            var extension = Path.GetExtension(contentUrl).ToLowerInvariant();
            lesson.VideoUrl = string.Empty;
            lesson.PdfUrl = string.Empty;

            if (extension == ".pdf")
            {
                lesson.PdfUrl = contentUrl;
            }
            else
            {
                lesson.VideoUrl = contentUrl;
            }
        }

        private static List<InstructorExamQuestionFormVm> BuildEmptyQuestions()
        {
            return new List<InstructorExamQuestionFormVm>
            {
                new(),
                new(),
                new()
            };
        }

        private static List<ExamQuestion> NormalizeQuestions(IEnumerable<InstructorExamQuestionFormVm>? questions)
        {
            return (questions ?? Enumerable.Empty<InstructorExamQuestionFormVm>())
                .Where(q => !string.IsNullOrWhiteSpace(q.Text))
                .Select(q => new ExamQuestion
                {
                    Text = q.Text.Trim(),
                    OptionA = q.OptionA?.Trim() ?? string.Empty,
                    OptionB = q.OptionB?.Trim() ?? string.Empty,
                    OptionC = q.OptionC?.Trim() ?? string.Empty,
                    OptionD = q.OptionD?.Trim() ?? string.Empty,
                    CorrectOption = NormalizeCorrectOption(q.CorrectOption),
                    Points = q.Points <= 0 ? 1 : q.Points
                })
                .ToList();
        }

        private static ServiceResult<bool> ValidateExam(InstructorExamFormVm model, List<ExamQuestion> questions)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return Fail(false, "عنوان الامتحان مطلوب.");

            if (model.DurationMinutes < 5)
                return Fail(false, "مدة الامتحان يجب أن تكون 5 دقائق على الأقل.");

            if (model.PassingScore < 1 || model.PassingScore > 100)
                return Fail(false, "درجة النجاح يجب أن تكون بين 1 و 100.");

            if (!questions.Any())
                return Fail(false, "أضف سؤال واحد على الأقل للامتحان.");

            if (questions.Any(q => string.IsNullOrWhiteSpace(q.OptionA) || string.IsNullOrWhiteSpace(q.OptionB)))
                return Fail(false, "كل سؤال يحتاج اختيارين على الأقل A و B.");

            return Ok(true, string.Empty);
        }

        private static string NormalizeCorrectOption(string? option)
        {
            var value = (option ?? "A").Trim().ToUpperInvariant();
            return value is "A" or "B" or "C" or "D" ? value : "A";
        }

        private static ServiceResult<T> Ok<T>(T data, string message)
        {
            return new ServiceResult<T> { IsSuccess = true, Data = data, Message = message };
        }

        private static ServiceResult<T> Fail<T>(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }

        private static ServiceResult<T> Fail<T>(T data, string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Data = data, Message = message };
        }
    }
}
