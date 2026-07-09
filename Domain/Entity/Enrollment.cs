using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class Enrollment
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public int OrderId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public decimal ProgressPercent { get; set; } = 0;
        public CompletionStatus CompletionStatus { get; set; }
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Course Course { get; set; }
        public virtual Order Order { get; set; }
        public virtual IEnumerable<Certificate> Certificates { get; set; }
        public virtual IEnumerable<LessonProgress> LessonProgresses { get; set; }
    }
}
