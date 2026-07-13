using System.ComponentModel.DataAnnotations;

namespace Application.Admin.DTOs
{
    public class RejectCourseDto
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [MinLength(5, ErrorMessage = "Rejection reason must be at least 5 characters.")]
        [MaxLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
