using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.DAL.Model
{
    public class CourseCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        public ICollection<Courses> Courses { get; set; }= new List<Courses>();

    }
}
