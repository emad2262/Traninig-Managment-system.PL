namespace Traninig_Managment_system.View_Model.Company.Compny_Course
{
    public class CourseIndexVM
    {
        public List<CourseListVM> Courses { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public int? SelectedCategoryId { get; set; }

        public int PublishedCount => Courses.Count(c => c.IsPublished);
    }
}
