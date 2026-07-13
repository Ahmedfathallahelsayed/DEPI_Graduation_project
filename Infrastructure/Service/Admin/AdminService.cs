using Application.Admin.DTOs;
using Application.Admin.Interfaces;
using Application.Common;
using Application.Courses.DTOs.Course;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistance;
using Infrastructure.Persistance.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Service.Admin
{
    public class AdminService : IAdminService
    {
        private readonly AppDBContext _context;

        public AdminService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Result<IEnumerable<CourseSummaryDto>>> GetPendingCoursesAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Where(c => c.Status == CourseStatus.SubmittedForApproval && !c.IsApproved)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var instructorIds = courses.Select(c => c.InstructorId).Distinct().ToList();
            var instructors = await _context.Users
                .Where(u => instructorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var dtos = courses.Select(c => MapToSummary(
                c,
                c.Category?.Name ?? string.Empty,
                instructors.GetValueOrDefault(c.InstructorId) ?? "Unknown"));

            return Result<IEnumerable<CourseSummaryDto>>.Success(dtos);
        }

        public async Task<Result<CourseResponseDto>> ApproveCourseAsync(int courseId)
        {
            var course = await LoadCourseAsync(courseId);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.SubmittedForApproval)
                return Result<CourseResponseDto>.Failure("Only courses submitted for approval can be approved.");

            if (course.IsApproved)
                return Result<CourseResponseDto>.Failure("Course is already approved. Use the publish endpoint to make it live.");

            // Approve only — publishing is a separate Admin action.
            course.IsApproved = true;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        public async Task<Result<CourseResponseDto>> RejectCourseAsync(int courseId, RejectCourseDto? dto = null)
        {
            var course = await LoadCourseAsync(courseId);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.SubmittedForApproval)
                return Result<CourseResponseDto>.Failure("Only courses submitted for approval can be rejected.");

            // No Rejected enum — return to Draft so instructor can revise and resubmit.
            course.IsApproved = false;
            course.Status = CourseStatus.Draft;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        public async Task<Result<CourseResponseDto>> PublishCourseAsync(int courseId)
        {
            var course = await LoadCourseAsync(courseId);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status == CourseStatus.Published && course.IsApproved)
                return Result<CourseResponseDto>.Failure("Course is already published.");

            if (!course.IsApproved)
                return Result<CourseResponseDto>.Failure("Only approved courses can be published. Approve the course first.");

            if (string.IsNullOrWhiteSpace(course.Title) || string.IsNullOrWhiteSpace(course.Description))
                return Result<CourseResponseDto>.Failure("Course must have a title and description before publishing.");

            if (course.CategoryId <= 0)
                return Result<CourseResponseDto>.Failure("Course must be assigned to a category before publishing.");

            course.Status = CourseStatus.Published;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        public async Task<Result<CourseResponseDto>> ArchiveCourseAsync(int courseId)
        {
            var course = await LoadCourseAsync(courseId);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.Published)
                return Result<CourseResponseDto>.Failure("Only published courses can be archived.");

            course.Status = CourseStatus.Archived;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        private async Task<Course?> LoadCourseAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseSections)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        private async Task<CourseResponseDto> MapToResponseAsync(Course course)
        {
            var instructor = await _context.Users.FindAsync(course.InstructorId);
            var instructorName = instructor is AppUser appUser ? appUser.FullName : "Unknown";
            return MapToResponse(course, course.Category?.Name ?? string.Empty, instructorName);
        }

        private static CourseResponseDto MapToResponse(Course c, string categoryName, string instructorName) => new()
        {
            Id = c.Id,
            Title = c.Title,
            ShortDescription = c.ShortDescription,
            Description = c.Description,
            ThumbnailUrl = c.ThumbnailUrl,
            Price = c.Price,
            Level = c.Level,
            Language = c.Language,
            Status = c.Status,
            IsApproved = c.IsApproved,
            CategoryId = c.CategoryId,
            CategoryName = categoryName,
            InstructorId = c.InstructorId,
            InstructorName = instructorName,
            EnrollmentCount = c.Enrollments?.Count() ?? 0,
            SectionCount = c.CourseSections?.Count() ?? 0,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };

        private static CourseSummaryDto MapToSummary(Course c, string categoryName, string instructorName) => new()
        {
            Id = c.Id,
            Title = c.Title,
            ShortDescription = c.ShortDescription,
            ThumbnailUrl = c.ThumbnailUrl,
            Price = c.Price,
            Level = c.Level,
            Language = c.Language,
            Status = c.Status,
            IsApproved = c.IsApproved,
            CategoryName = categoryName,
            InstructorName = instructorName,
            EnrollmentCount = c.Enrollments?.Count() ?? 0,
            CreatedAt = c.CreatedAt
        };
    }
}
