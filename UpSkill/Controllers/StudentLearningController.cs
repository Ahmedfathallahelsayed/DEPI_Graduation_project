using Application.CourseContent.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UpSkillAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentLearningController : ControllerBase
    {
        private readonly IStudentLearningService _studentLearningService;

        public StudentLearningController(IStudentLearningService studentLearningService)
        {
            _studentLearningService = studentLearningService;
        }

        [HttpGet("course/{courseId}/hierarchy")]
        // [Authorize] // Uncomment when Auth is fully integrated
        public async Task<IActionResult> GetCourseHierarchy(int courseId)
        {
            // For testing, if user is not authenticated, pass null or a test string
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-student-id";

            var hierarchy = await _studentLearningService.GetCourseHierarchyAsync(courseId, userId);
            return Ok(hierarchy);
        }

        [HttpPost("lessons/{lessonId}/complete")]
        // [Authorize(Roles = "Student")] // Uncomment when Auth is fully integrated
        public async Task<IActionResult> MarkLessonComplete(int lessonId)
        {
            // For testing, if user is not authenticated, pass null or a test string
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-student-id";

            var result = await _studentLearningService.MarkLessonCompleteAsync(lessonId, userId);
            if (!result) return BadRequest("Unable to mark lesson complete. Please ensure you are enrolled.");

            return Ok("Lesson marked as complete.");
        }
        [HttpGet("courses")]
        [AllowAnonymous] // Anyone can view the catalog, but IsEnrolled needs user info
        public async Task<IActionResult> GetStudentCatalog([FromQuery] string? search = null, [FromQuery] int? categoryId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            
            var catalog = await _studentLearningService.GetStudentCatalogAsync(userId, search, categoryId);
            return Ok(catalog);
        }

        [HttpGet("courses/{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var details = await _studentLearningService.GetStudentCourseDetailsAsync(courseId, userId);
            
            if (details == null) return NotFound("Course not found or not published.");
            
            return Ok(details);
        }

        [HttpPost("courses/{courseId}/enroll")]
        // [Authorize(Roles = "Student")]
        public async Task<IActionResult> EnrollStudent(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-student-id";

            var result = await _studentLearningService.EnrollStudentAsync(courseId, userId);
            
            if (!result.IsSuccess) return BadRequest(result.Error);
            
            return Ok("Enrolled successfully.");
        }

        [HttpGet("my-courses")]
        // [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "test-student-id";

            var myCourses = await _studentLearningService.GetMyCoursesAsync(userId);
            return Ok(myCourses);
        }
    }
}
