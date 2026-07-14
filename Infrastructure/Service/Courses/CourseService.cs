using Application.Common;
using Application.Courses.DTOs.Course;
using Application.Courses.Interfaces;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistance;
using Infrastructure.Persistance.DbContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Service.Courses
{
    /// <summary>
    /// Implements ICourseService using EF Core.
    /// Handles course CRUD, thumbnail upload to wwwroot/thumbnails/, and publish toggling.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public CourseService(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        // ── GET BY INSTRUCTOR (Dashboard) ────────────────────────────────────

        public async Task<Result<IEnumerable<CourseSummaryDto>>> GetByInstructorAsync(string instructorId)
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var dtos = courses.Select(c => MapToSummary(c, c.Category?.Name ?? "", ""));
            return Result<IEnumerable<CourseSummaryDto>>.Success(dtos);
        }

        // ── GET BY ID ────────────────────────────────────────────────────────

        public async Task<Result<CourseResponseDto>> GetByIdAsync(int id, string? instructorId = null)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseSections)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {id} was not found.");

            // If instructorId is provided, verify ownership
            if (instructorId != null && course.InstructorId != instructorId)
                return Result<CourseResponseDto>.Failure("You are not authorized to view this course.");

            // Fetch instructor name from AppUsers
            var instructor = await _context.Users.FindAsync(course.InstructorId);
            var instructorName = instructor is AppUser appUser ? appUser.FullName : "Unknown";

            return Result<CourseResponseDto>.Success(MapToResponse(course, course.Category?.Name ?? "", instructorName));
        }

        // ── CREATE ───────────────────────────────────────────────────────────

        public async Task<Result<CourseResponseDto>> CreateAsync(CreateCourseDto dto, string instructorId)
        {
            // Validate category exists
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category is null)
                return Result<CourseResponseDto>.Failure($"Category with ID {dto.CategoryId} does not exist.");

            // Handle thumbnail upload
            var thumbnailUrl = await SaveThumbnailAsync(dto.ThumbnailFile, null);

            var course = new Course
            {
                InstructorId     = instructorId,
                CategoryId       = dto.CategoryId,
                Title            = dto.Title.Trim(),
                ShortDescription = dto.ShortDescription.Trim(),
                Description      = dto.Description.Trim(),
                Price            = dto.Price,
                Level            = dto.Level,
                Language         = dto.Language.Trim(),
                ThumbnailUrl     = thumbnailUrl,
                Status           = CourseStatus.Draft,
                IsApproved       = false,
                CreatedAt        = DateTime.UtcNow,
                UpdatedAt        = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Fetch instructor name for response
            var instructor = await _context.Users.FindAsync(instructorId);
            var instructorName = instructor is AppUser appUser ? appUser.FullName : "Unknown";

            return Result<CourseResponseDto>.Success(MapToResponse(course, category.Name, instructorName));
        }

        // ── UPDATE ───────────────────────────────────────────────────────────

        public async Task<Result<CourseResponseDto>> UpdateAsync(int id, UpdateCourseDto dto, string instructorId)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {id} was not found.");

            if (course.InstructorId != instructorId)
                return Result<CourseResponseDto>.Failure("You are not authorized to edit this course.");

            // Validate category exists
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category is null)
                return Result<CourseResponseDto>.Failure($"Category with ID {dto.CategoryId} does not exist.");

            // Handle thumbnail: replace if new file provided, else keep existing
            var thumbnailUrl = await SaveThumbnailAsync(dto.ThumbnailFile, course.ThumbnailUrl);

            course.Title            = dto.Title.Trim();
            course.ShortDescription = dto.ShortDescription.Trim();
            course.Description      = dto.Description.Trim();
            course.Price            = dto.Price;
            course.CategoryId       = dto.CategoryId;
            course.Level            = dto.Level;
            course.Language         = dto.Language.Trim();
            course.ThumbnailUrl     = thumbnailUrl;

            // Reset status to Draft and revoke approval if course is modified
            course.Status           = CourseStatus.Draft;
            course.IsApproved       = false;
            
            course.UpdatedAt        = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Fetch instructor name for response
            var instructor = await _context.Users.FindAsync(instructorId);
            var instructorName = instructor is AppUser appUser ? appUser.FullName : "Unknown";

            return Result<CourseResponseDto>.Success(MapToResponse(course, category.Name, instructorName));
        }

        // ── DELETE ───────────────────────────────────────────────────────────

        public async Task<Result> DeleteAsync(int id, string instructorId)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null)
                return Result.Failure($"Course with ID {id} was not found.");

            if (course.InstructorId != instructorId)
                return Result.Failure("You are not authorized to delete this course.");

            if (course.Status == CourseStatus.Published)
                return Result.Failure("Published courses cannot be deleted. Please unpublish the course first.");

            if (course.Enrollments != null && course.Enrollments.Any())
                return Result.Failure("Cannot delete a course that has enrolled students.");

            // Delete thumbnail file from disk if it exists
            DeleteThumbnailFile(course.ThumbnailUrl);

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        // ── SUBMIT FOR REVIEW ────────────────────────────────────────────

        public async Task<Result<CourseResponseDto>> SubmitForReviewAsync(int id, string instructorId)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null)
                return Result<CourseResponseDto>.Failure($"Course with ID {id} was not found.");

            if (course.InstructorId != instructorId)
                return Result<CourseResponseDto>.Failure("You are not authorized to submit this course for review.");

            if (course.Status == CourseStatus.SubmittedForApproval)
                return Result<CourseResponseDto>.Failure("Course is already submitted for admin approval.");

            if (course.Status == CourseStatus.Published)
                return Result<CourseResponseDto>.Failure("Published courses cannot be submitted for review. Contact an admin to archive first.");

            if (course.Status != CourseStatus.Draft && course.Status != CourseStatus.Archived)
                return Result<CourseResponseDto>.Failure("Only draft or archived courses can be submitted for review.");

            var validationError = ValidateForPublishing(course);
            if (validationError != null)
                return Result<CourseResponseDto>.Failure(validationError);

            course.Status = CourseStatus.SubmittedForApproval;
            course.IsApproved = false;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var instructor = await _context.Users.FindAsync(instructorId);
            var instructorName = instructor is AppUser appUser ? appUser.FullName : "Unknown";

            return Result<CourseResponseDto>.Success(MapToResponse(course, course.Category?.Name ?? "", instructorName));
        }

        // ── GET PUBLISHED (Student Catalog) ──────────────────────────────────

        public async Task<Result<IEnumerable<CourseSummaryDto>>> GetPublishedAsync(string? search = null, int? categoryId = null)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Where(c => c.Status == CourseStatus.Published && c.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Title.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId.Value);

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var dtos    = courses.Select(c => MapToSummary(c, c.Category?.Name ?? "", ""));

            return Result<IEnumerable<CourseSummaryDto>>.Success(dtos);
        }

        // ── THUMBNAIL HELPERS ─────────────────────────────────────────────────

        /// <summary>
        /// Saves an uploaded thumbnail to wwwroot/thumbnails/.
        /// Returns the relative URL path, or the existing URL if no new file is provided.
        /// </summary>
        private async Task<string> SaveThumbnailAsync(IFormFile? file, string? existingUrl)
        {
            if (file is null || file.Length == 0)
                return existingUrl ?? string.Empty;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "thumbnails");
            Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Delete old thumbnail if it exists and is being replaced
            DeleteThumbnailFile(existingUrl);

            return $"/thumbnails/{uniqueFileName}";
        }

        /// <summary>Deletes a thumbnail file from disk if it exists.</summary>
        private void DeleteThumbnailFile(string? thumbnailUrl)
        {
            if (string.IsNullOrEmpty(thumbnailUrl)) return;

            var fullPath = Path.Combine(_env.WebRootPath, thumbnailUrl.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        // ── VALIDATION ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a validation error message if the course is not ready to publish,
        /// or null if all required fields are present.
        /// </summary>
        private static string? ValidateForPublishing(Course course)
        {
            if (string.IsNullOrWhiteSpace(course.Title))
                return "Course must have a title before submitting for review.";

            if (string.IsNullOrWhiteSpace(course.ShortDescription))
                return "Course must have a short description before submitting for review.";

            if (string.IsNullOrWhiteSpace(course.Description))
                return "Course must have a description before submitting for review.";

            if (string.IsNullOrWhiteSpace(course.Language))
                return "Course must have a language before submitting for review.";

            if (course.Price < 0)
                return "Course price cannot be negative.";

            if (course.CategoryId <= 0)
                return "Course must be assigned to a category before submitting for review.";

            var sections = course.CourseSections?.OrderBy(s => s.DisplayOrder).ToList() ?? [];
            if (sections.Count == 0)
                return "Course must have at least one section before submitting for review.";

            foreach (var section in sections)
            {
                var lessons = section.Lessons?.ToList() ?? [];
                if (lessons.Count == 0)
                    return $"Section '{section.Title}' must contain at least one lesson before submitting for review.";
            }

            return null;
        }

        // ── MAPPING HELPERS ───────────────────────────────────────────────────

        private static CourseResponseDto MapToResponse(Course c, string categoryName, string instructorName) => new()
        {
            Id               = c.Id,
            Title            = c.Title,
            ShortDescription = c.ShortDescription,
            Description      = c.Description,
            ThumbnailUrl     = c.ThumbnailUrl,
            Price            = c.Price,
            Level            = c.Level,
            Language         = c.Language,
            Status           = c.Status,
            IsApproved       = c.IsApproved,
            CategoryId       = c.CategoryId,
            CategoryName     = categoryName,
            InstructorId     = c.InstructorId,
            InstructorName   = instructorName,
            EnrollmentCount  = c.Enrollments?.Count() ?? 0,
            SectionCount     = c.CourseSections?.Count() ?? 0,
            CreatedAt        = c.CreatedAt,
            UpdatedAt        = c.UpdatedAt
        };

        private static CourseSummaryDto MapToSummary(Course c, string categoryName, string instructorName) => new()
        {
            Id               = c.Id,
            Title            = c.Title,
            ShortDescription = c.ShortDescription,
            ThumbnailUrl     = c.ThumbnailUrl,
            Price            = c.Price,
            Level            = c.Level,
            Language         = c.Language,
            Status           = c.Status,
            IsApproved       = c.IsApproved,
            CategoryName     = categoryName,
            InstructorName   = instructorName,
            EnrollmentCount  = c.Enrollments?.Count() ?? 0,
            CreatedAt        = c.CreatedAt
        };
    }
}
