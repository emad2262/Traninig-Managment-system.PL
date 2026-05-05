namespace Traninig_Managment_system.BLL.Dtos
{
    public class ManagerDashboardVm
    {
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalPlans { get; set; }
        public int TotalNotifications { get; set; }
        public int ExpiringSoonCount { get; set; }

        public List<ManagerChartPointVm> CompanyGrowth { get; set; } = new();
        public List<ManagerChartPointVm> EmployeeGrowth { get; set; } = new();
        public List<ManagerCompanyVm> ExpiringCompanies { get; set; } = new();
        public List<ManagerCompanyVm> RecentCompanies { get; set; } = new();
        public List<ManagerPlanVm> Plans { get; set; } = new();
    }

    public class ManagerChartPointVm
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
