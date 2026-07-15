using Application.Courses.DTOs.Course;
using Application.Courses.DTOs.Section;
using System.Collections.Generic;

namespace UpSkillView.Models.Instructor
{
    public class ManageContentViewModel
    {
        public CourseResponseDto Course { get; set; } = new();
        public List<SectionResponseDto> Sections { get; set; } = new();
    }
}
