using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class Lesson
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public LessonContentType ContentType { get; set; }
        public string VideoUrl { get; set; }
        public string TextContent { get; set; }
        public string AttachmentUrl { get; set; }
        public int DurationInMinutes { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPreview { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual CourseSection Section { get; set; }
        public virtual IEnumerable<LessonProgress> LessonProgresses { get; set; }
    }
}
