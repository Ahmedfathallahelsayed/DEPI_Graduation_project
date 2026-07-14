using Domain.Entity;
using Infrastructure.Repos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo.Contracts
{
    public interface ILesson : IRepo<Lesson>
    {
        Task<Lesson?> GetLessonWithSectionAndCourseAsync(int lessonId);
    }
}
