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
    }
}
