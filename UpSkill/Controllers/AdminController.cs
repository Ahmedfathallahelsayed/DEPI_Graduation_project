using Application.Admin.DTOs;
using Application.Admin.Interfaces;
using Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UpSkill.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>Courses submitted for review and not yet approved.</summary>
        [HttpGet("pending-courses")]
        public async Task<IActionResult> GetPendingCourses()
        {
            var result = await _adminService.GetPendingCoursesAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>Approve a submitted course (does not publish).</summary>
        [HttpPost("courses/{id:int}/approve")]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            var result = await _adminService.ApproveCourseAsync(id);
            return ToActionResult(result);
        }

        /// <summary>Reject a submitted course → Draft (instructor can revise and resubmit).</summary>
        [HttpPost("courses/{id:int}/reject")]
        public async Task<IActionResult> RejectCourse(int id, [FromBody] RejectCourseDto? dto = null)
        {
            var result = await _adminService.RejectCourseAsync(id, dto);
            return ToActionResult(result);
        }

        /// <summary>Publish an approved course (catalog-visible). Only Admin can publish.</summary>
        [HttpPost("courses/{id:int}/publish")]
        public async Task<IActionResult> PublishCourse(int id)
        {
            var result = await _adminService.PublishCourseAsync(id);
            return ToActionResult(result);
        }

        /// <summary>Archive a published course (remove from catalog).</summary>
        [HttpPost("courses/{id:int}/archive")]
        public async Task<IActionResult> ArchiveCourse(int id)
        {
            var result = await _adminService.ArchiveCourseAsync(id);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }
    }
}
