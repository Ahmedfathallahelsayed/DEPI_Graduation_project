using Application.CourseContent.DTOs;
using Application.CourseContent.Interfaces;
using Domain.Entity;
using Infrastructure.Persistance.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Service.CourseContent
{
    public class StudentLearningService : IStudentLearningService
    {
        private readonly AppDBContext _context;

        public StudentLearningService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SectionDto>> GetCourseHierarchyAsync(int courseId, string userId)
        {
            // 1. Check if user is enrolled
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            bool isEnrolled = enrollment != null;

            // 2. Get sections and lessons
            var sections = await _context.CourseSections
                .Include(s => s.Lessons)
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            // 3. Get lesson progress if enrolled
            List<LessonProgress> progresses = new List<LessonProgress>();
            if (isEnrolled)
            {
                progresses = await _context.LessonProgresses
                    .Where(lp => lp.EnrollmentId == enrollment.Id)
                    .ToListAsync();
            }

            // 4. Map to DTOs
            return sections.Select(section => new SectionDto
            {
                Id = section.Id,
                CourseId = section.CourseId,
                Title = section.Title,
                DisplayOrder = section.DisplayOrder,
                Lessons = section.Lessons.OrderBy(l => l.DisplayOrder).Select(lesson => new LessonDto
                {
                    Id = lesson.Id,
                    SectionId = lesson.SectionId,
                    Title = lesson.Title,
                    ContentType = lesson.ContentType,
                    DurationInMinutes = lesson.DurationInMinutes,
                    DisplayOrder = lesson.DisplayOrder,
                    IsPreview = lesson.IsPreview,
                    
                    // Hide content if not enrolled and not preview
                    VideoUrl = (isEnrolled || lesson.IsPreview) ? lesson.VideoUrl : null,
                    TextContent = (isEnrolled || lesson.IsPreview) ? lesson.TextContent : null,
                    AttachmentUrl = (isEnrolled || lesson.IsPreview) ? lesson.AttachmentUrl : null,

                    IsCompleted = progresses.Any(p => p.LessonId == lesson.Id && p.IsCompleted)
                }).ToList()
            });
        }

        public async Task<bool> MarkLessonCompleteAsync(int lessonId, string userId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return false;

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == lesson.Section.CourseId && e.StudentId == userId);

            if (enrollment == null) return false; // Student is not enrolled

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.EnrollmentId == enrollment.Id && lp.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    EnrollmentId = enrollment.Id,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow,
                    LastViewedAt = DateTime.UtcNow
                };
                _context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = progress.CompletedAt ?? DateTime.UtcNow;
                progress.LastViewedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Calculate progress percentage
            var totalLessons = await _context.Lessons
                .Where(l => l.Section.CourseId == lesson.Section.CourseId)
                .CountAsync();

            if (totalLessons > 0)
            {
                var completedLessons = await _context.LessonProgresses
                    .Where(lp => lp.EnrollmentId == enrollment.Id && lp.IsCompleted)
                    .CountAsync();

                enrollment.ProgressPercent = (decimal)completedLessons / totalLessons * 100;

                // Check for certificate generation condition if 100%
                if (enrollment.ProgressPercent == 100 && enrollment.CompletionStatus != Domain.Enum.CompletionStatus.Completed)
                {
                    enrollment.CompletionStatus = Domain.Enum.CompletionStatus.Completed;
                    
                    // You might trigger a certificate generation event here, handled by Member 4
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }
        public async Task<IEnumerable<StudentCourseSummaryDto>> GetStudentCatalogAsync(string userId, string? search = null, int? categoryId = null)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Lessons)
                .Where(c => c.Status == Domain.Enum.CourseStatus.Published && c.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Title.Contains(search) || c.ShortDescription.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            var courses = await query.ToListAsync();

            // Fetch user enrollments to set IsEnrolled flag
            var enrolledCourseIds = new HashSet<int>();
            if (!string.IsNullOrEmpty(userId))
            {
                var userEnrollments = await _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Select(e => e.CourseId)
                    .ToListAsync();
                enrolledCourseIds = new HashSet<int>(userEnrollments);
            }

            // Fetch Instructor Names
            var instructorIds = courses.Select(c => c.InstructorId).Distinct().ToList();
            var instructors = await _context.Users
                .Where(u => instructorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.UserName : u.FullName);

            return courses.Select(c => new StudentCourseSummaryDto
            {
                CourseId = c.Id,
                Title = c.Title,
                ShortDescription = c.ShortDescription,
                ThumbnailUrl = c.ThumbnailUrl,
                Price = c.Price,
                CategoryName = c.Category?.Name ?? "Uncategorized",
                InstructorName = instructors.ContainsKey(c.InstructorId) ? instructors[c.InstructorId] : "Unknown Instructor",
                Level = c.Level,
                TotalLessons = c.CourseSections.SelectMany(s => s.Lessons).Count(),
                IsEnrolled = enrolledCourseIds.Contains(c.Id)
            });
        }

        public async Task<StudentCourseDetailsDto?> GetStudentCourseDetailsAsync(int courseId, string userId)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.CourseSections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Status == Domain.Enum.CourseStatus.Published && c.IsApproved);

            if (course == null) return null;

            var enrollment = string.IsNullOrEmpty(userId) 
                ? null 
                : await _context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Id == course.InstructorId);
            var instructorName = instructor != null ? (string.IsNullOrWhiteSpace(instructor.FullName) ? instructor.UserName : instructor.FullName) : "Unknown Instructor";

            return new StudentCourseDetailsDto
            {
                CourseId = course.Id,
                Title = course.Title,
                FullDescription = course.Description,
                ThumbnailUrl = course.ThumbnailUrl,
                Price = course.Price,
                InstructorSummary = instructorName,
                CategoryName = course.Category?.Name ?? "Uncategorized",
                NumberOfSections = course.CourseSections.Count(),
                NumberOfLessons = course.CourseSections.SelectMany(s => s.Lessons).Count(),
                IsEnrolled = enrollment != null,
                ProgressPercent = enrollment?.ProgressPercent
            };
        }

        public async Task<Application.Common.Result> EnrollStudentAsync(int courseId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Application.Common.Result.Failure("User not authenticated.");

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.Status == Domain.Enum.CourseStatus.Published && c.IsApproved);

            if (course == null)
                return Application.Common.Result.Failure("Course not found or not published.");

            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (existingEnrollment != null)
                return Application.Common.Result.Failure("Student is already enrolled in this course.");

            var enrollment = new Enrollment
            {
                StudentId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                ProgressPercent = 0,
                CompletionStatus = Domain.Enum.CompletionStatus.NotStarted,
                LastAccessedAt = DateTime.UtcNow
                // OrderId would be set via payment flow, but we can set to 0 or nullable depending on schema.
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Application.Common.Result.Success();
        }

        public async Task<IEnumerable<MyCourseDto>> GetMyCoursesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<MyCourseDto>();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == userId)
                .OrderByDescending(e => e.LastAccessedAt)
                .ToListAsync();

            return enrollments.Select(e => new MyCourseDto
            {
                CourseId = e.CourseId,
                Title = e.Course?.Title ?? "Unknown Course",
                ThumbnailUrl = e.Course?.ThumbnailUrl,
                ProgressPercent = e.ProgressPercent,
                CompletionStatus = e.CompletionStatus,
                EnrolledAt = e.EnrollmentDate,
                LastAccessedAt = e.LastAccessedAt
            });
        }
    }
}
