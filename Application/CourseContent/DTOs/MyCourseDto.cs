using Domain.Enum;
using System;

namespace Application.CourseContent.DTOs
{
    public class MyCourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal ProgressPercent { get; set; }
        public CompletionStatus CompletionStatus { get; set; }
        public string CompletionStatusDisplay => CompletionStatus.ToString();
        public DateTime EnrolledAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
}
