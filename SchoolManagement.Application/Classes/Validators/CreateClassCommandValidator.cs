using FluentValidation;
using SchoolManagement.Application.Classes.Commands;
using SchoolManagement.Application.Interfaces;

namespace SchoolManagement.Application.Classes.Validators
{
    public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
    {
        private readonly IClassRepository _classRepository;
        private readonly IAcademicYearRepository _academicYearRepository;

        // ✅ FIX: Inject both repositories
        public CreateClassCommandValidator(
            IClassRepository classRepository,
            IAcademicYearRepository academicYearRepository)
        {
            _classRepository = classRepository ?? throw new ArgumentNullException(nameof(classRepository));
            _academicYearRepository = academicYearRepository ?? throw new ArgumentNullException(nameof(academicYearRepository));

            // Basic input validation
            RuleFor(x => x.ClassName)
                .NotEmpty().WithMessage("Class name is required.")
                .MaximumLength(100).WithMessage("Class name cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s\-]+$")
                .WithMessage("Class name can only contain letters, numbers, spaces, and hyphens.");

            RuleFor(x => x.ClassCode)
                .NotEmpty().WithMessage("Class code is required.")
                .MaximumLength(20).WithMessage("Class code cannot exceed 20 characters.")
                .Matches(@"^[A-Z0-9\-]+$")
                .WithMessage("Class code must contain only uppercase letters, numbers, and hyphens.")
                .MustAsync(BeUniqueClassCode)
                .WithMessage("Class code '{PropertyValue}' already exists.");

            RuleFor(x => x.Grade)
                .InclusiveBetween(1, 12)
                .WithMessage("Grade must be between 1 and 12.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.AcademicYearId)
                .NotEmpty().WithMessage("Academic year is required.")
                .MustAsync(AcademicYearExists)
                .WithMessage("Academic year does not exist.");
        }

        private async Task<bool> BeUniqueClassCode(
            CreateClassCommand command,
            string classCode,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(classCode))
                return true;

            var exists = await _classRepository.IsClassCodeExistsAsync(classCode, cancellationToken);
            return !exists;
        }

        private async Task<bool> AcademicYearExists(
            CreateClassCommand command,
            Guid academicYearId,
            CancellationToken cancellationToken)
        {
            if (academicYearId == Guid.Empty)
                return false;

            // ✅ Now this won't be null
            var exists = await _academicYearRepository.ExistsAsync(academicYearId, cancellationToken);
            return exists;
        }
    }
}
