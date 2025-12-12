using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Persistence.Converters
{
    public class EmailConverter : ValueConverter<Email, string>
    {
        public EmailConverter()
            : base(
                email => email.Value,
                value => new Email(value))
        {
        }
    }
}
