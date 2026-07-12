using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configurations
{
    public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
    {
        public void Configure(EntityTypeBuilder<LessonProgress> builder)
        {
            builder.HasKey(lp => lp.Id);

            builder.Property(lp => lp.LastViewedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Ensure unique combination of Enrollment and Lesson
            builder.HasIndex(lp => new { lp.EnrollmentId, lp.LessonId })
                   .IsUnique();

            // Relationships
            builder.HasOne(lp => lp.Enrollment)
                   .WithMany(e => e.LessonProgresses) // Assumes Enrollment has ICollection<LessonProgress>
                   .HasForeignKey(lp => lp.EnrollmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(lp => lp.Lesson)
                   .WithMany(l => l.LessonProgresses)
                   .HasForeignKey(lp => lp.LessonId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
