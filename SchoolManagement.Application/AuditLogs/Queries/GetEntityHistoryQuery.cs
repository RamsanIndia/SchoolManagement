using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.AuditLogs.Queries
{
    public class GetEntityHistoryQuery : IRequest<Result<List<AuditLogDto>>>
    {
        public string EntityName { get; set; }
        public string EntityId { get; set; }
    }
}
