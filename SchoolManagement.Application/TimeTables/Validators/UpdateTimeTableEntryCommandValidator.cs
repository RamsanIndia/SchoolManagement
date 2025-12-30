using FluentValidation;
using SchoolManagement.Application.TimeTables.Commands;
using System;

namespace SchoolManagement.Application.TimeTables.Validators
{
    public class UpdateTimeTableEntryCommandValidator : AbstractValidator<UpdateTimeTableEntryCommand>
    {
        public UpdateTimeTableEntryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("TimeTable Entry ID is required");

            RuleFor(x => x.SubjectId)
                .NotEmpty()
                .WithMessage("Subject ID is required");

            RuleFor(x => x.TeacherId)
                .NotEmpty()
                .WithMessage("Teacher ID is required");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Start time is required")
                .Must(BeValidTime)
                .WithMessage("Start time must be between 00:00 and 23:59");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("End time is required")
                .Must(BeValidTime)
                .WithMessage("End time must be between 00:00 and 24:00");

            RuleFor(x => x)
                .Must(x => x.StartTime < x.EndTime)
                .WithMessage("Start time must be before end time")
                .When(x => x.StartTime != default && x.EndTime != default);

            RuleFor(x => x)
                .Must(x => (x.EndTime - x.StartTime).TotalMinutes >= 30)
                .WithMessage("Period duration must be at least 30 minutes")
                .When(x => x.StartTime < x.EndTime);

            RuleFor(x => x.RoomNumber)
                .NotEmpty()
                .WithMessage("Room number is required")
                .MaximumLength(20)
                .WithMessage("Room number cannot exceed 20 characters");
        }

        private bool BeValidTime(TimeSpan time)
        {
            return time >= TimeSpan.Zero && time <= TimeSpan.FromHours(24);
        }
    }
}
