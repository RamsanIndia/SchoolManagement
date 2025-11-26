using MediatR;
using SchoolManagement.Application.Auth.Queries;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserProfileQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.AuthRepository.GetByIdAsync(request.UserId);
            if (user == null)
                return Result<UserDto>.Failure("User not found");

            string roleName = Enum.IsDefined(typeof(UserType), user.UserType)
                ? ((UserType)user.UserType).ToString()
                : "Unknown";

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = new List<string> { roleName }
            };

            return Result<UserDto>.Success(userDto);
        }
    }
}
