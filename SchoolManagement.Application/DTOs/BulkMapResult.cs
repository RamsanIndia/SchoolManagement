using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class BulkMapResult
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int WarningCount { get; set; }

        public List<SuccessfulMapping> SuccessfulMappings { get; set; } = new();
        public List<BulkMapError> Errors { get; set; } = new();
        public List<BulkMapWarning> Warnings { get; set; } = new();

        public bool HasErrors => FailureCount > 0;
        public bool IsPartialSuccess => SuccessCount > 0 && FailureCount > 0;
        public bool IsCompleteSuccess => SuccessCount > 0 && FailureCount == 0;
    }
}
