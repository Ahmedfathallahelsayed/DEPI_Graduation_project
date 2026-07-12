using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configurations
{
    public class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
    {
        public void Configure(EntityTypeBuilder<CourseSection> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(s => s.DisplayOrder)
                   .IsRequired();

            builder.Property(s => s.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relationship with Course
            builder.HasOne(s => s.Course)
                   .WithMany(c => c.CourseSections) // Assumes Course has ICollection<CourseSection> CourseSections
                   .HasForeignKey(s => s.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
