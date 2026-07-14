using Application.Common;
using Application.Courses.DTOs.Lesson;
using System.Threading.Tasks;

namespace Application.Courses.Interfaces
{
    public interface ILessonService
    {
        Task<Result<LessonResponseDto>> GetByIdAsync(int id);
        Task<Result<LessonResponseDto>> CreateAsync(CreateLessonDto dto, string instructorId);
        Task<Result<LessonResponseDto>> UpdateAsync(int id, UpdateLessonDto dto, string instructorId);
        Task<Result> DeleteAsync(int id, string instructorId);
        Task<Result> CompleteLessonAsync(int lessonId, string studentId);
    }
}
