using Domain.Enum;

namespace Application.Courses.DTOs.Course
{
    /// <summary>
    /// Returned to the client when reading course data (GET operations).
    /// Includes category name and instructor info for display purposes.
    /// </summary>
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelDisplay => Level.ToString();
        public string Language { get; set; }
        public CourseStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public bool IsApproved { get; set; }

        // Category info
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        // Instructor info
        public string InstructorId { get; set; }
        public string InstructorName { get; set; }

        // Stats (used in Instructor Dashboard)
        public int EnrollmentCount { get; set; }
        public int SectionCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
