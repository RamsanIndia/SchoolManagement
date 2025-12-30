using MediatR;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Commands
{
    public class DeleteTimeTableEntryCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public string Reason { get; set; }

        public DeleteTimeTableEntryCommand()
        {
        }

        public DeleteTimeTableEntryCommand(Guid id, string reason = null)
        {
            Id = id;
            Reason = reason;
        }
    }
}
