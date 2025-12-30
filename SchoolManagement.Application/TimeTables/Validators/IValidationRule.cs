using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public interface IValidationRule<in T>
    {
        Task ValidateAsync(T request, CancellationToken cancellationToken);
    }

}
