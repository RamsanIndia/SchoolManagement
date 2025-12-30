using FluentValidation;
using SchoolManagement.Application.SectionSubjects.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.SectionSubjects.Validators
{
    public class BulkMapSubjectsCommandValidator : AbstractValidator<BulkMapSubjectsCommand>
    {
        public BulkMapSubjectsCommandValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty()
                .WithMessage("Section ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Section ID must be a valid GUID");

            RuleFor(x => x.SubjectMappings)
                .NotEmpty()
                .WithMessage("At least one subject mapping is required")
                .Must(x => x.Count <= 50)
                .WithMessage("Cannot map more than 50 subjects at once");

            RuleForEach(x => x.SubjectMappings)
                .ChildRules(mapping =>
                {
                    mapping.RuleFor(m => m.SubjectId)
                        .NotEmpty()
                        .WithMessage("Subject ID is required");

                    mapping.RuleFor(m => m.SubjectName)
                        .NotEmpty()
                        .WithMessage("Subject name is required")
                        .MaximumLength(100)
                        .WithMessage("Subject name cannot exceed 100 characters");

                    mapping.RuleFor(m => m.SubjectCode)
                        .NotEmpty()
                        .WithMessage("Subject code is required")
                        .MaximumLength(20)
                        .WithMessage("Subject code cannot exceed 20 characters");

                    mapping.RuleFor(m => m.TeacherId)
                        .NotEmpty()
                        .WithMessage("Teacher ID is required");

                    mapping.RuleFor(m => m.TeacherName)
                        .NotEmpty()
                        .WithMessage("Teacher name is required");

                    mapping.RuleFor(m => m.WeeklyPeriods)
                        .GreaterThan(0)
                        .WithMessage("Weekly periods must be greater than 0")
                        .LessThanOrEqualTo(20)
                        .WithMessage("Weekly periods cannot exceed 20");
                });
        }
    }
}
