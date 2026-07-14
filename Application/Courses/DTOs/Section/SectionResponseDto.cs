using Application.Courses.DTOs.Lesson;
using System;
using System.Collections.Generic;

namespace Application.Courses.DTOs.Section
{
    public class SectionResponseDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LessonResponseDto> Lessons { get; set; } = new();
    }
}
