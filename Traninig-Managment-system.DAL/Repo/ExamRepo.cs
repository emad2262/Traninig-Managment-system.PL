using Microsoft.EntityFrameworkCore;
using Traninig_Managment_system.DAL.Model;
using Traninig_Managment_system.DAL.Repo.Irepo;

namespace Traninig_Managment_system.DAL.Repo
{
    public class ExamRepo : Repo<Exam>, IExamRepo
    {
        private readonly ApplicationDbContext _context;

        public ExamRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> UpdateExamWithQuestionsAsync(Exam exam, IEnumerable<ExamQuestion> questions)
        {
            try
            {
                var currentExam = await _context.Exams
                    .Include(e => e.Questions)
                    .FirstOrDefaultAsync(e => e.Id == exam.Id);

                if (currentExam == null)
                {
                    return false;
                }

                currentExam.CourseId = exam.CourseId;
                currentExam.ChapterId = exam.ChapterId;
                currentExam.Title = exam.Title;
                currentExam.Description = exam.Description;
                currentExam.DurationMinutes = exam.DurationMinutes;
                currentExam.PassingScore = exam.PassingScore;
                currentExam.IsPublished = exam.IsPublished;

                if (currentExam.Questions.Any())
                {
                    _context.ExamQuestions.RemoveRange(currentExam.Questions);
                }

                currentExam.Questions = questions
                    .Select(q => new ExamQuestion
                    {
                        Text = q.Text,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectOption = q.CorrectOption,
                        Points = q.Points
                    })
                    .ToList();

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
