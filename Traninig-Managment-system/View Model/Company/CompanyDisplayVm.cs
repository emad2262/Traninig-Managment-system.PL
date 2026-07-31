namespace Traninig_Managment_system.View_Model.Company
{
    public class CompanyDisplayVm
    {
        public string CompanyName { get; set; } = string.Empty;

        // Workspace counts
        public int EmployeeCount { get; set; }
        public int InstructorCount { get; set; }
        public int CategoryCount { get; set; }
        public int CourseCount { get; set; }
        public int PublishedCourseCount { get; set; }

        // EmployeeCourse (assignments) — drives the completion ribbon
        //public int TotalAssignments { get; set; }
        //public int CompletedAssignments { get; set; }
        //public int InProgressAssignments { get; set; }
        //public int NotStartedAssignments { get; set; }

        //public int CertificatesIssued { get; set; }

        // Subscription
        public string PlanName { get; set; } = string.Empty;
        public DateTime SubscriptionEndDate { get; set; }

    }
}
