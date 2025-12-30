using FluentValidation;
using SchoolManagement.Application.TimeTables.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public class GetTeacherTimeTableQueryValidator : AbstractValidator<GetTeacherTimeTableQuery>
    {
        public GetTeacherTimeTableQueryValidator()
        {
            RuleFor(x => x.TeacherId)
                .NotEmpty()
                .WithMessage("Teacher ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Teacher ID cannot be empty");
        }
    }
}
