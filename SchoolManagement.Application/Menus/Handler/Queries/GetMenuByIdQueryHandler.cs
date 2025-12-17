using AutoMapper;
using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Menus.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Menus.Handler.Queries
{
    public class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, Result<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMenuByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<MenuDto>> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
        {
            // Fetch single menu by ID
            var menu = await _unitOfWork.MenuRepository.GetByIdAsync(request.Id, cancellationToken);

            if (menu == null)
            {
                return Result<MenuDto>.Failure("Menu not found", "The requested menu does not exist.");
            }

            // Map Menu entity to MenuDto
            var menuDto = _mapper.Map<MenuDto>(menu);

            return Result<MenuDto>.Success(menuDto, "Menu retrieved successfully");
        }
    }
}