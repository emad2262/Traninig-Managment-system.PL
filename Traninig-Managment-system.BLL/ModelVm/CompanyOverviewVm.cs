

namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CompanyOverviewVm
    {
        // Timer
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int DaysRemaining { get; set; }

        // Dashboard
        public int TotalCourses { get; set; }
        public int ActiveInstructors { get; set; }
        public int TotalStudents { get; set; }
        public double CompletionRate { get; set; }

        // UI only
        public string PlanName { get; set; } = "Standard";

    }
}