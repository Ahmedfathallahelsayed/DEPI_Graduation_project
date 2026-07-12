using Application.CourseContent.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.CourseContent.Interfaces
{
    public interface IStudentLearningService
    {
        Task<IEnumerable<SectionDto>> GetCourseHierarchyAsync(int courseId, string userId);
        Task<bool> MarkLessonCompleteAsync(int lessonId, string userId);
    }
}
