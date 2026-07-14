using Domain.Enum;

namespace Application.Admin.DTOs
{
    public class CourseReviewDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelDisplay => Level.ToString();
        public string Language { get; set; } = string.Empty;

        public CourseStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public bool IsApproved { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public string InstructorId { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string? InstructorEmail { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public IReadOnlyList<SectionReviewDto> Sections { get; set; } = Array.Empty<SectionReviewDto>();
    }

    public class SectionReviewDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public IReadOnlyList<LessonReviewDto> Lessons { get; set; } = Array.Empty<LessonReviewDto>();
    }

    public class LessonReviewDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public LessonContentType ContentType { get; set; }
        public string ContentTypeDisplay => ContentType.ToString();
        public string? VideoUrl { get; set; }
        public string? TextContent { get; set; }
        public string? AttachmentUrl { get; set; }
        public int DurationInMinutes { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPreview { get; set; }
    }
}
