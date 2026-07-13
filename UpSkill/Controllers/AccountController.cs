using Application.Account.DTOs;
using Application.Account.Interface;
using Application.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UpSkillAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IRoleService roleService;
        private readonly IUserService userService;
        private readonly IAccountService accountService;

        public AccountController(IRoleService roleService, IUserService userService, IAccountService accountService)
        {
            this.roleService = roleService;
            this.userService = userService;
            this.accountService = accountService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var result = await roleService.CreateRoleAsync(roleName);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result.Error);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("GetAllRoles")]
        public IActionResult GetAllRoles()
        {
            var roles = roleService.GetAllRoles();
            return Ok(roles);
        }
        [AllowAnonymous]
        [HttpPost("RegisterAsInstructor")]
        public async Task<IActionResult> RegisterAsInstructor([FromBody] RegisterReq request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await accountService.RegisterAsInstructorAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result.Error);
        }
        [AllowAnonymous]
        [HttpPost("RegisterAsStudent")]
        public async Task<IActionResult> RegisterAsStudent([FromBody] RegisterReq request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await accountService.ReqisterAsStudentAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result.Error);
        }

        [AllowAnonymous]
        [HttpPost("RegisterAsAdmin")]
        public async Task<IActionResult> RegisterAsAdmin([FromBody] RegisterReq request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await accountService.RegisterAsAdmin(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result.Error);
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginReq request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await accountService.Login(request);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return BadRequest(result.Error);
        }

    }
}
