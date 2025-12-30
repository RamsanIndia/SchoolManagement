using FluentValidation;
using SchoolManagement.Application.Teachers.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Validators
{
    public class GetTeacherWorkloadQueryValidator : AbstractValidator<GetTeacherWorkloadQuery>
    {
        public GetTeacherWorkloadQueryValidator()
        {
            RuleFor(x => x.TeacherId)
                .NotEmpty()
                .WithMessage("Teacher ID is required");
        }
    }
}
