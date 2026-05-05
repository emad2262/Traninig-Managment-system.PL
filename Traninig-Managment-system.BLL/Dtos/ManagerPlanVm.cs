namespace Traninig_Managment_system.BLL.Dtos
{
    public class ManagerPlanVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Plan.PlanType Type { get; set; } = Plan.PlanType.Basic;
        public double Price { get; set; }
        public int DurationInDays { get; set; }
        public int MaxEmployees { get; set; }
        public int MaxCourses { get; set; }
        public bool IsActive { get; set; } = true;
        public int CompanyCount { get; set; }
        public string FeaturesText { get; set; } = string.Empty;
        public List<PlanFeatureVm> Features { get; set; } = new();
    }

    public class PlanFeatureVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsHighlighted { get; set; }
        public int SortOrder { get; set; }
    }
}
