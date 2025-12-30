using FluentValidation;
using SchoolManagement.Application.Sections.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Validators
{
    public class MapSubjectCommandValidator : AbstractValidator<MapSubjectCommand>
    {
        public MapSubjectCommandValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty()
                .WithMessage("Section ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Section ID must be a valid GUID");

            RuleFor(x => x.SubjectId)
                .NotEmpty()
                .WithMessage("Subject ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Subject ID must be a valid GUID");

            RuleFor(x => x.SubjectName)
                .NotEmpty()
                .WithMessage("Subject name is required")
                .MaximumLength(100)
                .WithMessage("Subject name cannot exceed 100 characters");

            RuleFor(x => x.SubjectCode)
                .NotEmpty()
                .WithMessage("Subject code is required")
                .MaximumLength(20)
                .WithMessage("Subject code cannot exceed 20 characters");

            RuleFor(x => x.TeacherId)
                .NotEmpty()
                .WithMessage("Teacher ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Teacher ID must be a valid GUID");

            RuleFor(x => x.TeacherName)
                .NotEmpty()
                .WithMessage("Teacher name is required")
                .MaximumLength(200)
                .WithMessage("Teacher name cannot exceed 200 characters");

            RuleFor(x => x.WeeklyPeriods)
                .GreaterThan(0)
                .WithMessage("Weekly periods must be greater than 0")
                .LessThanOrEqualTo(20)
                .WithMessage("Weekly periods cannot exceed 20");
        }
    }
}
