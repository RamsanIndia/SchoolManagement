using FluentValidation;
using SchoolManagement.Application.Menus.Commands;
using SchoolManagement.Domain.Enums;
using System;

namespace SchoolManagement.Application.Menus.Validators
{
    public class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
    {
        public CreateMenuCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Menu name is required.")
                .MaximumLength(100).WithMessage("Menu name cannot exceed 100 characters.")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Menu name can only contain letters, numbers, underscores, and hyphens.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required.")
                .MaximumLength(200).WithMessage("Display name cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon name cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Icon));

            RuleFor(x => x.Route)
                .MaximumLength(500).WithMessage("Route cannot exceed 500 characters.")
                .Must(BeValidRoute).WithMessage("Route must start with '/' or be empty.")
                .When(x => !string.IsNullOrEmpty(x.Route));

            RuleFor(x => x.Component)
                .MaximumLength(200).WithMessage("Component name cannot exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Component));

            RuleFor(x => x.Type)
                .Must(BeValidMenuType).WithMessage("Invalid menu type specified.")
                .WithName("MenuType");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be a positive number.")
                .LessThanOrEqualTo(9999).WithMessage("Sort order cannot exceed 9999.");

            RuleFor(x => x.ParentMenuId)
                .NotEqual(Guid.Empty).WithMessage("Parent menu ID cannot be empty.")
                .When(x => x.ParentMenuId.HasValue);

            // Business rule: Ensure route or component is provided based on menu type
            RuleFor(x => x)
                .Must(HaveValidRouteOrComponent).WithMessage("Either Route or Component must be provided for navigation menus.")
                .When(x => x.Type == 1); // Assuming Type 1 is navigation menu
        }

        private bool BeValidRoute(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                return true;

            return route.StartsWith("/") && !route.Contains("//");
        }

        private bool BeValidMenuType(int type)
        {
            // Assuming MenuType enum values: 0 = Group, 1 = Menu, 2 = Button
            return Enum.IsDefined(typeof(MenuType), type);
        }

        private bool HaveValidRouteOrComponent(CreateMenuCommand command)
        {
            return !string.IsNullOrWhiteSpace(command.Route) ||
                   !string.IsNullOrWhiteSpace(command.Component);
        }
    }
}