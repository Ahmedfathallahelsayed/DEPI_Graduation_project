using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class Course
    {
        public int Id { get; set; }
        public string InstructorId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public string Language { get; set; }
        public CourseStatus Status { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? RejectionReason { get; set; }

        // Navigation Properties
        public virtual Category Category { get; set; }
        public virtual IEnumerable<CourseSection> CourseSections { get; set; }
        public virtual IEnumerable<Enrollment> Enrollments { get; set; }
        public virtual IEnumerable<OrderItem> OrderItems { get; set; }
    }
}
