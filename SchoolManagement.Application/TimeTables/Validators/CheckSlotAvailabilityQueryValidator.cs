using FluentValidation;
using SchoolManagement.Application.TimeTables.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public class CheckSlotAvailabilityQueryValidator
        : AbstractValidator<CheckSlotAvailabilityQuery>
    {
        public CheckSlotAvailabilityQueryValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty()
                .WithMessage("Section ID is required");

            RuleFor(x => x.TeacherId)
                .NotEmpty()
                .WithMessage("Teacher ID is required");

            RuleFor(x => x.RoomNumber)
                .NotEmpty()
                .WithMessage("Room number is required")
                .MaximumLength(20)
                .WithMessage("Room number cannot exceed 20 characters");

            RuleFor(x => x.DayOfWeek)
                .IsInEnum()
                .WithMessage("Invalid day of week")
                .NotEqual(DayOfWeek.Sunday)
                .WithMessage("Cannot schedule classes on Sunday");

            RuleFor(x => x.PeriodNumber)
                .InclusiveBetween(1, 10)
                .WithMessage("Period number must be between 1 and 10");
        }
    }
}
