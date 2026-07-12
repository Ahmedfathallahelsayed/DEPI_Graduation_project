using Application.CourseContent.DTOs;
using Application.CourseContent.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UpSkillAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Instructor")] // Commended out for easy testing if auth isn't fully ready
    public class CourseContentController : ControllerBase
    {
        private readonly ICourseContentService _courseContentService;

        public CourseContentController(ICourseContentService courseContentService)
        {
            _courseContentService = courseContentService;
        }

        [HttpGet("course/{courseId}/sections")]
        public async Task<IActionResult> GetCourseSections(int courseId)
        {
            var sections = await _courseContentService.GetCourseSectionsAsync(courseId);
            return Ok(sections);
        }

        [HttpPost("sections")]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
        {
            var section = await _courseContentService.CreateSectionAsync(dto);
            return Ok(section);
        }

        [HttpPut("sections/{id}")]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] CreateSectionDto dto)
        {
            var section = await _courseContentService.UpdateSectionAsync(id, dto);
            if (section == null) return NotFound("Section not found");
            return Ok(section);
        }

        [HttpDelete("sections/{id}")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var result = await _courseContentService.DeleteSectionAsync(id);
            if (!result) return NotFound("Section not found");
            return Ok("Section deleted successfully");
        }

        [HttpPost("lessons")]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonDto dto)
        {
            var lesson = await _courseContentService.CreateLessonAsync(dto);
            return Ok(lesson);
        }

        [HttpPut("lessons/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, [FromBody] CreateLessonDto dto)
        {
            var lesson = await _courseContentService.UpdateLessonAsync(id, dto);
            if (lesson == null) return NotFound("Lesson not found");
            return Ok(lesson);
        }

        [HttpDelete("lessons/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var result = await _courseContentService.DeleteLessonAsync(id);
            if (!result) return NotFound("Lesson not found");
            return Ok("Lesson deleted successfully");
        }
    }
}
