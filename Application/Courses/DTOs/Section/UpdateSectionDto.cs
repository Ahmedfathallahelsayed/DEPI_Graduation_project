using System.ComponentModel.DataAnnotations;

namespace Application.Courses.DTOs.Section
{
    public class UpdateSectionDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Title { get; set; }

        [Range(1, int.MaxValue)]
        public int DisplayOrder { get; set; }
    }
}
