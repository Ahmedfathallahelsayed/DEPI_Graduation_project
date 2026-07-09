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
    public class EnrollmentsConfig : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ProgressPercent)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0.00);

            builder.Property(e => e.CompletionStatus)
                   .IsRequired();

            builder.Property(e => e.EnrollmentDate)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.LastAccessedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne<AppUser>()
                   .WithMany(u => u.Enrollments)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Course)
                   .WithMany(c => c.Enrollments)
                   .HasForeignKey(e => e.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Order)
                   .WithOne(o => o.Enrollment)
                   .HasForeignKey<Enrollment>(e => e.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
