using Application.Common;
using Application.Courses.DTOs.Category;
using Application.Courses.Interfaces;
using Domain.Entity;
using Infrastructure.Persistance.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Service.Courses
{
    /// <summary>
    /// Implements ICategoryService using EF Core AppDBContext.
    /// Admin-only write operations are enforced at the controller level via [Authorize].
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly AppDBContext _context;

        public CategoryService(AppDBContext context)
        {
            _context = context;
        }

        // ── GET ALL ─────────────────────────────────────────────────────────

        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetAllAsync()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => MapToResponse(c))
                .ToListAsync();

            return Result<IEnumerable<CategoryResponseDto>>.Success(categories);
        }

        // ── GET ACTIVE ONLY ─────────────────────────────────────────────────

        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetActiveAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => MapToResponse(c))
                .ToListAsync();

            return Result<IEnumerable<CategoryResponseDto>>.Success(categories);
        }

        // ── GET BY ID ───────────────────────────────────────────────────────

        public async Task<Result<CategoryResponseDto>> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category is null)
                return Result<CategoryResponseDto>.Failure($"Category with ID {id} was not found.");

            return Result<CategoryResponseDto>.Success(MapToResponse(category));
        }

        // ── CREATE ──────────────────────────────────────────────────────────

        public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto)
        {
            // Enforce unique category name
            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (exists)
                return Result<CategoryResponseDto>.Failure($"A category with the name '{dto.Name}' already exists.");

            var category = new Category
            {
                Name        = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(MapToResponse(category));
        }

        // ── UPDATE ──────────────────────────────────────────────────────────

        public async Task<Result<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category is null)
                return Result<CategoryResponseDto>.Failure($"Category with ID {id} was not found.");

            // Check for name conflict with other categories
            var nameConflict = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

            if (nameConflict)
                return Result<CategoryResponseDto>.Failure($"Another category with the name '{dto.Name}' already exists.");

            category.Name        = dto.Name.Trim();
            category.Description = dto.Description?.Trim() ?? string.Empty;
            category.IsActive    = dto.IsActive;

            await _context.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(MapToResponse(category));
        }

        // ── DELETE ──────────────────────────────────────────────────────────

        public async Task<Result> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
                return Result.Failure($"Category with ID {id} was not found.");

            // Prevent deletion if courses are linked to this category
            if (category.Courses != null && category.Courses.Any())
                return Result.Failure("Cannot delete a category that has courses assigned to it.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        // ── MAPPING HELPER ───────────────────────────────────────────────────

        private static CategoryResponseDto MapToResponse(Category c) => new()
        {
            Id          = c.Id,
            Name        = c.Name,
            Description = c.Description,
            IsActive    = c.IsActive,
            CreatedAt   = c.CreatedAt
        };
    }
}
