using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class Order
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Enrollment Enrollment { get; set; }
        public virtual IEnumerable<OrderItem> OrderItems { get; set; }
    }
}
