using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class Certificate
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public string CertificateCode { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public string CertificateUrl { get; set; }

        // Navigation Properties
        public virtual Enrollment Enrollment { get; set; }
    }
}
