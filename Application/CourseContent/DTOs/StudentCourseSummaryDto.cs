using Domain.Enum;

namespace Application.CourseContent.DTOs
{
    public class StudentCourseSummaryDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string InstructorName { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelDisplay => Level.ToString();
        public int TotalLessons { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
