using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Shared.Correlation
{
    public interface ICorrelationIdAccessor
    {
        string? CorrelationId { get; set; }
    }
}
