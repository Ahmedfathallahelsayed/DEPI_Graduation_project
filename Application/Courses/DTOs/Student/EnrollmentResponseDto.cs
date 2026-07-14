using Domain.Enum;
using System;

namespace Application.Courses.DTOs.Student
{
    public class EnrollmentResponseDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public decimal ProgressPercent { get; set; }
        public CompletionStatus CompletionStatus { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
}
