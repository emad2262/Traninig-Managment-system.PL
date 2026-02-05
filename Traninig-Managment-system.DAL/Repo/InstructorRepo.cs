namespace Traninig_Managment_system.DAL.Repo
{
    public class InstructorRepo : Repo<Instructor>, IInstructorRepo
    {
        public InstructorRepo(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }

}