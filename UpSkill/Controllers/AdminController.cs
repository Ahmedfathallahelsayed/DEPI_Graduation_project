using Application.Admin.DTOs;
using Application.Admin.Interfaces;
using Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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

        /// <summary>LMS dashboard statistics for admins.</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _adminService.GetDashboardStatsAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>Courses submitted for review and not yet approved.</summary>
        [HttpGet("pending-courses")]
        public async Task<IActionResult> GetPendingCourses()
        {
            var result = await _adminService.GetPendingCoursesAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>Full course review details (sections, lessons, resources) for a pending course.</summary>
        [HttpGet("pending-courses/{courseId:int}")]
        public async Task<IActionResult> GetPendingCourseDetails(int courseId)
        {
            var result = await _adminService.GetPendingCourseDetailsAsync(courseId);
            return ToActionResult(result);
        }

        /// <summary>Approve a submitted course (directly publishes it).</summary>
        [HttpPost("courses/{id:int}/approve")]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            var result = await _adminService.ApproveCourseAsync(id);
            return ToActionResult(result);
        }

        /// <summary>Reject a submitted course with a required reason.</summary>
        [HttpPost("courses/{id:int}/reject")]
        public async Task<IActionResult> RejectCourse(int id, [FromBody] RejectCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.RejectCourseAsync(id, dto);
            return ToActionResult(result);
        }

        /// <summary>Archive a published course (remove from catalog).</summary>
        [HttpPost("courses/{id:int}/archive")]
        public async Task<IActionResult> ArchiveCourse(int id)
        {
            var result = await _adminService.ArchiveCourseAsync(id);
            return ToActionResult(result);
        }

        // ── User Management Endpoints ───────────────────────────────────

        /// <summary>Get list of all users in the system.</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _adminService.GetAllUsersAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>Block a user account.</summary>
        [HttpPost("users/{id}/block")]
        public async Task<IActionResult> BlockUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _adminService.BlockUserAsync(id, currentUserId);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }

        /// <summary>Unblock a user account.</summary>
        [HttpPost("users/{id}/unblock")]
        public async Task<IActionResult> UnblockUser(string id)
        {
            var result = await _adminService.UnblockUserAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }

        /// <summary>Edit user details (Full Name and Phone Number only).</summary>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] EditUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.EditUserAsync(id, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }

        private IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result.Value);

            return BadRequest(result.Error);
        }
    }
}
