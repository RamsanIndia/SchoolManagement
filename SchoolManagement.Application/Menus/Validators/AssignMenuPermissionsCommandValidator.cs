using FluentValidation;
using SchoolManagement.Application.Menus.Commands;
using SchoolManagement.Application.DTOs;
using System;
using System.Linq;

namespace SchoolManagement.Application.Menus.Validators
{
    public class AssignMenuPermissionsCommandValidator : AbstractValidator<AssignMenuPermissionsCommand>
    {
        public AssignMenuPermissionsCommandValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Role ID cannot be empty.");

            RuleFor(x => x.MenuPermissions)
                .NotNull().WithMessage("Menu permissions cannot be null.")
                .NotEmpty().WithMessage("At least one menu permission must be provided.")
                .Must(HaveValidMenuIds).WithMessage("All menu IDs must be valid (non-empty GUIDs).")
                .When(x => x.MenuPermissions != null);

            RuleForEach(x => x.MenuPermissions)
                .ChildRules(permission =>
                {
                    permission.RuleFor(x => x.Key)
                        .NotEqual(Guid.Empty).WithMessage("Menu ID cannot be empty.");

                    permission.RuleFor(x => x.Value)
                        .NotNull().WithMessage("Menu permissions cannot be null.")
                        .SetValidator(new MenuPermissionsDtoValidator());
                })
                .When(x => x.MenuPermissions != null && x.MenuPermissions.Any());
        }

        private bool HaveValidMenuIds(Dictionary<Guid, MenuPermissionsDto> menuPermissions)
        {
            return menuPermissions.All(mp => mp.Key != Guid.Empty && mp.Value != null);
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

            // Optional: Add logical validation rules
            // Example: If CanEdit is true, CanView should also be true
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