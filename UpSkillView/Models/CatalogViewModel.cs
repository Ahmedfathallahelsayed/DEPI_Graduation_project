using Application.Courses.DTOs.Category;
using Application.CourseContent.DTOs;
using System.Collections.Generic;
using Domain.Enum;

namespace UpSkillView.Models
{
    public class CatalogViewModel
    {
        public IEnumerable<StudentCourseSummaryDto> Courses { get; set; } = new List<StudentCourseSummaryDto>();
        public IEnumerable<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();

        // Current filter states to pre-fill the UI
        public string? SearchQuery { get; set; }
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public List<CourseLevel> SelectedLevels { get; set; } = new List<CourseLevel>();
        public string? SelectedPrice { get; set; }
    }
}
