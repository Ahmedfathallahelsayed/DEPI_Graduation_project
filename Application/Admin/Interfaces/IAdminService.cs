using Application.Admin.DTOs;
using Application.Common;
using Application.Courses.DTOs.Course;

namespace Application.Admin.Interfaces
{
    public interface IAdminService
    {
        Task<Result<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<Result<IEnumerable<CourseSummaryDto>>> GetPendingCoursesAsync();
        Task<Result<CourseReviewDto>> GetPendingCourseDetailsAsync(int courseId);
        Task<Result<CourseResponseDto>> ApproveCourseAsync(int courseId);
        Task<Result<CourseResponseDto>> RejectCourseAsync(int courseId, RejectCourseDto dto);
        Task<Result<CourseResponseDto>> PublishCourseAsync(int courseId);
        Task<Result<CourseResponseDto>> ArchiveCourseAsync(int courseId);
    }
}
