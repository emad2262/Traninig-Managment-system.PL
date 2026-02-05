using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Model
{
    public class Badge
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public string? IconUrl { get; set; }

        [MaxLength(50)]
        public string BadgeType { get; set; } = string.Empty; // Achievement, Participation, Completion

        [MaxLength(20)]
        public string Tier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum

        public int Points { get; set; }

        // Navigation
        public ICollection<EmployeeBadge> EmployeeBadges { get; set; } = new List<EmployeeBadge>();
    }
}
