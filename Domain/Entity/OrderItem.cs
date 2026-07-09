using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int CourseId { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }
        public virtual Course Course { get; set; }
    }
}
