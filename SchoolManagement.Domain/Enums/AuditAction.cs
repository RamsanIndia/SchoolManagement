using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Enums
{
    public enum AuditAction
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Read = 4,
        Login = 5,
        Logout = 6,
        Export = 7,
        Import = 8,
        BulkUpdate = 9,
        BulkDelete = 10
    }
}
