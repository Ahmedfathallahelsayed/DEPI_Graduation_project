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
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(o => o.OrderNumber)
                   .IsUnique();

            builder.Property(o => o.TotalAmount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(o => o.PaymentMethod)
                   .HasMaxLength(50);

            builder.Property(o => o.PaymentStatus)
                   .IsRequired();

            builder.Property(o => o.OrderedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne<AppUser>()
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.StudentId)
                   .OnDelete(DeleteBehavior.Restrict); // Resolves path conflicts
        }
    }
}
