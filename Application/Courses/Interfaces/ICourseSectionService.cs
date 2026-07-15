using Application.Common;
using Application.Courses.DTOs.Section;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Courses.Interfaces
{
    public interface ICourseSectionService
    {
        Task<Result<IEnumerable<SectionResponseDto>>> GetSectionsByCourseIdAsync(int courseId);
        Task<Result<SectionResponseDto>> GetByIdAsync(int id);
        Task<Result<SectionResponseDto>> CreateAsync(CreateSectionDto dto, string instructorId);
        Task<Result<SectionResponseDto>> UpdateAsync(int id, UpdateSectionDto dto, string instructorId);
        Task<Result> DeleteAsync(int id, string instructorId);
    }
}
