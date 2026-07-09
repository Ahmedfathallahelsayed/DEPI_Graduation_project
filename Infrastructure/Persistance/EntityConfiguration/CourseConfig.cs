using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class CourseConfig : IEntityTypeConfiguration<Course>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(c => c.ShortDescription)
                   .HasMaxLength(500);

            builder.Property(c => c.Description)
                   .IsRequired();

            builder.Property(c => c.ThumbnailUrl)
                   .HasMaxLength(2048);

            builder.Property(c => c.Price)
                   .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Language)
                   .HasMaxLength(50);

            builder.Property(c => c.Level)
                   .IsRequired();

            builder.Property(c => c.Status)
                   .IsRequired();

            builder.Property(c => c.IsApproved)
                   .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne<AppUser>()
                   .WithMany(u => u.AuthoredCourses)
                   .HasForeignKey(c => c.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Category)
                   .WithMany(cat => cat.Courses)
                   .HasForeignKey(c => c.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
