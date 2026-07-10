using Domain.Enum;

namespace Application.Courses.DTOs.Course
{
    /// <summary>
    /// Lightweight DTO used in list views (course catalog, instructor dashboard list).
    /// Avoids sending heavy Description field in list queries.
    /// </summary>
    public class CourseSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelDisplay => Level.ToString();
        public string Language { get; set; }
        public CourseStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public bool IsApproved { get; set; }
        public string CategoryName { get; set; }
        public string InstructorName { get; set; }
        public int EnrollmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
