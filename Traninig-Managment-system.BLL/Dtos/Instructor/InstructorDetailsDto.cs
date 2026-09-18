using System;
using System.Collections.Generic;
using System.Text;

namespace Traninig_Managment_system.BLL.Dtos.Instructor
{
    public class InstructorDetailsDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }

        public int TotalCourses { get; set; }
        public int RunningCourses { get; set; }
        public int TotalStudents { get; set; }

        public IReadOnlyList<InstructorCourseDto> Courses { get; set; } = Array.Empty<InstructorCourseDto>();
    }
}
public class InstructorCourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Image { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int EnrolledCount { get; set; }
}
