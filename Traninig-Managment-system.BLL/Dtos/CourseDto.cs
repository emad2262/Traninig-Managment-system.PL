using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos
{
    public class CourseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الكورس مطلوب.")]
        [MaxLength(200, ErrorMessage = "عنوان الكورس يجب ألا يزيد عن 200 حرف.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "وصف الكورس مطلوب.")]
        public string Description { get; set; } = string.Empty;

        public string? Logo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "عدد الساعات يجب أن يكون أكبر من صفر.")]
        public int DurationInHours { get; set; }

        public bool IsPublished { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "القسم المختار غير صالح.")]
        public int CategoryId { get; set; }

        public int? InstructorId { get; set; }

        public string? CategoryName { get; set; }
        public string? InstructorName { get; set; }
    }
}
