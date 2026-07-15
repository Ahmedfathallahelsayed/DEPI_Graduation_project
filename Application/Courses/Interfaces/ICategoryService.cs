using Application.Common;
using Application.Courses.DTOs.Category;

namespace Application.Courses.Interfaces
{
    /// <summary>
    /// Defines all Category management operations.
    /// Implemented in Infrastructure layer by CategoryService.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>Get all categories (active and inactive).</summary>
        Task<Result<IEnumerable<CategoryResponseDto>>> GetAllAsync();

        /// <summary>Get only active categories (used in course creation dropdowns).</summary>
        Task<Result<IEnumerable<CategoryResponseDto>>> GetActiveAsync();

        /// <summary>Get a single category by ID.</summary>
        Task<Result<CategoryResponseDto>> GetByIdAsync(int id);

        /// <summary>Create a new category. Admin only.</summary>
        Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto);

        /// <summary>Update an existing category. Admin only.</summary>
        Task<Result<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto);

        /// <summary>Delete a category by ID. Admin only.</summary>
        Task<Result> DeleteAsync(int id);
    }
}
