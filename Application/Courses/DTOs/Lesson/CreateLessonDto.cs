using Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Application.Courses.DTOs.Lesson
{
    public class CreateLessonDto
    {
        [Required]
        public int SectionId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; }

        [Required]
        public LessonContentType ContentType { get; set; }

        public string? VideoUrl { get; set; }
        public string? TextContent { get; set; }
        public string? AttachmentUrl { get; set; }

        [Range(0, 1000)]
        public int DurationInMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int DisplayOrder { get; set; }

        public bool IsPreview { get; set; }
    }
}
