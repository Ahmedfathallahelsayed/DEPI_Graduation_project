using Application.Courses.DTOs.Section;
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
    public class CourseSectionController : ControllerBase
    {
        private readonly ICourseSectionService _sectionService;

        public CourseSectionController(ICourseSectionService sectionService)
        {
            _sectionService = sectionService;
        }

        private string GetCurrentUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ── GET /api/coursesection/course/{courseId} ─────────────────────────
        [HttpGet("course/{courseId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCourseId(int courseId)
        {
            var result = await _sectionService.GetSectionsByCourseIdAsync(courseId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── GET /api/coursesection/{id} ──────────────────────────────────────
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sectionService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        // ── POST /api/coursesection ──────────────────────────────────────────
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Instructor")]
        public async Task<IActionResult> Create([FromBody] CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var result = await _sectionService.CreateAsync(dto, instructorId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(result.Error);
        }

        // ── PUT /api/coursesection/{id} ──────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Instructor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSectionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var result = await _sectionService.UpdateAsync(id, dto, instructorId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        // ── DELETE /api/coursesection/{id} ───────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = GetCurrentUserId();
            var result = await _sectionService.DeleteAsync(id, instructorId);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
    }
}
