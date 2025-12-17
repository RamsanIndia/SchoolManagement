using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Shared.Correlation
{
    public class CorrelationIdAccessor : ICorrelationIdAccessor
    {
        public string? CorrelationId { get; set; }
    }
}
