namespace Traninig_Managment_system.BLL.Dtos
{
    public class CategoryAndCoursesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int DraftCourses { get; set; }
        public IEnumerable<CourseDto> Courses { get; set; } = new List<CourseDto>();
    }
}
