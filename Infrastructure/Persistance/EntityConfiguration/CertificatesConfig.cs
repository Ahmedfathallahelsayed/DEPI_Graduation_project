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
    public class CertificatesConfig : IEntityTypeConfiguration<Certificate>
    {
        public void Configure(EntityTypeBuilder<Certificate> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CertificateCode)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(c => c.CertificateCode)
                   .IsUnique();

            builder.Property(c => c.CertificateUrl)
                   .IsRequired()
                   .HasMaxLength(2048);

            builder.Property(c => c.IssuedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(c => c.Enrollment)
                   .WithMany(e => e.Certificates)
                   .HasForeignKey(c => c.EnrollmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
