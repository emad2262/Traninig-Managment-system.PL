namespace Traninig_Managment_system.BLL.Services.classes
{
    public class InstructorExamService : InstructorServiceBase, IInstructorExamService
    {
        private readonly IExamRepo _examRepo;

        public InstructorExamService(
            IInstructorRepo instructorRepo,
            ICourseRepo courseRepo,
            ICourseChapterRepo courseChapterRepo,
            IExamRepo examRepo)
            : base(instructorRepo, courseRepo, courseChapterRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<InstructorExamFormVm?> BuildExamCreateModelAsync(int courseId, string userId, int? chapterId = null)
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
            if (instructor == null)
            {
                return null;
            }

            var exam = await _examRepo.GetOneAsync(
                e => e.Id == examId,
                e => e.Course,
                e => e.Chapter!,
                e => e.Questions);

            if (exam == null || exam.Course.InstructorId != instructor.Id)
            {
                return null;
            }

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
            {
                return Fail<int>("لم يتم العثور على بيانات المدرب.");
            }

            var course = await GetOwnedCourseAsync(model.CourseId, instructor);
            if (course == null)
            {
                return Fail<int>("الكورس غير موجود أو لا يتبع هذا المدرب.");
            }

            if (!model.ChapterId.HasValue)
            {
                return Fail<int>("اختر الشابتر الذي سيظهر بعده الامتحان.");
            }

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
            {
                return Fail<int>(chapterValidation.Message);
            }

            if (await _examRepo.CountAsync(e => e.ChapterId == model.ChapterId.Value) > 0)
            {
                return Fail<int>("هذا الشابتر عليه امتحان بالفعل. عدل الامتحان الموجود أو احذفه أولاً.");
            }

            var questions = NormalizeQuestions(model.Questions);
            var validation = ValidateExam(model, questions);
            if (!validation.IsSuccess)
            {
                return Fail<int>(validation.Message);
            }

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

            if (!await _examRepo.CreateAsync(exam))
            {
                return Fail<int>("تعذر إضافة الامتحان حالياً.");
            }

            return Ok(exam.Id, "تم إضافة الامتحان للشابتر بنجاح.");
        }

        public async Task<ServiceResult<bool>> UpdateExamAsync(InstructorExamFormVm model, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail(false, "لم يتم العثور على بيانات المدرب.");
            }

            var exam = await _examRepo.GetOneAsync(
                e => e.Id == model.Id && e.CourseId == model.CourseId,
                e => e.Course,
                e => e.Questions);

            if (exam == null || exam.Course.InstructorId != instructor.Id)
            {
                return Fail(false, "الامتحان غير موجود أو لا يتبع هذا المدرب.");
            }

            if (!model.ChapterId.HasValue)
            {
                return Fail(false, "اختر الشابتر الذي سيظهر بعده الامتحان.");
            }

            var chapterValidation = await ValidateChapterForCourseAsync(model.CourseId, model.ChapterId, instructor);
            if (!chapterValidation.IsSuccess)
            {
                return Fail(false, chapterValidation.Message);
            }

            if (await _examRepo.CountAsync(e => e.Id != model.Id && e.ChapterId == model.ChapterId.Value) > 0)
            {
                return Fail(false, "هذا الشابتر عليه امتحان بالفعل.");
            }

            var questions = NormalizeQuestions(model.Questions);
            var validation = ValidateExam(model, questions);
            if (!validation.IsSuccess)
            {
                return Fail(false, validation.Message);
            }

            var updatedExam = new Exam
            {
                Id = exam.Id,
                CourseId = exam.CourseId,
                ChapterId = model.ChapterId,
                Title = model.Title.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                DurationMinutes = model.DurationMinutes,
                PassingScore = model.PassingScore,
                IsPublished = model.IsPublished,
                CreatedAt = exam.CreatedAt
            };

            if (!await _examRepo.UpdateExamWithQuestionsAsync(updatedExam, questions))
            {
                return Fail(false, "تعذر تعديل الامتحان حالياً.");
            }

            return Ok(true, "تم تعديل الامتحان بنجاح.");
        }

        public async Task<ServiceResult<bool>> DeleteExamAsync(int examId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail(false, "لم يتم العثور على بيانات المدرب.");
            }

            var exam = await _examRepo.GetOneAsync(
                e => e.Id == examId,
                e => e.Course);

            if (exam == null || exam.Course.InstructorId != instructor.Id)
            {
                return Fail(false, "الامتحان غير موجود.");
            }

            if (!await _examRepo.Delete(exam))
            {
                return Fail(false, "تعذر حذف الامتحان حالياً.");
            }

            return Ok(true, "تم حذف الامتحان بنجاح.");
        }

        public async Task<ServiceResult<bool>> ToggleExamPublishAsync(int examId, string userId)
        {
            var instructor = await ResolveInstructorAsync(userId);
            if (instructor == null)
            {
                return Fail(false, "لم يتم العثور على بيانات المدرب.");
            }

            var exam = await _examRepo.GetOneAsync(
                e => e.Id == examId,
                e => e.Course);

            if (exam == null || exam.Course.InstructorId != instructor.Id)
            {
                return Fail(false, "الامتحان غير موجود.");
            }

            var updatedExam = new Exam
            {
                Id = exam.Id,
                CourseId = exam.CourseId,
                ChapterId = exam.ChapterId,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                PassingScore = exam.PassingScore,
                IsPublished = !exam.IsPublished,
                CreatedAt = exam.CreatedAt
            };

            if (!await _examRepo.UpdateAsync(updatedExam))
            {
                return Fail(false, "تعذر تحديث حالة الامتحان حالياً.");
            }

            return Ok(
                updatedExam.IsPublished,
                updatedExam.IsPublished ? "تم نشر الامتحان." : "تم تحويل الامتحان إلى مسودة.");
        }
    }
}
