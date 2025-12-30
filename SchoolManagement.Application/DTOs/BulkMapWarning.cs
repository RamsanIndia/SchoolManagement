using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class BulkMapWarning
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string WarningMessage { get; set; }
    }
}
