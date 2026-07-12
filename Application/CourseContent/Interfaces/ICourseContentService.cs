using Application.CourseContent.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.CourseContent.Interfaces
{
    public interface ICourseContentService
    {
        // Sections
        Task<SectionDto> CreateSectionAsync(CreateSectionDto dto);
        Task<SectionDto> UpdateSectionAsync(int id, CreateSectionDto dto);
        Task<bool> DeleteSectionAsync(int id);
        Task<IEnumerable<SectionDto>> GetCourseSectionsAsync(int courseId);

        // Lessons
        Task<LessonDto> CreateLessonAsync(CreateLessonDto dto);
        Task<LessonDto> UpdateLessonAsync(int id, CreateLessonDto dto);
        Task<bool> DeleteLessonAsync(int id);
    }
}
