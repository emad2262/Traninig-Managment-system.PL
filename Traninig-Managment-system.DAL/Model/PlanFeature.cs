using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Model
{
    public class PlanFeature
    {
        public int PlanId { get; set; }
        public Plan Plan { get; set; }

        public int FeatureId { get; set; }
        public Feature Feature { get; set; }

        public bool IsEnabled { get; set; }
        public string? Value { get; set; } // مثال: عدد الموظفين
    }
}
