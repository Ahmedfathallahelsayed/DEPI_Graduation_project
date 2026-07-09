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
    internal class LesssonConfig : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Title)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(l => l.VideoUrl)
                   .HasMaxLength(2048);

            builder.Property(l => l.AttachmentUrl)
                   .HasMaxLength(2048);

            builder.Property(l => l.IsPreview)
                   .HasDefaultValue(false);

            builder.Property(l => l.ContentType)
                   .IsRequired();

            builder.Property(l => l.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(l => l.Section)
                   .WithMany(s => s.Lessons)
                   .HasForeignKey(l => l.SectionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
