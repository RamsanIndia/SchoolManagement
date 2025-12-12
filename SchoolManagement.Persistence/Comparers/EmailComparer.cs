using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Comparers
{
    public class EmailComparer : ValueComparer<Email>
    {
        public EmailComparer()
            : base(
                (left, right) => left.Value == right.Value,
                email => email.Value.GetHashCode(),
                email => new Email(email.Value))
        {
        }
    }
}
