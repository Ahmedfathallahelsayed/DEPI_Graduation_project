using Application.Common;
using Application.Courses.DTOs.Student;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Courses.Interfaces
{
    public interface IStudentFlowService
    {
        Task<Result<EnrollmentResponseDto>> EnrollInCourseAsync(int courseId, string studentId);
        Task<Result<IEnumerable<EnrollmentResponseDto>>> GetMyCoursesAsync(string studentId);
        Task<Result<StudentCourseProgressDto>> GetCourseProgressAsync(int courseId, string studentId);
        Task<Result<StudentCourseProgressDto>> CompleteLessonAsync(int lessonId, string studentId);
        Task<Result<CertificateResponseDto>> GetCertificateAsync(int enrollmentId, string studentId);
    }
}
