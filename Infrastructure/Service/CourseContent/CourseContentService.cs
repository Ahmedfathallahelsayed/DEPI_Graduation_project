using Application.CourseContent.DTOs;
using Application.CourseContent.Interfaces;
using Domain.Entity;
using Infrastructure.Persistance.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service.CourseContent
{
    public class CourseContentService : ICourseContentService
    {
        private readonly AppDBContext _context;

        public CourseContentService(AppDBContext context)
        {
            _context = context;
        }

        // Sections
        public async Task<SectionDto> CreateSectionAsync(CreateSectionDto dto)
        {
            // Validate if the course exists before adding the section
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
            {
                throw new System.ArgumentException($"Course with ID {dto.CourseId} does not exist.");
            }

            var section = new CourseSection
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                DisplayOrder = dto.DisplayOrder
            };

            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            return MapToDto(section);
        }

        public async Task<SectionDto> UpdateSectionAsync(int id, CreateSectionDto dto)
        {
            var section = await _context.CourseSections.FindAsync(id);
            if (section == null) return null;

            section.Title = dto.Title;
            section.DisplayOrder = dto.DisplayOrder;

            await _context.SaveChangesAsync();
            return MapToDto(section);
        }

        public async Task<bool> DeleteSectionAsync(int id)
        {
            var section = await _context.CourseSections.FindAsync(id);
            if (section == null) return false;

            _context.CourseSections.Remove(section);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SectionDto>> GetCourseSectionsAsync(int courseId)
        {
            var sections = await _context.CourseSections
                .Include(s => s.Lessons)
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return sections.Select(MapToDto);
        }

        // Lessons
        public async Task<LessonDto> CreateLessonAsync(CreateLessonDto dto)
        {
            var lesson = new Lesson
            {
                SectionId = dto.SectionId,
                Title = dto.Title,
                ContentType = dto.ContentType,
                VideoUrl = dto.VideoUrl ?? "",
                TextContent = dto.TextContent ?? "",
                AttachmentUrl = dto.AttachmentUrl ?? "",
                DurationInMinutes = dto.DurationInMinutes,
                DisplayOrder = dto.DisplayOrder,
                IsPreview = dto.IsPreview
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return MapToDto(lesson);
        }

        public async Task<LessonDto> UpdateLessonAsync(int id, CreateLessonDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return null;

            lesson.Title = dto.Title;
            lesson.ContentType = dto.ContentType;
            lesson.VideoUrl = dto.VideoUrl ?? "";
            lesson.TextContent = dto.TextContent ?? "";
            lesson.AttachmentUrl = dto.AttachmentUrl ?? "";
            lesson.DurationInMinutes = dto.DurationInMinutes;
            lesson.DisplayOrder = dto.DisplayOrder;
            lesson.IsPreview = dto.IsPreview;

            await _context.SaveChangesAsync();
            return MapToDto(lesson);
        }

        public async Task<bool> DeleteLessonAsync(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return true;
        }

        // Mapping Helpers
        private SectionDto MapToDto(CourseSection section)
        {
            return new SectionDto
            {
                Id = section.Id,
                CourseId = section.CourseId,
                Title = section.Title,
                DisplayOrder = section.DisplayOrder,
                Lessons = section.Lessons?.OrderBy(l => l.DisplayOrder).Select(MapToDto).ToList() ?? new List<LessonDto>()
            };
        }

        private LessonDto MapToDto(Lesson lesson)
        {
            return new LessonDto
            {
                Id = lesson.Id,
                SectionId = lesson.SectionId,
                Title = lesson.Title,
                ContentType = lesson.ContentType,
                VideoUrl = lesson.VideoUrl,
                TextContent = lesson.TextContent,
                AttachmentUrl = lesson.AttachmentUrl,
                DurationInMinutes = lesson.DurationInMinutes,
                DisplayOrder = lesson.DisplayOrder,
                IsPreview = lesson.IsPreview
            };
        }
    }
}
