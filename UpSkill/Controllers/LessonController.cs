using Application.Courses.DTOs.Lesson;
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
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        private string GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── GET /api/lesson/{id} ───────────────────────────────────────────
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _lessonService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        // ── POST /api/lesson ───────────────────────────────────────────────
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Instructor")]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var result = await _lessonService.CreateAsync(dto, instructorId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(result.Error);
        }

        // ── PUT /api/lesson/{id} ───────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Instructor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var result = await _lessonService.UpdateAsync(id, dto, instructorId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── DELETE /api/lesson/{id} ────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = GetCurrentUserId();
            var result = await _lessonService.DeleteAsync(id, instructorId);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPost("{lessonId:int}/complete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Student")]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var studentId = GetCurrentUserId();

            var result = await _lessonService.CompleteLessonAsync(lessonId, studentId);

            return result.IsSuccess
                ? Ok()
                : BadRequest(result.Error);
        }
    }
}
