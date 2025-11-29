using FluentValidation;
using SchoolManagement.Application.RolePermissions.Commands;
using SchoolManagement.Application.DTOs;
using System;

namespace SchoolManagement.Application.RolePermissions.Validators
{
    public class UpdateMenuPermissionCommandValidator : AbstractValidator<UpdateMenuPermissionCommand>
    {
        public UpdateMenuPermissionCommandValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Role ID cannot be empty.");

            RuleFor(x => x.MenuId)
                .NotEmpty().WithMessage("Menu ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Menu ID cannot be empty.");

            RuleFor(x => x.Permissions)
                .NotNull().WithMessage("Permissions cannot be null.")
                .SetValidator(new MenuPermissionsDtoValidator());
        }
    }

    public class MenuPermissionsDtoValidator : AbstractValidator<MenuPermissionsDto>
    {
        public MenuPermissionsDtoValidator()
        {
            // Business rule: At least one permission should be granted
            RuleFor(x => x)
                .Must(HaveAtLeastOnePermission)
                .WithMessage("At least one permission must be granted for the menu.");

            // Logical dependencies: CanView should be true if any other permission is granted
            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Add permission is granted.")
                .When(x => x.CanAdd);

            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Edit permission is granted.")
                .When(x => x.CanEdit);

            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Delete permission is granted.")
                .When(x => x.CanDelete);

            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Export permission is granted.")
                .When(x => x.CanExport);

            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Print permission is granted.")
                .When(x => x.CanPrint);

            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Approve permission is granted.")
                .When(x => x.CanApprove);

            RuleFor(x => x.CanView)
                .Equal(true)
                .WithMessage("View permission is required when Reject permission is granted.")
                .When(x => x.CanReject);
        }

        private bool HaveAtLeastOnePermission(MenuPermissionsDto permissions)
        {
            return permissions.CanView ||
                   permissions.CanAdd ||
                   permissions.CanEdit ||
                   permissions.CanDelete ||
                   permissions.CanExport ||
                   permissions.CanPrint ||
                   permissions.CanApprove ||
                   permissions.CanReject;
        }
    }
}