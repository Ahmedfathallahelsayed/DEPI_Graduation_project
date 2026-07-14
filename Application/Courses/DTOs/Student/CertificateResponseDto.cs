using System;

namespace Application.Courses.DTOs.Student
{
    public class CertificateResponseDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public string CourseTitle { get; set; }
        public string CertificateCode { get; set; }
        public DateTime IssuedAt { get; set; }
        public string CertificateUrl { get; set; }
    }
}
