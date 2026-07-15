using Infrastructure.Persistance.DbContext;
using Infrastructure.Repo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDBContext dBContext;

        public UnitOfWork(AppDBContext dBContext,
                         ICategory categoryRepo,
                         ICertificate certificateRepo,
                         ICourse courseRepo,
                         ICourseSection courseSectionRepo,
                         IEnrollment enrollmentRepo, 
                         ILesson lessonRepo,
                         ILessonProgress lessonProgressRepo,
                         IOrder orderRepo,
                         IOrderItem orderItemRepo)
        {
            this.dBContext = dBContext;
            CategoryRepo = categoryRepo;
            CertificateRepo = certificateRepo;
            CourseRepo = courseRepo;
            CourseSectionRepo = courseSectionRepo;
            EnrollmentRepo = enrollmentRepo;
            LessonRepo = lessonRepo;
            LessonProgressRepo = lessonProgressRepo;
            OrderRepo = orderRepo;
            OrderItemRepo = orderItemRepo;
        }

        public ICategory CategoryRepo { get; }
        public ICertificate CertificateRepo { get; }
        public ICourse CourseRepo { get; }
        public ICourseSection CourseSectionRepo { get; }
        public IEnrollment EnrollmentRepo { get; }
        public ILesson LessonRepo { get; }
        public ILessonProgress LessonProgressRepo { get; }
        public IOrder OrderRepo { get; }
        public IOrderItem OrderItemRepo { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await dBContext.SaveChangesAsync();
        }
    }
}
