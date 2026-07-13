using System.ComponentModel.DataAnnotations;

namespace Application.Admin.DTOs
{
    public class RejectCourseDto
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
