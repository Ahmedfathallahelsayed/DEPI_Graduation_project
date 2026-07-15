using Application.Common;
using Application.Courses.DTOs.Category;
using Application.Courses.Interfaces;
using Domain.Entity;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service.Courses
{
    /// <summary>
    /// Implements ICategoryService using Repository and Unit of Work patterns.
    /// Admin-only write operations are enforced at the controller level via [Authorize].
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ── GET ALL ─────────────────────────────────────────────────────────

        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetAllAsync()
        {
            var categories = await _unitOfWork.CategoryRepo.getAll();
            var dtos = categories.OrderBy(c => c.Name).Select(MapToResponse);
            return Result<IEnumerable<CategoryResponseDto>>.Success(dtos);
        }

        // ── GET ACTIVE ONLY ─────────────────────────────────────────────────

        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetActiveAsync()
        {
            var categories = await _unitOfWork.CategoryRepo.getAll();
            var dtos = categories.Where(c => c.IsActive).OrderBy(c => c.Name).Select(MapToResponse);
            return Result<IEnumerable<CategoryResponseDto>>.Success(dtos);
        }

        // ── GET BY ID ───────────────────────────────────────────────────────

        public async Task<Result<CategoryResponseDto>> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.CategoryRepo.getById(id);
            if (category is null)
                return Result<CategoryResponseDto>.Failure($"Category with ID {id} was not found.");

            return Result<CategoryResponseDto>.Success(MapToResponse(category));
        }

        // ── CREATE ──────────────────────────────────────────────────────────

        public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto)
        {
            var categories = await _unitOfWork.CategoryRepo.getAll();
            var exists = categories.Any(c => c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (exists)
                return Result<CategoryResponseDto>.Failure($"A category with the name '{dto.Name}' already exists.");

            var category = new Category
            {
                Name        = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow
            };

            await _unitOfWork.CategoryRepo.Create(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(MapToResponse(category));
        }

        // ── UPDATE ──────────────────────────────────────────────────────────

        public async Task<Result<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.CategoryRepo.getById(id);
            if (category is null)
                return Result<CategoryResponseDto>.Failure($"Category with ID {id} was not found.");

            var categories = await _unitOfWork.CategoryRepo.getAll();
            var nameConflict = categories.Any(c => c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase) && c.Id != id);
            if (nameConflict)
                return Result<CategoryResponseDto>.Failure($"Another category with the name '{dto.Name}' already exists.");

            category.Name        = dto.Name.Trim();
            category.Description = dto.Description?.Trim() ?? string.Empty;
            category.IsActive    = dto.IsActive;

            await _unitOfWork.CategoryRepo.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(MapToResponse(category));
        }

        // ── DELETE ──────────────────────────────────────────────────────────

        public async Task<Result> DeleteAsync(int id)
        {
            var courses = await _unitOfWork.CourseRepo.getAll();
            var hasCourses = courses.Any(c => c.CategoryId == id);
            if (hasCourses)
                return Result.Failure("Cannot delete a category that has courses assigned to it.");

            var category = await _unitOfWork.CategoryRepo.Delete(id);
            if (category is null)
                return Result.Failure($"Category with ID {id} was not found.");

            await _unitOfWork.SaveChangesAsync();
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
