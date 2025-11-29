using FluentValidation;
using SchoolManagement.Application.Permissions.Commands;

namespace SchoolManagement.Application.Permissions.Validators
{
    public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
    {
        public CreatePermissionCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Permission name is required.")
                .MaximumLength(100).WithMessage("Permission name cannot exceed 100 characters.")
                .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Permission name can only contain letters, numbers, dots, underscores, and hyphens.")
                .Must(BeInValidFormat).WithMessage("Permission name should follow the format: Module.Action.Resource (e.g., Users.Create.Student)");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required.")
                .MaximumLength(200).WithMessage("Display name cannot exceed 200 characters.");

            RuleFor(x => x.Module)
                .NotEmpty().WithMessage("Module is required.")
                .MaximumLength(50).WithMessage("Module name cannot exceed 50 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Module name can only contain letters, numbers, underscores, and hyphens.");

            RuleFor(x => x.Action)
                .NotEmpty().WithMessage("Action is required.")
                .MaximumLength(50).WithMessage("Action name cannot exceed 50 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Action name can only contain letters, numbers, underscores, and hyphens.");

            RuleFor(x => x.Resource)
                .NotEmpty().WithMessage("Resource is required.")
                .MaximumLength(50).WithMessage("Resource name cannot exceed 50 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Resource name can only contain letters, numbers, underscores, and hyphens.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // Business rule: Name should match Module.Action.Resource format
            RuleFor(x => x)
                .Must(HaveConsistentNaming)
                .WithMessage("Permission name must match the format: {Module}.{Action}.{Resource}")
                .When(x => !string.IsNullOrWhiteSpace(x.Name) &&
                           !string.IsNullOrWhiteSpace(x.Module) &&
                           !string.IsNullOrWhiteSpace(x.Action) &&
                           !string.IsNullOrWhiteSpace(x.Resource));
        }

        private bool BeInValidFormat(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Check if it follows Module.Action.Resource format
            var parts = name.Split('.');
            return parts.Length >= 2 && parts.Length <= 4;
        }

        private bool HaveConsistentNaming(CreatePermissionCommand command)
        {
            var expectedName = $"{command.Module}.{command.Action}.{command.Resource}";
            return command.Name.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
        }
    }
}