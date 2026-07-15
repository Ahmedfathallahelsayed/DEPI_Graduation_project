using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class LessonProgressConfig : IEntityTypeConfiguration<LessonProgress>
    {
        public void Configure(EntityTypeBuilder<LessonProgress> builder)
        {
            builder.HasKey(lp => lp.Id);

            builder.Property(lp => lp.IsCompleted)
                   .HasDefaultValue(false);
            builder.Property(lp => lp.LastViewedAt)
                   .HasDefaultValueSql("GETUTCDATE()");


            // Relationship
            builder.HasOne(lp => lp.Enrollment)
                   .WithMany(e => e.LessonProgresses)
                   .HasForeignKey(lp => lp.EnrollmentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(lp => lp.Lesson)
                   .WithMany(l => l.LessonProgresses)
                   .HasForeignKey(lp => lp.LessonId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
