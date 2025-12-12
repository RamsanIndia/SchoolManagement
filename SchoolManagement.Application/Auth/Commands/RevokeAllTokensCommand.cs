using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Commands
{
    public class RevokeAllTokensCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string RevokedByIp { get; set; } // Add this property to fix CS1061
    }
}
