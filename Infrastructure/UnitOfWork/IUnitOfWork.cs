using Infrastructure.Repo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICategory CategoryRepo { get; }
        ICertificate CertificateRepo { get; }
        ICourse CourseRepo { get; }
        ICourseSection CourseSectionRepo { get; }
        IEnrollment EnrollmentRepo { get; }
        ILesson LessonRepo { get; }
        ILessonProgress LessonProgressRepo { get; }
        IOrder OrderRepo { get; }
        IOrderItem OrderItemRepo { get; }

        public Task<int> SaveChangesAsync();
    }
}
