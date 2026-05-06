namespace Traninig_Managment_system.BLL.Services.classes
{
    public abstract class InstructorServiceBase
    {
        protected readonly IInstructorRepo InstructorRepo;
        protected readonly ICourseRepo CourseRepo;
        protected readonly ICourseChapterRepo CourseChapterRepo;

        protected InstructorServiceBase(
            IInstructorRepo instructorRepo,
            ICourseRepo courseRepo,
            ICourseChapterRepo courseChapterRepo)
        {
            InstructorRepo = instructorRepo;
            CourseRepo = courseRepo;
            CourseChapterRepo = courseChapterRepo;
        }

        protected async Task<Instructor?> ResolveInstructorAsync(string userId)
        {
            return await InstructorRepo.GetOneAsync(i => i.UserId == userId && i.IsActive);
        }

        protected async Task<Course?> GetOwnedCourseAsync(int courseId, Instructor instructor)
        {
            return await CourseRepo.GetOneAsync(
                c => c.Id == courseId && c.InstructorId == instructor.Id,
                c => c.Category);
        }

        protected async Task<CourseChapter?> GetOwnedChapterAsync(int chapterId, Instructor instructor, bool asNoTracking = false)
        {
            var chapter = await CourseChapterRepo.GetOneAsync(
                ch => ch.Id == chapterId,
                ch => ch.Course);

            if (chapter == null || chapter.Course.InstructorId != instructor.Id)
            {
                return null;
            }

            return chapter;
        }

        protected async Task<ServiceResult<CourseChapter?>> ValidateChapterForCourseAsync(int courseId, int? chapterId, Instructor instructor)
        {
            if (!chapterId.HasValue)
            {
                return Ok<CourseChapter?>(null, string.Empty);
            }

            var chapter = await GetOwnedChapterAsync(chapterId.Value, instructor, asNoTracking: true);
            if (chapter == null || chapter.CourseId != courseId)
            {
                return new ServiceResult<CourseChapter?>
                {
                    IsSuccess = false,
                    Message = "الشابتر غير موجود أو لا يتبع هذا الكورس."
                };
            }

            return Ok<CourseChapter?>(chapter, string.Empty);
        }

        protected async Task<List<InstructorChapterOptionVm>> BuildChapterOptionsAsync(int courseId)
        {
            var chapters = await CourseChapterRepo.GetAllAsync(ch => ch.CourseId == courseId);

            return chapters
                .OrderBy(ch => ch.Order)
                .ThenBy(ch => ch.Title)
                .Select(ch => new InstructorChapterOptionVm
                {
                    Id = ch.Id,
                    Title = ch.Title,
                    Order = ch.Order
                })
                .ToList();
        }

        protected static InstructorCourseCardVm MapCourseCard(Course course)
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

        protected static InstructorChapterVm MapChapter(CourseChapter chapter)
        {
            return MapChapter(chapter, chapter.Lessons, chapter.Exams);
        }

        protected static InstructorChapterVm MapChapter(
            CourseChapter chapter,
            IEnumerable<Lesson> lessons,
            IEnumerable<Exam> exams)
        {
            var lessonRows = lessons
                .OrderBy(l => l.Order)
                .Select(l => MapLesson(l, chapter.Title))
                .ToList();

            var examRows = exams
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
                LessonCount = lessonRows.Count,
                ExamCount = examRows.Count,
                AverageProgress = lessonRows.Any()
                    ? Math.Round(lessonRows.Average(l => l.AverageWatchedPercentage), 1)
                    : 0,
                Lessons = lessonRows,
                Exams = examRows
            };
        }

        protected static InstructorLessonVm MapLesson(Lesson lesson, string? chapterTitle = null)
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

        protected static InstructorExamVm MapExam(Exam exam, string? chapterTitle = null)
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

        protected static string? GetLessonContentUrl(Lesson lesson)
        {
            if (!string.IsNullOrWhiteSpace(lesson.VideoUrl))
            {
                return lesson.VideoUrl;
            }

            if (!string.IsNullOrWhiteSpace(lesson.PdfUrl))
            {
                return lesson.PdfUrl;
            }

            return null;
        }

        protected static void SetLessonContentUrl(Lesson lesson, string? contentUrl)
        {
            if (string.IsNullOrWhiteSpace(contentUrl))
            {
                return;
            }

            var extension = Path.GetExtension(contentUrl).ToLowerInvariant();
            lesson.VideoUrl = string.Empty;
            lesson.PdfUrl = string.Empty;

           
        }

        protected static List<InstructorExamQuestionFormVm> BuildEmptyQuestions()
        {
            return new List<InstructorExamQuestionFormVm>
            {
                new(),
                new(),
                new(),
                new(),
                new()
            };
        }

        protected static List<ExamQuestion> NormalizeQuestions(IEnumerable<InstructorExamQuestionFormVm>? questions)
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

        protected static ServiceResult<bool> ValidateExam(InstructorExamFormVm model, List<ExamQuestion> questions)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Fail(false, "عنوان الامتحان مطلوب.");
            }

            if (model.DurationMinutes < 5)
            {
                return Fail(false, "مدة الامتحان يجب أن تكون 5 دقائق على الأقل.");
            }

            if (model.PassingScore < 1 || model.PassingScore > 100)
            {
                return Fail(false, "درجة النجاح يجب أن تكون بين 1 و 100.");
            }

            if (!questions.Any())
            {
                return Fail(false, "أضف سؤال واحد على الأقل للامتحان.");
            }

            return Ok(true, string.Empty);
        }

        protected static string NormalizeCorrectOption(string? option)
        {
            var value = (option ?? "A").Trim().ToUpperInvariant();
            return value is "A" or "B" or "C" or "D" ? value : "A";
        }

        protected static ServiceResult<T> Ok<T>(T data, string message)
        {
            return new ServiceResult<T> { IsSuccess = true, Data = data, Message = message };
        }

        protected static ServiceResult<T> Fail<T>(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }

        protected static ServiceResult<T> Fail<T>(T data, string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Data = data, Message = message };
        }
    }
}
