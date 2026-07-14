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
    public class LessonRepo : Repo<Lesson>, ILesson
    {
        public LessonRepo(AppDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<Lesson?> GetLessonWithSectionAndCourseAsync(int lessonId)
        {
            return await dbContext.Lessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.CourseSections)
                            .ThenInclude(cs => cs.Lessons)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
        }
    }
}
