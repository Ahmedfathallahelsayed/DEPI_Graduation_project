using Application.CourseContent.DTOs;
using System.Collections.Generic;

namespace UpSkillView.Models.Instructor
{
    public class ManageContentViewModel
    {
        public int CourseId { get; set; }
        public List<SectionDto> Sections { get; set; } = new List<SectionDto>();
    }
}
