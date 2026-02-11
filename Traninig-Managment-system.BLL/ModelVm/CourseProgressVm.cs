namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CourseProgressVm
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int ProgressPercentage { get; set; }
    }
}