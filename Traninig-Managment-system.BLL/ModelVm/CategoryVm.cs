
namespace Traninig_Managment_system.BLL.ModelVm
{
    public class CategoryVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<CourseVm> Courses { get; set; }=new List<CourseVm>();
    }
}
