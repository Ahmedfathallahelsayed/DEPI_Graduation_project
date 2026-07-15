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
    public class CourseRepo : Repo<Course>, ICourse
    {
        public CourseRepo(AppDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Course>> GetByInstructorAsync(string instructorId)
        {
            return await dbContext.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseSections)
                .Where(c => c.InstructorId == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Course?> GetByIdWithDetailsAsync(int id)
        {
            return await dbContext.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Course>> GetPublishedAsync(string? search = null, int? categoryId = null)
        {
            var query = dbContext.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Where(c => c.Status == Domain.Enum.CourseStatus.Published && c.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<Course>> GetSubmittedForApprovalAsync()
        {
            return await dbContext.Courses
                .Include(c => c.Category)
                .Where(c => c.Status == Domain.Enum.CourseStatus.SubmittedForApproval)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }
    }
}
