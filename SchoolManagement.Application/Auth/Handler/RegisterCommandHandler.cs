using MediatR;
using SchoolManagement.Application.Auth.Commands;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Auth.Handler
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;

        public RegisterCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
        }

        public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _unitOfWork.AuthRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return Result<UserDto>.Failure("Email is already registered");

            var hashedPassword = _passwordService.HashPassword(request.Password);

            var user = new User(
                request.Username,
                request.Email,
                request.FirstName,
                request.LastName,
                hashedPassword,
                (UserType)request.Role);

            await _unitOfWork.AuthRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

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
