using Domain.Enum;

namespace Application.CourseContent.DTOs
{
    public class StudentCourseDetailsDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string FullDescription { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public string InstructorSummary { get; set; }
        public string CategoryName { get; set; }
        public int NumberOfSections { get; set; }
        public int NumberOfLessons { get; set; }
        public bool IsEnrolled { get; set; }
        public decimal? ProgressPercent { get; set; }
    }
}
