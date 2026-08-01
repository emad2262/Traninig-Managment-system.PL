namespace Traninig_Managment_system.View_Model.Company.Compny_Course
{
    public class CourseDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public int DurationInHours { get; set; }
        public bool IsPublished { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public List<CourseLessonVM> Lessons { get; set; } = new();
    }
}
