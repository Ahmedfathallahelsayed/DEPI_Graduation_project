using Domain.Entity;
using Infrastructure.Persistance.DbContext;
using Infrastructure.Repo.Contracts;
using Infrastructure.Repos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repo.Implementation
{
    public class EnrollmentRepo : Repo<Enrollment>, IEnrollment
    {
        public EnrollmentRepo(AppDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId)
        {
            return await dbContext.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseSections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.LessonProgresses)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);
        }

        public async Task<Enrollment?> GetEnrollmentByStudentAndCourseAsync(string studentId, int courseId)
        {
            return await dbContext.Enrollments
                .Include(e => e.Course)
                .Include(e => e.LessonProgresses)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<List<Enrollment>> GetStudentEnrollmentsWithDetailsAsync(string studentId)
        {
            return await dbContext.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();
        }
    }
}
