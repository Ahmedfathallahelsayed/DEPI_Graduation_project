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
    public class LessonProgressRepo : Repo<LessonProgress>, ILessonProgress
    {
        public LessonProgressRepo(AppDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<LessonProgress?> GetProgressAsync(int enrollmentId, int lessonId)
        {
            return await dbContext.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.EnrollmentId == enrollmentId && lp.LessonId == lessonId);
        }

        public async Task<List<LessonProgress>> GetProgressesForEnrollmentAsync(int enrollmentId)
        {
            return await dbContext.LessonProgresses
                .Where(lp => lp.EnrollmentId == enrollmentId)
                .ToListAsync();
        }
    }
}
