using Application.Common;
using Application.Courses.DTOs.Lesson;
using Application.Courses.DTOs.Section;
using Application.Courses.Interfaces;
using Domain.Entity;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service.Courses
{
    public class CourseSectionService : ICourseSectionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseSectionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<SectionResponseDto>>> GetSectionsByCourseIdAsync(int courseId)
        {
            var sections = await _unitOfWork.CourseSectionRepo.GetSectionsWithLessonsByCourseIdAsync(courseId);
            var dtos = sections.Select(MapToResponse);
            return Result<IEnumerable<SectionResponseDto>>.Success(dtos);
        }

        public async Task<Result<SectionResponseDto>> GetByIdAsync(int id)
        {
            var section = await _unitOfWork.CourseSectionRepo.GetSectionWithLessonsAsync(id);
            if (section is null)
                return Result<SectionResponseDto>.Failure($"Section with ID {id} was not found.");

            return Result<SectionResponseDto>.Success(MapToResponse(section));
        }

        public async Task<Result<SectionResponseDto>> CreateAsync(CreateSectionDto dto, string instructorId)
        {
            var course = await _unitOfWork.CourseRepo.getById(dto.CourseId);
            if (course is null)
                return Result<SectionResponseDto>.Failure($"Course with ID {dto.CourseId} was not found.");

            if (course.InstructorId != instructorId)
                return Result<SectionResponseDto>.Failure("You are not authorized to add sections to this course.");

            var section = new CourseSection
            {
                CourseId     = dto.CourseId,
                Title        = dto.Title.Trim(),
                DisplayOrder = dto.DisplayOrder,
                CreatedAt    = DateTime.UtcNow
            };

            await _unitOfWork.CourseSectionRepo.Create(section);
            await _unitOfWork.SaveChangesAsync();

            return Result<SectionResponseDto>.Success(MapToResponse(section));
        }

        public async Task<Result<SectionResponseDto>> UpdateAsync(int id, UpdateSectionDto dto, string instructorId)
        {
            var section = await _unitOfWork.CourseSectionRepo.getById(id);
            if (section is null)
                return Result<SectionResponseDto>.Failure($"Section with ID {id} was not found.");

            var course = await _unitOfWork.CourseRepo.getById(section.CourseId);
            if (course is null || course.InstructorId != instructorId)
                return Result<SectionResponseDto>.Failure("You are not authorized to update this section.");

            section.Title        = dto.Title.Trim();
            section.DisplayOrder = dto.DisplayOrder;

            await _unitOfWork.CourseSectionRepo.Update(section);
            await _unitOfWork.SaveChangesAsync();

            // Refresh details to include lessons
            var updatedSection = await _unitOfWork.CourseSectionRepo.GetSectionWithLessonsAsync(id);
            return Result<SectionResponseDto>.Success(MapToResponse(updatedSection ?? section));
        }

        public async Task<Result> DeleteAsync(int id, string instructorId)
        {
            var section = await _unitOfWork.CourseSectionRepo.GetSectionWithLessonsAsync(id);
            if (section is null)
                return Result.Failure($"Section with ID {id} was not found.");

            var course = await _unitOfWork.CourseRepo.getById(section.CourseId);
            if (course is null || course.InstructorId != instructorId)
                return Result.Failure("You are not authorized to delete this section.");

            if (section.Lessons != null && section.Lessons.Any())
                return Result.Failure("Cannot delete section containing lessons. Please delete lessons first.");

            await _unitOfWork.CourseSectionRepo.Delete(id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        private static SectionResponseDto MapToResponse(CourseSection s) => new()
        {
            Id           = s.Id,
            CourseId     = s.CourseId,
            Title        = s.Title,
            DisplayOrder = s.DisplayOrder,
            CreatedAt    = s.CreatedAt,
            Lessons      = s.Lessons?.Select(l => new LessonResponseDto
            {
                Id                = l.Id,
                SectionId         = l.SectionId,
                Title             = l.Title,
                ContentType       = l.ContentType,
                VideoUrl          = l.VideoUrl ?? string.Empty,
                TextContent       = l.TextContent ?? string.Empty,
                AttachmentUrl     = l.AttachmentUrl ?? string.Empty,
                DurationInMinutes = l.DurationInMinutes,
                DisplayOrder      = l.DisplayOrder,
                IsPreview         = l.IsPreview,
                CreatedAt         = l.CreatedAt
            }).OrderBy(l => l.DisplayOrder).ToList() ?? new()
        };
    }
}
