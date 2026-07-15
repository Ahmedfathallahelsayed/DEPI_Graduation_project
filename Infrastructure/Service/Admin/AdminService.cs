using Application.Admin.DTOs;
using Application.Admin.Interfaces;
using Application.Common;
using Application.Courses.DTOs.Course;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistance;
using Infrastructure.Persistance.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service.Admin
{
    public class AdminService : IAdminService
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminService(AppDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();

            var roleIds = await _context.Roles
                .AsNoTracking()
                .Where(r => r.Name == "Student" || r.Name == "Instructor" || r.Name == "Admin")
                .Select(r => new { r.Name, r.Id })
                .ToListAsync();

            var studentRoleId = roleIds.FirstOrDefault(r => r.Name == "Student")?.Id;
            var instructorRoleId = roleIds.FirstOrDefault(r => r.Name == "Instructor")?.Id;
            var adminRoleId = roleIds.FirstOrDefault(r => r.Name == "Admin")?.Id;

            var totalStudents = studentRoleId is null
                ? 0
                : await _context.UserRoles.CountAsync(ur => ur.RoleId == studentRoleId);

            var totalInstructors = instructorRoleId is null
                ? 0
                : await _context.UserRoles.CountAsync(ur => ur.RoleId == instructorRoleId);

            var totalAdmins = adminRoleId is null
                ? 0
                : await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRoleId);

            var totalCourses = await _context.Courses.CountAsync();
            
            var pendingCoursesCount = await _context.Courses
                .CountAsync(c => c.Status == CourseStatus.SubmittedForApproval && !c.IsApproved);

            var publishedCoursesCount = await _context.Courses
                .CountAsync(c => c.Status == CourseStatus.Published);

            var totalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            return Result<DashboardStatsDto>.Success(new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalStudents = totalStudents,
                TotalInstructors = totalInstructors,
                TotalAdmins = totalAdmins,
                TotalCourses = totalCourses,
                PendingCoursesCount = pendingCoursesCount,
                PublishedCoursesCount = publishedCoursesCount,
                TotalRevenue = totalRevenue
            });
        }

        public async Task<Result<IEnumerable<CourseSummaryDto>>> GetPendingCoursesAsync()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Where(c => c.Status == CourseStatus.SubmittedForApproval && !c.IsApproved)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var instructorIds = courses.Select(c => c.InstructorId).Distinct().ToList();
            var instructors = await _context.Users
                .AsNoTracking()
                .Where(u => instructorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var dtos = courses.Select(c => MapToSummary(
                c,
                c.Category?.Name ?? string.Empty,
                instructors.GetValueOrDefault(c.InstructorId) ?? "Unknown"));

            return Result<IEnumerable<CourseSummaryDto>>.Success(dtos);
        }

        public async Task<Result<CourseReviewDto>> GetPendingCourseDetailsAsync(int courseId)
        {
            var course = await LoadCourseWithContentAsync(courseId, track: false);
            if (course is null)
                return Result<CourseReviewDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.SubmittedForApproval)
                return Result<CourseReviewDto>.Failure("Course is not in the admin review queue.");

            var instructor = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == course.InstructorId);
            return Result<CourseReviewDto>.Success(MapToReview(course, instructor));
        }

        public async Task<Result<CourseResponseDto>> ApproveCourseAsync(int courseId)
        {
            var course = await LoadCourseWithContentAsync(courseId, track: true);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.SubmittedForApproval)
                return Result<CourseResponseDto>.Failure("Only courses submitted for approval can be approved.");

            var validationError = ValidateForPublish(course);
            if (validationError is not null)
                return Result<CourseResponseDto>.Failure(validationError);

            course.IsApproved = true;
            course.Status = CourseStatus.Published; // Directly publish on approval
            course.RejectionReason = null;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        public async Task<Result<CourseResponseDto>> RejectCourseAsync(int courseId, RejectCourseDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return Result<CourseResponseDto>.Failure("Rejection reason is required.");

            var reason = dto.Reason.Trim();
            if (reason.Length < 5)
                return Result<CourseResponseDto>.Failure("Rejection reason must be at least 5 characters.");

            if (reason.Length > 500)
                return Result<CourseResponseDto>.Failure("Rejection reason cannot exceed 500 characters.");

            var course = await LoadCourseWithContentAsync(courseId, track: true);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.SubmittedForApproval)
                return Result<CourseResponseDto>.Failure("Only courses submitted for approval can be rejected.");

            course.IsApproved = false;
            course.Status = CourseStatus.Draft;
            course.RejectionReason = reason;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        public async Task<Result<CourseResponseDto>> ArchiveCourseAsync(int courseId)
        {
            var course = await LoadCourseWithContentAsync(courseId, track: true);
            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.Published)
                return Result<CourseResponseDto>.Failure("Only published courses can be archived.");

            course.Status = CourseStatus.Archived;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<CourseResponseDto>.Success(await MapToResponseAsync(course));
        }

        // ── User Management Implementation ──────────────────────────────
        public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userRoles = await _context.UserRoles
                .AsNoTracking()
                .ToListAsync();

            var roles = await _context.Roles
                .AsNoTracking()
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            var userRolesGrouped = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ur => roles.GetValueOrDefault(ur.RoleId) ?? "Unknown").FirstOrDefault() ?? "None"
                );

            var dtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Role = userRolesGrouped.GetValueOrDefault(u.Id) ?? "None",
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive,
                PhoneNumber = u.PhoneNumber ?? string.Empty
            });

            return Result<IEnumerable<UserDto>>.Success(dtos);
        }

        public async Task<Result> BlockUserAsync(string userId, string currentUserId)
        {
            if (userId == currentUserId)
            {
                return Result.Failure("You cannot block your own admin account.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            var isUserAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isUserAdmin)
            {
                var adminRoleId = (await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin"))?.Id;
                if (adminRoleId != null)
                {
                    var activeAdminsCount = await (from ur in _context.UserRoles
                                                   join u in _context.Users on ur.UserId equals u.Id
                                                   where ur.RoleId == adminRoleId && u.IsActive
                                                   select u).CountAsync();

                    if (activeAdminsCount <= 1 && user.IsActive)
                    {
                        return Result.Failure("You cannot block the last active administrator.");
                    }
                }
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Result.Failure(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        public async Task<Result> UnblockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Result.Failure(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        public async Task<Result> EditUserAsync(string userId, EditUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
            }

            return Result.Success();
        }

        private async Task<Course?> LoadCourseWithContentAsync(int courseId, bool track)
        {
            IQueryable<Course> query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Lessons);

            if (!track)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(c => c.Id == courseId);
        }

        private static string? ValidateForPublish(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.Title))
                return "Course must have a title before publishing.";

            if (string.IsNullOrWhiteSpace(course.ShortDescription))
                return "Course must have a short description before publishing.";

            if (string.IsNullOrWhiteSpace(course.Description))
                return "Course must have a description before publishing.";

            if (course.CategoryId <= 0 || course.Category is null)
                return "Course must be assigned to a valid category before publishing.";

            if (string.IsNullOrWhiteSpace(course.Language))
                return "Course must have a language before publishing.";

            if (course.Price < 0)
                return "Course price cannot be negative.";

            var sections = course.CourseSections?.OrderBy(s => s.DisplayOrder).ToList() ?? [];
            if (sections.Count == 0)
                return "Course must have at least one section before publishing.";

            foreach (var section in sections)
            {
                var lessons = section.Lessons?.ToList() ?? [];
                if (lessons.Count == 0)
                    return $"Section '{section.Title}' must contain at least one lesson before publishing.";
            }

            return null;
        }

        private async Task<CourseResponseDto> MapToResponseAsync(Course course)
        {
            var instructor = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == course.InstructorId);
            var instructorName = instructor?.FullName ?? "Unknown";
            return MapToResponse(course, course.Category?.Name ?? string.Empty, instructorName);
        }

        private static CourseReviewDto MapToReview(Course course, AppUser? instructor)
        {
            var sections = (course.CourseSections ?? Enumerable.Empty<CourseSection>())
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new SectionReviewDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    DisplayOrder = s.DisplayOrder,
                    Lessons = (s.Lessons ?? Enumerable.Empty<Lesson>())
                        .OrderBy(l => l.DisplayOrder)
                        .Select(l => new LessonReviewDto
                        {
                            Id = l.Id,
                            Title = l.Title,
                            ContentType = l.ContentType,
                            VideoUrl = l.VideoUrl,
                            TextContent = l.TextContent,
                            AttachmentUrl = l.AttachmentUrl,
                            DurationInMinutes = l.DurationInMinutes,
                            DisplayOrder = l.DisplayOrder,
                            IsPreview = l.IsPreview
                        })
                        .ToList()
                })
                .ToList();

            return new CourseReviewDto
            {
                CourseId = course.Id,
                Title = course.Title,
                ShortDescription = course.ShortDescription,
                Description = course.Description,
                ThumbnailUrl = course.ThumbnailUrl,
                Price = course.Price,
                Level = course.Level,
                Language = course.Language,
                Status = course.Status,
                IsApproved = course.IsApproved,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.Name ?? string.Empty,
                InstructorId = course.InstructorId,
                InstructorName = instructor?.FullName ?? "Unknown",
                InstructorEmail = instructor?.Email,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                Sections = sections
            };
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
            UpdatedAt = c.UpdatedAt,
            RejectionReason = c.RejectionReason
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
            CreatedAt = c.CreatedAt,
            RejectionReason = c.RejectionReason
        };
    }
}
