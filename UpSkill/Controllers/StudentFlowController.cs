using Application.Courses.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UpSkill.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Student")]
    public class StudentFlowController : ControllerBase
    {
        private readonly IStudentFlowService _studentFlowService;

        public StudentFlowController(IStudentFlowService studentFlowService)
        {
            _studentFlowService = studentFlowService;
        }

        private string GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── POST /api/studentflow/enroll/{courseId} ─────────────────────────
        [HttpPost("enroll/{courseId:int}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var studentId = GetCurrentUserId();
            var result = await _studentFlowService.EnrollInCourseAsync(courseId, studentId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── GET /api/studentflow/my-courses ──────────────────────────────────
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = GetCurrentUserId();
            var result = await _studentFlowService.GetMyCoursesAsync(studentId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── GET /api/studentflow/course/{courseId}/progress ──────────────────
        [HttpGet("course/{courseId:int}/progress")]
        public async Task<IActionResult> GetProgress(int courseId)
        {
            var studentId = GetCurrentUserId();
            var result = await _studentFlowService.GetCourseProgressAsync(courseId, studentId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── POST /api/studentflow/lesson/{lessonId}/complete ─────────────────
        [HttpPost("lesson/{lessonId:int}/complete")]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var studentId = GetCurrentUserId();
            var result = await _studentFlowService.CompleteLessonAsync(lessonId, studentId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── GET /api/studentflow/certificate/{enrollmentId} ──────────────────
        [HttpGet("certificate/{enrollmentId:int}")]
        public async Task<IActionResult> GetCertificate(int enrollmentId)
        {
            var studentId = GetCurrentUserId();
            var result = await _studentFlowService.GetCertificateAsync(enrollmentId, studentId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
