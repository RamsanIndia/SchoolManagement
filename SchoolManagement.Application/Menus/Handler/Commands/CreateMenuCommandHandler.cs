using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Menus.Commands;
using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Menus.Handler.Commands
{
    public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Result<MenuDto>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateMenuCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<MenuDto>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate parent menu exists if ParentMenuId is provided
                if (request.ParentMenuId.HasValue)
                {
                    var parentMenu = await _menuRepository.GetByIdAsync(request.ParentMenuId.Value, cancellationToken);
                    if (parentMenu == null)
                    {
                        return Result<MenuDto>.Failure("Parent menu not found", "The specified parent menu does not exist.");
                    }
                }

                // Check for duplicate menu name
                var existingMenu = await _menuRepository.GetByNameAsync(request.Name, cancellationToken);
                if (existingMenu != null)
                {
                    return Result<MenuDto>.Failure("Duplicate menu", $"A menu with the name '{request.Name}' already exists.");
                }

                // Create new menu
                var menu = new Menu(
                    request.Name,
                    request.DisplayName,
                    request.Route,
                    (MenuType)request.Type,
                    request.Icon,
                    request.Description,
                    request.ParentMenuId
                );

                menu.SetSortOrder(request.SortOrder);

                var createdMenu = await _menuRepository.CreateAsync(menu, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var menuDto = new MenuDto
                {
                    Id = createdMenu.Id,
                    Name = createdMenu.Name,
                    DisplayName = createdMenu.DisplayName,
                    Route = createdMenu.Route,
                    Type = createdMenu.Type.ToString(),
                    Icon = createdMenu.Icon,
                    Description = createdMenu.Description,
                    ParentMenuId = createdMenu.ParentMenuId,
                    SortOrder = createdMenu.SortOrder,
                    IsActive = createdMenu.IsActive
                };

                return Result<MenuDto>.Success(menuDto, "Menu created successfully");
            }
            catch (Exception ex)
            {
                return Result<MenuDto>.Failure("Menu creation failed", $"An error occurred while creating the menu: {ex.Message}");
            }
        }
    }
}