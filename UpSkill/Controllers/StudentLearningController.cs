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
    }
}
