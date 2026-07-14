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
            try
            {
                var section = await _courseContentService.CreateSectionAsync(dto);
                return Ok(section);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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

        // ── Helper Endpoint for Testing ─────────────────────────────────
        [HttpPost("seed-dummy-course")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedDummyCourse([FromServices] Infrastructure.Persistance.DbContext.AppDBContext dbContext)
        {
            // 1. Create a dummy Category if none exists
            var category = dbContext.Categories.FirstOrDefault();
            if (category == null)
            {
                category = new Domain.Entity.Category { Name = "Test Category", Description = "For Testing" };
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();
            }

            // 2. Create a dummy User (Instructor) if none exists
            var user = dbContext.Users.FirstOrDefault(u => u.Email == "test@instructor.com");
            if (user == null)
            {
                user = new Infrastructure.Persistance.AppUser 
                { 
                    UserName = "test_instructor", 
                    Email = "test@instructor.com",
                    FullName = "Test Instructor" 
                };
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            // 3. Create a dummy Course
            var course = new Domain.Entity.Course
            {
                Title = "كورس تجريبي لـ Member 3",
                ShortDescription = "وصف قصير للتجربة",
                Description = "هذا الكورس تم إنشاؤه تلقائياً لتجربة الـ Sections والـ Lessons",
                ThumbnailUrl = "https://example.com/thumbnail.png",
                Price = 0,
                Language = "Arabic",
                CategoryId = category.Id,
                InstructorId = user.Id
            };
            dbContext.Courses.Add(course);
            await dbContext.SaveChangesAsync();

            return Ok(new { 
                Message = "تم إنشاء كورس وهمي بنجاح عشان تجرب عليه!", 
                CourseId = course.Id,
                CourseTitle = course.Title
            });
        }
    }
}
