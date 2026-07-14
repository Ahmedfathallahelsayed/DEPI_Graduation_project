using System.Collections.Generic;

namespace Application.Courses.DTOs.Student
{
    public class StudentCourseProgressDto
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public decimal ProgressPercent { get; set; }
        public string CompletionStatus { get; set; }
        public List<int> CompletedLessonIds { get; set; } = new();
    }
}
