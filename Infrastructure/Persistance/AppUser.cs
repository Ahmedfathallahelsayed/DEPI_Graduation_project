using Domain.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance
{
    public class AppUser: IdentityUser
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual IEnumerable<Order> Orders { get; set; }
        public virtual IEnumerable<Enrollment> Enrollments { get; set; }
        public virtual IEnumerable<Course> AuthoredCourses { get; set; }
    }
}
