namespace Traninig_Managment_system.View_Model.Company.Compny_Course
{
    public class CourseListVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public int DurationInHours { get; set; }
        public int LessonCount { get; set; }
        public bool IsPublished { get; set; }
    }
}
