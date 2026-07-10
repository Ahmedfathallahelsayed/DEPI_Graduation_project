using Application.Common;
using Application.Courses.DTOs.Course;

namespace Application.Courses.Interfaces
{
    /// <summary>
    /// Defines all Course management operations for instructors.
    /// Implemented in Infrastructure layer by CourseService.
    /// </summary>
    public interface ICourseService
    {
        // ── Instructor Operations ────────────────────────────────────────────

        /// <summary>
        /// Get all courses belonging to a specific instructor (Instructor Dashboard).
        /// </summary>
        Task<Result<IEnumerable<CourseSummaryDto>>> GetByInstructorAsync(string instructorId);

        /// <summary>
        /// Get a single course by ID. Validates ownership if instructorId is provided.
        /// </summary>
        Task<Result<CourseResponseDto>> GetByIdAsync(int id, string? instructorId = null);

        /// <summary>
        /// Create a new course. Thumbnail file is saved to local storage.
        /// Returns the created course data.
        /// </summary>
        Task<Result<CourseResponseDto>> CreateAsync(CreateCourseDto dto, string instructorId);

        /// <summary>
        /// Update course metadata. If ThumbnailFile is provided, old thumbnail is replaced.
        /// Only the owning instructor can update.
        /// </summary>
        Task<Result<CourseResponseDto>> UpdateAsync(int id, UpdateCourseDto dto, string instructorId);

        /// <summary>
        /// Delete a course. Only the owning instructor can delete.
        /// Course must be in Draft status (cannot delete published courses).
        /// </summary>
        Task<Result> DeleteAsync(int id, string instructorId);

        /// <summary>
        /// Toggle course publish status.
        /// Draft → SubmittedForApproval (or Published if auto-approved).
        /// Published → Archived.
        /// Validates that course has title, description, price, and category before publishing.
        /// </summary>
        Task<Result<CourseResponseDto>> TogglePublishAsync(int id, string instructorId);

        // ── Public / Catalog Operations (used by TM4 - Student flow) ────────

        /// <summary>
        /// Get all published and approved courses for the student catalog.
        /// Supports optional search by title and filter by categoryId.
        /// </summary>
        Task<Result<IEnumerable<CourseSummaryDto>>> GetPublishedAsync(string? search = null, int? categoryId = null);
    }
}
