using Application.Courses.DTOs.Course;
using Application.Courses.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UpSkill.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // ── Helper: Get logged-in instructor ID from JWT ──────────────────
        private string GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ═════════════════════════════════════════════════════════════════
        // PUBLIC ENDPOINTS (Student Catalog — used by TM4)
        // ═════════════════════════════════════════════════════════════════

        // ── GET /api/course ───────────────────────────────────────────────
        /// <summary>
        /// Get all published and approved courses.
        /// Supports optional ?search=keyword and ?categoryId=1 query params.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublished(
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null)
        {
            var result = await _courseService.GetPublishedAsync(search, categoryId);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        // ── GET /api/course/{id} ──────────────────────────────────────────
        /// <summary>Get a single course by ID (public — no ownership check).</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _courseService.GetByIdAsync(id);
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }

        // ═════════════════════════════════════════════════════════════════
        // INSTRUCTOR DASHBOARD ENDPOINTS
        // ═════════════════════════════════════════════════════════════════

        // ── GET /api/course/my-courses ────────────────────────────────────
        /// <summary>Get all courses belonging to the logged-in instructor.</summary>
        [HttpGet("my-courses")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCourses()
        {
            var instructorId = GetCurrentUserId();
            var result = await _courseService.GetByInstructorAsync(instructorId);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        // ── GET /api/course/my-courses/{id} ──────────────────────────────
        /// <summary>
        /// Get a specific course by ID with ownership validation.
        /// Only the owning instructor can access.
        /// </summary>
        [HttpGet("my-courses/{id:int}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyCoursesById(int id)
        {
            var instructorId = GetCurrentUserId();
            var result = await _courseService.GetByIdAsync(id, instructorId);
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }

        // ── POST /api/course ──────────────────────────────────────────────
        /// <summary>
        /// Create a new course. Instructor only.
        /// Send as multipart/form-data to include a thumbnail image.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create([FromForm] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var result = await _courseService.CreateAsync(dto, instructorId);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(result.Error);
        }

        // ── PUT /api/course/{id} ──────────────────────────────────────────
        /// <summary>
        /// Update course metadata. Instructor only.
        /// Only the owning instructor can update.
        /// Send as multipart/form-data to replace thumbnail.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var result = await _courseService.UpdateAsync(id, dto, instructorId);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        // ── DELETE /api/course/{id} ───────────────────────────────────────
        /// <summary>
        /// Delete a course. Instructor only.
        /// Cannot delete published courses or courses with enrolled students.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = GetCurrentUserId();
            var result = await _courseService.DeleteAsync(id, instructorId);

            return result.IsSuccess
                ? NoContent()
                : BadRequest(result.Error);
        }

        // ── POST /api/course/{id}/submit-for-review ───────────────────────
        /// <summary>
        /// Submit a draft/archived course for admin review. Instructor only.
        /// Does not publish — Admin publishes via POST /api/Admin/courses/{id}/publish.
        /// </summary>
        [HttpPost("{id:int}/submit-for-review")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var instructorId = GetCurrentUserId();
            var result = await _courseService.SubmitForReviewAsync(id, instructorId);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }
    }
}
