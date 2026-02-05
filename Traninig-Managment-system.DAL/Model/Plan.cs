namespace Traninig_Managment_system.DAL.Model
{
    public class Plan
    {
        public enum PlanType
        {
            Basic = 1,
            Pro = 2,
            Premium = 3
        }
        [Key]
        public int Id { get; set; }
        

        [Required]
        public string Name { get; set; }
        public PlanType Type { get; set; }
        public double Price { get; set; }

        public int DurationInDays { get; set; }
        public int MaxEmployees { get; set; }
        public int MaxCourses { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Company> Companys { get; set; }=new List<Company>();

    }
}
