using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class PendingNotification :BaseEntity
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }

        public void MarkAsSent()
        {
            IsSent = true;
            SentAt = DateTime.UtcNow;
        }
    }
}
