using Domain.Entity;
using Infrastructure.Repos.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repo.Contracts
{
    public interface IEnrollment : IRepo<Enrollment>
    {
        Task<Enrollment?> GetEnrollmentWithDetailsAsync(int enrollmentId);
        Task<Enrollment?> GetEnrollmentByStudentAndCourseAsync(string studentId, int courseId);
        Task<List<Enrollment>> GetStudentEnrollmentsWithDetailsAsync(string studentId);
    }
}
