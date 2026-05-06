using Traninig_Managment_system.DAL.Model;

namespace Traninig_Managment_system.DAL.Repo.Irepo
{
    public interface IExamRepo : IRepo<Exam>
    {
        Task<bool> UpdateExamWithQuestionsAsync(Exam exam, IEnumerable<ExamQuestion> questions);
    }
}
