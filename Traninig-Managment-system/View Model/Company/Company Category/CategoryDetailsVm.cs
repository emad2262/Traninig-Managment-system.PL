namespace Traninig_Managment_system.View_Model.Company.Company_Category
{
    public class CategoryDetailsVm
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalCourse { get; set; }
        public List<CategoryCourseVM> Courses { get; set; } = new();
    }

    public class CategoryCourseVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public int DurationInHours { get; set; }
        public bool IsPublished { get; set; }
    }
}
