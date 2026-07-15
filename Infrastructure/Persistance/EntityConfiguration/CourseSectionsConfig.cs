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
    public class CourseSectionsConfig : IEntityTypeConfiguration<CourseSection>
    {
        public void Configure(EntityTypeBuilder<CourseSection> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.Title)
                   .IsRequired().HasMaxLength(255);

            builder.Property(cs => cs.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(cs => cs.Course)
                   .WithMany(c => c.CourseSections)
                   .HasForeignKey(cs => cs.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
