using FluentValidation;
using SchoolManagement.Application.TimeTables.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public class GenerateTimeTableCommandValidator : AbstractValidator<GenerateTimeTableCommand>
    {
        public GenerateTimeTableCommandValidator()
        {
            RuleFor(x => x.SectionId)
                .NotEmpty()
                .WithMessage("Section ID is required");

            RuleFor(x => x.PeriodsPerDay)
                .InclusiveBetween(1, 10)
                .WithMessage("Periods per day must be between 1 and 10");

            RuleFor(x => x.PeriodDuration)
                .InclusiveBetween(30, 120)
                .WithMessage("Period duration must be between 30 and 120 minutes");

            RuleFor(x => x.BreakAfterPeriod)
                .GreaterThan(0)
                .WithMessage("Break period must be greater than 0")
                .LessThanOrEqualTo(x => x.PeriodsPerDay)
                .WithMessage("Break period must be within the total periods per day");

            RuleFor(x => x.BreakDuration)
                .InclusiveBetween(15, 60)
                .WithMessage("Break duration must be between 15 and 60 minutes");

            RuleFor(x => x.SchoolStartTime)
                .Must(BeValidSchoolStartTime)
                .WithMessage("School start time must be between 6:00 AM and 10:00 AM");

            RuleFor(x => x.WorkingDays)
                .NotNull()
                .WithMessage("Working days cannot be null")
                .Must(days => days != null && days.Any())
                .WithMessage("At least one working day must be specified")
                .Must(days => days != null && days.All(d => d != DayOfWeek.Sunday))
                .WithMessage("Sunday cannot be a working day")
                .Must(days => days != null && days.Distinct().Count() == days.Length)
                .WithMessage("Working days cannot contain duplicates");

            RuleFor(x => x)
                .Must(ValidateTotalSchoolHours)
                .WithMessage("Total school hours exceed reasonable limits (max 12 hours per day)");
        }

        private bool BeValidSchoolStartTime(TimeSpan startTime)
        {
            return startTime >= TimeSpan.FromHours(6) && startTime <= TimeSpan.FromHours(10);
        }

        private bool ValidateTotalSchoolHours(GenerateTimeTableCommand command)
        {
            var totalMinutes = (command.PeriodsPerDay * command.PeriodDuration) + command.BreakDuration;
            var totalHours = totalMinutes / 60.0;
            return totalHours <= 12;
        }
    }
}
