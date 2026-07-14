using Application.Courses.DTOs.Category;
using Application.Courses.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UpSkill.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // ── GET /api/category ─────────────────────────────────────────────
        /// <summary>Get all categories (Admin sees all, public sees active only).</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetActiveAsync();
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        // ── GET /api/category/all ─────────────────────────────────────────
        /// <summary>Get ALL categories including inactive. Admin only.</summary>
        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var result = await _categoryService.GetAllAsync();
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        // ── GET /api/category/{id} ────────────────────────────────────────
        /// <summary>Get a single category by ID.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }

        // ── POST /api/category ────────────────────────────────────────────
        /// <summary>Create a new category. Admin only.</summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _categoryService.CreateAsync(dto);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(result.Error);
        }

        // ── PUT /api/category/{id} ────────────────────────────────────────
        /// <summary>Update an existing category. Admin only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _categoryService.UpdateAsync(id, dto);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        // ── DELETE /api/category/{id} ─────────────────────────────────────
        /// <summary>Delete a category. Admin only. Fails if courses are assigned to it.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return result.IsSuccess
                ? NoContent()
                : BadRequest(result.Error);
        }
    }
}
