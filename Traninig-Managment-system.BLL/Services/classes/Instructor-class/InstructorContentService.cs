namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorContentService : InstructorServiceBase, IInstructorContentService
    {
        private readonly ILessonRepo _lessonRepo;
        private readonly IEmployeeLessonRepo _employeeLessonRepo;

        public InstructorContentService(
            IInstructorRepo instructorRepo,
            ICourseRepo courseRepo,
            ICourseChapterRepo courseChapterRepo,
            ILessonRepo lessonRepo,
            IEmployeeLessonRepo employeeLessonRepo)
            : base(instructorRepo, courseRepo, courseChapterRepo)
        {
            _lessonRepo = lessonRepo;
            _employeeLessonRepo = employeeLessonRepo;
        }

        public async Task<InstructorChapterFormVm?> BuildChapterCreateModelAsync(int courseId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return null;
            }

            var course = await GetOwnedCourseAsync(courseId, instructor);
            if (course == null)
            {
                return null;
            }

            var nextOrder = (await CourseChapterRepo.GetAllAsync(ch => ch.CourseId == courseId))
                .Select(ch => ch.Order)
                .DefaultIfEmpty(0)
                .Max();

            return new InstructorChapterFormVm
            {
                CourseId = courseId,
                Order = nextOrder + 1
            };
        }

        public async Task<InstructorChapterFormVm?> GetChapterForEditAsync(int chapterId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return null;
            }

            var chapter = await CourseChapterRepo.GetOneAsync(ch => ch.Id == chapterId, ch => ch.Course);
            if (chapter == null || chapter.Course.InstructorId != instructor.Id)
            {
                return null;
            }

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
            {
                return Fail<int>("لم يتم العثور على بيانات المدرب.");
            }

            var course = await GetOwnedCourseAsync(model.CourseId, instructor);
            if (course == null)
            {
                return Fail<int>("الكورس غير موجود أو لا يتبع هذا المدرب.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Fail<int>("عنوان الشابتر مطلوب.");
            }

            var nextOrder = (await CourseChapterRepo.GetAllAsync(ch => ch.CourseId == model.CourseId))
                .Select(ch => ch.Order)
                .DefaultIfEmpty(0)
                .Max();

            var chapter = new CourseChapter
            {
                CourseId = model.CourseId,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                Order = model.Order > 0 ? model.Order : nextOrder + 1,
                CreatedAt = DateTime.UtcNow
            };

            if (!await CourseChapterRepo.CreateAsync(chapter))
            {
                return Fail<int>("تعذر إضافة الشابتر حالياً.");
            }

            return Ok(chapter.Id, "تم إضافة الشابتر بنجاح.");
        }

        public async Task<ServiceResult<bool>> UpdateChapterAsync(InstructorChapterFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail(false, "لم يتم العثور على بيانات المدرب.");
            }

            var chapter = await CourseChapterRepo.GetOneAsync(ch => ch.Id == model.Id, ch => ch.Course);
            if (chapter == null || chapter.CourseId != model.CourseId || chapter.Course.InstructorId != instructor.Id)
            {
                return Fail(false, "الشابتر غير موجود أو لا يتبع هذا المدرب.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Fail(false, "عنوان الشابتر مطلوب.");
            }

            var updatedChapter = new CourseChapter
            {
                Id = chapter.Id,
                CourseId = chapter.CourseId,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                Order = model.Order > 0 ? model.Order : chapter.Order,
                CreatedAt = chapter.CreatedAt
            };

            if (!await CourseChapterRepo.UpdateAsync(updatedChapter))
            {
                return Fail(false, "تعذر تعديل الشابتر حالياً.");
            }

            return Ok(true, "تم تعديل الشابتر بنجاح.");
        }

        public async Task<ServiceResult<bool>> DeleteChapterAsync(int chapterId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail(false, "لم يتم العثور على بيانات المدرب.");
            }

            var chapter = await CourseChapterRepo.GetOneAsync(
                ch => ch.Id == chapterId,
                ch => ch.Course,
                ch => ch.Lessons,
                ch => ch.Exams);

            if (chapter == null || chapter.Course.InstructorId != instructor.Id)
            {
                return Fail(false, "الشابتر غير موجود.");
            }

            if (chapter.Lessons.Any() || chapter.Exams.Any())
            {
                return Fail(false, "لا يمكن حذف الشابتر قبل نقل أو حذف الدروس والامتحان المرتبط به.");
            }

            if (!await CourseChapterRepo.Delete(chapter))
            {
                return Fail(false, "تعذر حذف الشابتر حالياً.");
            }

            return Ok(true, "تم حذف الشابتر بنجاح.");
        }

        public async Task<InstructorLessonFormVm?> BuildLessonCreateModelAsync(int courseId, string userId, int? chapterId = null)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return null;
            }

            var course = await GetOwnedCourseAsync(courseId, instructor);
            if (course == null)
            {
                return null;
            }

            CourseChapter? selectedChapter = null;
            if (chapterId.HasValue)
            {
                selectedChapter = await GetOwnedChapterAsync(chapterId.Value, instructor, asNoTracking: true);
                if (selectedChapter == null || selectedChapter.CourseId != courseId)
                {
                    return null;
                }
            }

            var nextOrder = (await _lessonRepo.GetAllAsync(l => l.CourseId == courseId && l.ChapterId == chapterId))
                .Select(l => l.Order)
                .DefaultIfEmpty(0)
                .Max();

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
            if (instructor == null)
            {
                return null;
            }

            var lesson = await _lessonRepo.GetOneAsync(
                l => l.Id == lessonId,
                l => l.Courses,
                l => l.Chapter!);

            if (lesson == null || lesson.Courses.InstructorId != instructor.Id)
            {
                return null;
            }

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
            {
                return Fail<int>("لم يتم العثور على بيانات المدرب.");
            }

            var course = await GetOwnedCourseAsync(model.CourseId, instructor);
            if (course == null)
            {
                return Fail<int>("الكورس غير موجود أو لا يتبع هذا المدرب.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Fail<int>("عنوان الدرس مطلوب.");
            }

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
            {
                return Fail<int>(chapterValidation.Message);
            }

            var nextOrder = (await _lessonRepo.GetAllAsync(
                    l => l.CourseId == model.CourseId && l.ChapterId == model.ChapterId))
                .Select(l => l.Order)
                .DefaultIfEmpty(0)
                .Max();

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

            if (!await _lessonRepo.CreateAsync(lesson))
            {
                return Fail<int>("تعذر إضافة الدرس حالياً.");
            }

            return Ok(lesson.Id, "تم إضافة الدرس بنجاح.");
        }

        public async Task<ServiceResult<bool>> UpdateLessonAsync(InstructorLessonFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail(false, "لم يتم العثور على بيانات المدرب.");
            }

            var lesson = await _lessonRepo.GetOneAsync(
                l => l.Id == model.Id && l.CourseId == model.CourseId,
                l => l.Courses);

            if (lesson == null || lesson.Courses.InstructorId != instructor.Id)
            {
                return Fail(false, "الدرس غير موجود أو لا يتبع هذا المدرب.");
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Fail(false, "عنوان الدرس مطلوب.");
            }

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
            {
                return Fail(false, chapterValidation.Message);
            }

            var updatedLesson = new Lesson
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                ChapterId = model.ChapterId,
                Title = model.Title.Trim(),
                Content = model.Description?.Trim() ?? string.Empty,
                Order = model.Order > 0 ? model.Order : lesson.Order,
                CreatedAt = lesson.CreatedAt,
                VideoUrl = lesson.VideoUrl,
                PdfUrl = lesson.PdfUrl
            };

            if (!string.IsNullOrWhiteSpace(model.ExistingContentUrl))
            {
                SetLessonContentUrl(updatedLesson, model.ExistingContentUrl);
            }

            if (!await _lessonRepo.UpdateAsync(updatedLesson))
            {
                return Fail(false, "تعذر تعديل الدرس حالياً.");
            }

            return Ok(true, "تم تعديل الدرس بنجاح.");
        }

        public async Task<ServiceResult<string?>> DeleteLessonAsync(int lessonId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail<string?>(null, "لم يتم العثور على بيانات المدرب.");
            }

            var lesson = await _lessonRepo.GetOneAsync(
                l => l.Id == lessonId,
                l => l.Courses);

            if (lesson == null || lesson.Courses.InstructorId != instructor.Id)
            {
                return Fail<string?>(null, "الدرس غير موجود.");
            }

            if (await _employeeLessonRepo.CountAsync(el => el.LessonId == lessonId) > 0)
            {
                return Fail<string?>(null, "لا يمكن حذف الدرس لأنه مرتبط بتقدم موظفين.");
            }

            var contentUrl = GetLessonContentUrl(lesson);
            if (!await _lessonRepo.Delete(lesson))
            {
                return Fail<string?>(null, "تعذر حذف الدرس حالياً.");
            }

            return Ok(contentUrl, "تم حذف الدرس بنجاح.");
        }
    }
}
