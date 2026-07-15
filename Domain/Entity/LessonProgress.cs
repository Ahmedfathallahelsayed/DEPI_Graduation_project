using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class LessonProgress
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int LessonId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastViewedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Enrollment Enrollment { get; set; }
        public virtual Lesson Lesson { get; set; }
    }
}
