using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Menus.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Menus.Handler.Queries
{
    public class GetMenusByRoleQueryHandler : IRequestHandler<GetMenusByRoleQuery, Result<List<MenuDto>>>
    {
        private readonly IMenuRepository _menuRepository;

        public GetMenusByRoleQueryHandler(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<Result<List<MenuDto>>> Handle(GetMenusByRoleQuery request, CancellationToken cancellationToken)
        {
            var menus = await _menuRepository.GetMenusByRoleAsync(request.RoleId);

            if (menus == null || !menus.Any())
                return Result<List<MenuDto>>.Failure("No menus found for this role.");

            var list = menus.Select(m => new MenuDto
            {
                Id = m.Id,
                Name = m.Name,
                DisplayName = m.DisplayName,
                Description = m.Description,
                Icon = m.Icon,
                Route = m.Route,
                Component = m.Component,
                Type = m.Type.ToString(),
                SortOrder = m.SortOrder,
                IsActive = m.IsActive,
                IsVisible = m.IsVisible,
                ParentMenuId = m.ParentMenuId,
                ParentMenuName = m.ParentMenu?.DisplayName
            }).ToList();

            return Result<List<MenuDto>>.Success(list, "Menus fetched successfully.");
        }
    }
}
