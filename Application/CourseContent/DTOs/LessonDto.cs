using Domain.Enum;

namespace Application.CourseContent.DTOs
{
    public class LessonDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public LessonContentType ContentType { get; set; }
        public string? VideoUrl { get; set; }
        public string? TextContent { get; set; }
        public string? AttachmentUrl { get; set; }
        public int DurationInMinutes { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPreview { get; set; }
        public bool IsCompleted { get; set; } // Only true when accessed by student context
    }

    public class CreateLessonDto
    {
        public int SectionId { get; set; }
        public string Title { get; set; }
        public LessonContentType ContentType { get; set; }
        public string? VideoUrl { get; set; }
        public string? TextContent { get; set; }
        public string? AttachmentUrl { get; set; }
        public int DurationInMinutes { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPreview { get; set; }
    }
}
