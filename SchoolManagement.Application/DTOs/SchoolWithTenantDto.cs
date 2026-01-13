using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class SchoolWithTenantDto
    {
        public Guid SchoolId { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }
        public Guid TenantId { get; set; }
        public string TenantCode { get; set; }
        public string TenantName { get; set; }
    }
}
