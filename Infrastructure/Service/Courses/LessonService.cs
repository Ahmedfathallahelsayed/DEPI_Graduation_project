using Application.Common;
using Application.Courses.DTOs.Lesson;
using Application.Courses.Interfaces;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Service.Courses
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LessonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LessonResponseDto>> GetByIdAsync(int id)
        {
            var lesson = await _unitOfWork.LessonRepo.getById(id);
            if (lesson is null)
                return Result<LessonResponseDto>.Failure($"Lesson with ID {id} was not found.");

            return Result<LessonResponseDto>.Success(MapToResponse(lesson));
        }

        public async Task<Result<LessonResponseDto>> CreateAsync(CreateLessonDto dto, string instructorId)
        {
            var section = await _unitOfWork.CourseSectionRepo.getById(dto.SectionId);
            if (section is null)
                return Result<LessonResponseDto>.Failure($"Section with ID {dto.SectionId} was not found.");

            var course = await _unitOfWork.CourseRepo.getById(section.CourseId);
            if (course is null || course.InstructorId != instructorId)
                return Result<LessonResponseDto>.Failure("You are not authorized to add lessons to this section.");

            var lesson = new Lesson
            {
                SectionId         = dto.SectionId,
                Title             = dto.Title.Trim(),
                ContentType       = dto.ContentType,
                VideoUrl          = dto.VideoUrl?.Trim() ?? string.Empty,
                TextContent       = dto.TextContent?.Trim() ?? string.Empty,
                AttachmentUrl     = dto.AttachmentUrl?.Trim() ?? string.Empty,
                DurationInMinutes = dto.DurationInMinutes,
                DisplayOrder      = dto.DisplayOrder,
                IsPreview         = dto.IsPreview,
                CreatedAt         = DateTime.UtcNow
            };

            await _unitOfWork.LessonRepo.Create(lesson);
            await _unitOfWork.SaveChangesAsync();

            return Result<LessonResponseDto>.Success(MapToResponse(lesson));
        }

        public async Task<Result<LessonResponseDto>> UpdateAsync(int id, UpdateLessonDto dto, string instructorId)
        {
            var lesson = await _unitOfWork.LessonRepo.GetLessonWithSectionAndCourseAsync(id);
            if (lesson is null)
                return Result<LessonResponseDto>.Failure($"Lesson with ID {id} was not found.");

            if (lesson.Section?.Course?.InstructorId != instructorId)
                return Result<LessonResponseDto>.Failure("You are not authorized to update this lesson.");

            lesson.Title             = dto.Title.Trim();
            lesson.ContentType       = dto.ContentType;
            lesson.VideoUrl          = dto.VideoUrl?.Trim() ?? string.Empty;
            lesson.TextContent       = dto.TextContent?.Trim() ?? string.Empty;
            lesson.AttachmentUrl     = dto.AttachmentUrl?.Trim() ?? string.Empty;
            lesson.DurationInMinutes = dto.DurationInMinutes;
            lesson.DisplayOrder      = dto.DisplayOrder;
            lesson.IsPreview         = dto.IsPreview;

            await _unitOfWork.LessonRepo.Update(lesson);
            await _unitOfWork.SaveChangesAsync();

            return Result<LessonResponseDto>.Success(MapToResponse(lesson));
        }

        public async Task<Result> DeleteAsync(int id, string instructorId)
        {
            var lesson = await _unitOfWork.LessonRepo.GetLessonWithSectionAndCourseAsync(id);
            if (lesson is null)
                return Result.Failure($"Lesson with ID {id} was not found.");

            if (lesson.Section?.Course?.InstructorId != instructorId)
                return Result.Failure("You are not authorized to delete this lesson.");

            await _unitOfWork.LessonRepo.Delete(id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }



        private static LessonResponseDto MapToResponse(Lesson l) => new()
        {
            Id                = l.Id,
            SectionId         = l.SectionId,
            Title             = l.Title,
            ContentType       = l.ContentType,
            VideoUrl          = l.VideoUrl ?? string.Empty,
            TextContent       = l.TextContent ?? string.Empty,
            AttachmentUrl     = l.AttachmentUrl ?? string.Empty,
            DurationInMinutes = l.DurationInMinutes,
            DisplayOrder      = l.DisplayOrder,
            IsPreview         = l.IsPreview,
            CreatedAt         = l.CreatedAt
        };

        public async Task<Result> CompleteLessonAsync(int lessonId, string studentId)
        {
            // Get lesson with course hierarchy
            var lesson = await _unitOfWork.LessonRepo
                .GetLessonWithSectionAndCourseAsync(lessonId);

            if (lesson == null)
                return Result.Failure("Lesson not found.");

            // Check student enrollment
            var enrollment = await _unitOfWork.EnrollmentRepo
                .GetEnrollmentByStudentAndCourseAsync(studentId, lesson.Section.CourseId);

            if (enrollment == null)
                return Result.Failure("You are not enrolled in this course.");

            // Get existing progress
            var progress = await _unitOfWork.LessonProgressRepo
                .GetProgressAsync(enrollment.Id, lessonId);

            // check Progress and update last viewed time if already completed
            if (progress != null && progress.IsCompleted)
            {
                progress.LastViewedAt = DateTime.UtcNow;
                await _unitOfWork.LessonProgressRepo.Update(progress);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }

            // Create progress if it doesn't exist
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

                await _unitOfWork.LessonProgressRepo.Create(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
                progress.LastViewedAt = DateTime.UtcNow;

                await _unitOfWork.LessonProgressRepo.Update(progress);
            }

            // Update enrollment activity
            enrollment.LastAccessedAt = DateTime.UtcNow;

            // Get latest progress records
            var progresses = await _unitOfWork.LessonProgressRepo
                .GetProgressesForEnrollmentAsync(enrollment.Id);

            // Count completed lessons
            var completedLessons = progresses.Count(p => p.IsCompleted);

            // Count total lessons in the course
            var totalLessons = lesson.Section.Course.CourseSections
                .SelectMany(s => s.Lessons)
                .Count();

            // Update progress percentage
            enrollment.ProgressPercent = totalLessons == 0
                ? 0
                : Math.Round((completedLessons * 100m) / totalLessons, 2);

            // Update completion status
            if (completedLessons == 0)
            {
                enrollment.CompletionStatus = CompletionStatus.NotStarted;
            }
            else if (completedLessons == totalLessons)
            {
                enrollment.CompletionStatus = CompletionStatus.Completed;
            }
            else
            {
                enrollment.CompletionStatus = CompletionStatus.InProgress;
            }

            // Save enrollment changes
            await _unitOfWork.EnrollmentRepo.Update(enrollment);

            // Save all changes
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
