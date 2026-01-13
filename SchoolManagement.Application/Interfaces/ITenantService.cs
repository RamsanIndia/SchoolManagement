using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Interfaces
{
    public interface ITenantService
    {
        // 🔐 Security boundary
        Guid TenantId { get; }

        // 🏫 Domain context (optional)
        Guid? SchoolId { get; }
        string? SchoolName { get; }

        bool IsTenantSet { get; }
        bool IsSchoolSet { get; }
    }
}
