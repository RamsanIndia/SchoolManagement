using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class BiometricDevice : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSyncTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public BiometricDevice()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            IsDeleted = false;
            IsOnline = false;
            LastSyncTime = DateTime.MinValue;
        }

        public void MarkAsDeleted(bool value)
        {
            IsDeleted = value;
        }
    }
}
