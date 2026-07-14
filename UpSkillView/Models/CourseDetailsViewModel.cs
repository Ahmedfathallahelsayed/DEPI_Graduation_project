using Application.CourseContent.DTOs;
using System.Collections.Generic;

namespace UpSkillView.Models
{
    public class CourseDetailsViewModel
    {
        public StudentCourseDetailsDto Details { get; set; }
        public List<SectionDto> Sections { get; set; }
    }
}
