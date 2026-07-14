using System.Collections.Generic;
using Application.CourseContent.DTOs;

namespace UpSkillView.Models
{
    public class CoursePlayerViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public decimal ProgressPercentage { get; set; }
        public List<SectionDto> Sections { get; set; }
    }
}
