using System.Collections.Generic;

namespace Application.CourseContent.DTOs
{
    public class SectionDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
        public List<LessonDto> Lessons { get; set; } = new List<LessonDto>();
    }

    public class CreateSectionDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
    }
}
