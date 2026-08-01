namespace Traninig_Managment_system.View_Model.Company.Compny_Course
{
    public class CourseLessonVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? VideoUrl { get; set; }
        public string? PdfUrl { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
