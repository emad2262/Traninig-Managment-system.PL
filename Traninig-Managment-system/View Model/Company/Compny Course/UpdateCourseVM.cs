using System.ComponentModel.DataAnnotations;

namespace Traninig_Managment_system.View_Model.Company.Compny_Course
{
    public class UpdateCourseVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter a course title.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "The title must be between 3 and 150 characters.")]
        [Display(Name = "Course title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Describe what this course covers.")]
        [StringLength(2000, ErrorMessage = "The description cannot exceed 2000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pick a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Pick a category.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Pick an instructor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Pick an instructor.")]
        public int? InstructorId { get; set; }

        [Range(1, 1000, ErrorMessage = "Duration must be between 1 and 1000 hours.")]
        [Display(Name = "Duration (hours)")]
        public int DurationInHours { get; set; }

        [Required(ErrorMessage = "Set a start date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Starts on")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Set an end date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ends on")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Published")]
        public bool IsPublished { get; set; }

        // اللوجو الحالي — بيتبعت hidden عشان ميضيعش لو مرفعش صورة جديدة
        public string? CurrentLogo { get; set; }

        [Display(Name = "Replace logo")]
        public IFormFile? Logo { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Instructors { get; set; } = new();
    }
}
