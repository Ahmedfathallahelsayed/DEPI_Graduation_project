using Application.CourseContent.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.CourseContent.Interfaces
{
    public interface IStudentLearningService
    {
        Task<IEnumerable<SectionDto>> GetCourseHierarchyAsync(int courseId, string userId);
        Task<bool> MarkLessonCompleteAsync(int lessonId, string userId);
        Task<IEnumerable<StudentCourseSummaryDto>> GetStudentCatalogAsync(string userId, string? search = null, int? categoryId = null);
        Task<StudentCourseDetailsDto?> GetStudentCourseDetailsAsync(int courseId, string userId);
        Task<Application.Common.Result> EnrollStudentAsync(int courseId, string userId);
        Task<IEnumerable<MyCourseDto>> GetMyCoursesAsync(string userId);
    }
}
