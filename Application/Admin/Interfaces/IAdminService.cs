using Application.Admin.DTOs;
using Application.Common;
using Application.Courses.DTOs.Course;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Admin.Interfaces
{
    public interface IAdminService
    {
        Task<Result<DashboardStatsDto>> GetDashboardStatsAsync();
        Task<Result<IEnumerable<CourseSummaryDto>>> GetPendingCoursesAsync();
        Task<Result<CourseReviewDto>> GetPendingCourseDetailsAsync(int courseId);
        Task<Result<CourseResponseDto>> ApproveCourseAsync(int courseId);
        Task<Result<CourseResponseDto>> RejectCourseAsync(int courseId, RejectCourseDto dto);
        Task<Result<CourseResponseDto>> ArchiveCourseAsync(int courseId);
        
        // User Management
        Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<Result> BlockUserAsync(string userId, string currentUserId);
        Task<Result> UnblockUserAsync(string userId);
        Task<Result> EditUserAsync(string userId, EditUserDto dto);
    }
}
