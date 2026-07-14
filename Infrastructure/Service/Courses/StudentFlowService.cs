using Application.Common;
using Application.Courses.DTOs.Student;
using Application.Courses.Interfaces;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service.Courses
{
    public class StudentFlowService : IStudentFlowService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentFlowService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ── ENROLL IN COURSE ──────────────────────────────────────────────────

        public async Task<Result<EnrollmentResponseDto>> EnrollInCourseAsync(int courseId, string studentId)
        {
            var course = await _unitOfWork.CourseRepo.getById(courseId);
            if (course is null)
                return Result<EnrollmentResponseDto>.Failure($"Course with ID {courseId} was not found.");

            if (course.Status != CourseStatus.Published || !course.IsApproved)
                return Result<EnrollmentResponseDto>.Failure("This course is not currently available for enrollment.");

            var existing = await _unitOfWork.EnrollmentRepo.GetEnrollmentByStudentAndCourseAsync(studentId, courseId);
            if (existing is not null)
                return Result<EnrollmentResponseDto>.Failure("You are already enrolled in this course.");

            // Create Order
            var order = new Order
            {
                StudentId     = studentId,
                OrderNumber   = "ORD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                TotalAmount   = course.Price,
                PaymentMethod = PaymentMethod.Visa,
                PaymentStatus = PaymentStatus.Completed,
                OrderedAt     = DateTime.UtcNow
            };

            await _unitOfWork.OrderRepo.Create(order);
            await _unitOfWork.SaveChangesAsync(); // Fetch order ID

            // Create OrderItem
            var orderItem = new OrderItem
            {
                OrderId   = order.Id,
                CourseId  = courseId,
                UnitPrice = course.Price
            };
            await _unitOfWork.OrderItemRepo.Create(orderItem);

            // Create Enrollment
            var enrollment = new Enrollment
            {
                StudentId        = studentId,
                CourseId         = courseId,
                OrderId          = order.Id,
                EnrollmentDate   = DateTime.UtcNow,
                ProgressPercent  = 0,
                CompletionStatus = CompletionStatus.NotStarted,
                LastAccessedAt   = DateTime.UtcNow
            };

            await _unitOfWork.EnrollmentRepo.Create(enrollment);
            await _unitOfWork.SaveChangesAsync();

            var response = new EnrollmentResponseDto
            {
                Id               = enrollment.Id,
                StudentId        = enrollment.StudentId,
                CourseId         = enrollment.CourseId,
                CourseTitle      = course.Title,
                EnrollmentDate   = enrollment.EnrollmentDate,
                ProgressPercent  = enrollment.ProgressPercent,
                CompletionStatus = enrollment.CompletionStatus,
                LastAccessedAt   = enrollment.LastAccessedAt
            };

            return Result<EnrollmentResponseDto>.Success(response);
        }

        // ── GET MY COURSES ────────────────────────────────────────────────────

        public async Task<Result<IEnumerable<EnrollmentResponseDto>>> GetMyCoursesAsync(string studentId)
        {
            var enrollments = await _unitOfWork.EnrollmentRepo.GetStudentEnrollmentsWithDetailsAsync(studentId);
            var dtos = enrollments.Select(e => new EnrollmentResponseDto
            {
                Id               = e.Id,
                StudentId        = e.StudentId,
                CourseId         = e.CourseId,
                CourseTitle      = e.Course?.Title ?? "Unknown",
                EnrollmentDate   = e.EnrollmentDate,
                ProgressPercent  = e.ProgressPercent,
                CompletionStatus = e.CompletionStatus,
                LastAccessedAt   = e.LastAccessedAt
            });

            return Result<IEnumerable<EnrollmentResponseDto>>.Success(dtos);
        }

        // ── GET COURSE PROGRESS ───────────────────────────────────────────────

        public async Task<Result<StudentCourseProgressDto>> GetCourseProgressAsync(int courseId, string studentId)
        {
            var enrollment = await _unitOfWork.EnrollmentRepo.GetEnrollmentByStudentAndCourseAsync(studentId, courseId);
            if (enrollment is null)
                return Result<StudentCourseProgressDto>.Failure("You are not enrolled in this course.");

            var completedLessonIds = enrollment.LessonProgresses?
                .Where(lp => lp.IsCompleted)
                .Select(lp => lp.LessonId)
                .ToList() ?? new List<int>();

            var dto = new StudentCourseProgressDto
            {
                EnrollmentId       = enrollment.Id,
                CourseId           = enrollment.CourseId,
                ProgressPercent    = enrollment.ProgressPercent,
                CompletionStatus   = enrollment.CompletionStatus.ToString(),
                CompletedLessonIds = completedLessonIds
            };

            return Result<StudentCourseProgressDto>.Success(dto);
        }

        // ── COMPLETE LESSON ───────────────────────────────────────────────────

        public async Task<Result<StudentCourseProgressDto>> CompleteLessonAsync(int lessonId, string studentId)
        {
            var lesson = await _unitOfWork.LessonRepo.GetLessonWithSectionAndCourseAsync(lessonId);
            if (lesson is null)
                return Result<StudentCourseProgressDto>.Failure($"Lesson with ID {lessonId} was not found.");

            var courseId = lesson.Section?.CourseId ?? 0;
            var enrollment = await _unitOfWork.EnrollmentRepo.GetEnrollmentByStudentAndCourseAsync(studentId, courseId);
            if (enrollment is null)
                return Result<StudentCourseProgressDto>.Failure("You are not enrolled in the course containing this lesson.");

            // Check if progress already exists
            var progress = await _unitOfWork.LessonProgressRepo.GetProgressAsync(enrollment.Id, lessonId);
            if (progress is null)
            {
                progress = new LessonProgress
                {
                    EnrollmentId = enrollment.Id,
                    LessonId     = lessonId,
                    IsCompleted  = true,
                    CompletedAt  = DateTime.UtcNow,
                    LastViewedAt = DateTime.UtcNow
                };
                await _unitOfWork.LessonProgressRepo.Create(progress);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted  = true;
                progress.CompletedAt  = DateTime.UtcNow;
                progress.LastViewedAt = DateTime.UtcNow;
                await _unitOfWork.LessonProgressRepo.Update(progress);
            }
            else
            {
                // Already completed, just update last viewed
                progress.LastViewedAt = DateTime.UtcNow;
                await _unitOfWork.LessonProgressRepo.Update(progress);
            }

            await _unitOfWork.SaveChangesAsync(); // Save progress change first

            // Recalculate course completion progress
            var sections = await _unitOfWork.CourseSectionRepo.GetSectionsWithLessonsByCourseIdAsync(courseId);
            var totalLessons = sections.Sum(s => s.Lessons?.Count() ?? 0);

            if (totalLessons > 0)
            {
                var completedProgresses = await _unitOfWork.LessonProgressRepo.GetProgressesForEnrollmentAsync(enrollment.Id);
                var completedCount = completedProgresses.Count(p => p.IsCompleted);

                enrollment.ProgressPercent = Math.Round(((decimal)completedCount / totalLessons) * 100, 2);
                enrollment.LastAccessedAt  = DateTime.UtcNow;

                if (enrollment.ProgressPercent >= 100)
                {
                    enrollment.CompletionStatus = CompletionStatus.Completed;

                    // Generate Certificate
                    var certificates = await _unitOfWork.CertificateRepo.getAll();
                    var hasCert = certificates.Any(c => c.EnrollmentId == enrollment.Id);
                    if (!hasCert)
                    {
                        var certificate = new Certificate
                        {
                            EnrollmentId    = enrollment.Id,
                            CertificateCode = "CERT-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                            IssuedAt        = DateTime.UtcNow,
                            CertificateUrl  = $"/certificates/{enrollment.Id}.pdf"
                        };
                        await _unitOfWork.CertificateRepo.Create(certificate);
                    }
                }
                else
                {
                    enrollment.CompletionStatus = CompletionStatus.InProgress;
                }

                await _unitOfWork.EnrollmentRepo.Update(enrollment);
                await _unitOfWork.SaveChangesAsync();
            }

            // Get refreshed progress
            return await GetCourseProgressAsync(courseId, studentId);
        }

        // ── GET CERTIFICATE ───────────────────────────────────────────────────

        public async Task<Result<CertificateResponseDto>> GetCertificateAsync(int enrollmentId, string studentId)
        {
            var enrollment = await _unitOfWork.EnrollmentRepo.getById(enrollmentId);
            if (enrollment is null || enrollment.StudentId != studentId)
                return Result<CertificateResponseDto>.Failure("Enrollment record not found or access denied.");

            var course = await _unitOfWork.CourseRepo.getById(enrollment.CourseId);

            var certificates = await _unitOfWork.CertificateRepo.getAll();
            var cert = certificates.FirstOrDefault(c => c.EnrollmentId == enrollmentId);

            if (cert is null)
                return Result<CertificateResponseDto>.Failure("No certificate has been generated for this enrollment yet.");

            var dto = new CertificateResponseDto
            {
                Id              = cert.Id,
                EnrollmentId    = cert.EnrollmentId,
                CourseTitle     = course?.Title ?? "Unknown Course",
                CertificateCode = cert.CertificateCode,
                IssuedAt        = cert.IssuedAt,
                CertificateUrl  = cert.CertificateUrl
            };

            return Result<CertificateResponseDto>.Success(dto);
        }
    }
}
