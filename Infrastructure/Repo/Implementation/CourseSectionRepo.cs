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
    public class CourseSectionRepo : Repo<CourseSection>, ICourseSection
    {
        public CourseSectionRepo(AppDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<CourseSection>> GetSectionsWithLessonsByCourseIdAsync(int courseId)
        {
            return await dbContext.CourseSections
                .Include(s => s.Lessons)
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<CourseSection?> GetSectionWithLessonsAsync(int sectionId)
        {
            return await dbContext.CourseSections
                .Include(s => s.Lessons)
                .FirstOrDefaultAsync(s => s.Id == sectionId);
        }
    }
}
