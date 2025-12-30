using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Teachers.Queries;
using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Teachers.Handlers.Queries
{
    public class GetTeacherByIdQueryHandler
        : IRequestHandler<GetTeacherByIdQuery, Result<TeacherDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTeacherByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TeacherDto>> Handle(
            GetTeacherByIdQuery request,
            CancellationToken cancellationToken)
        {
            var teacher = await _unitOfWork.TeachersRepository
                .GetTeacherWithDetailsAsync(request.TeacherId, cancellationToken);

            if (teacher == null)
            {
                return Result<TeacherDto>.Failure(
                    "TeacherNotFound",
                    $"Teacher with ID '{request.TeacherId}' not found"
                );
            }

            var teacherDto = new TeacherDto
            {
                Id = teacher.Id,
                FirstName = teacher.Name.FirstName,
                LastName = teacher.Name.LastName,
                FullName = teacher.FullName,
                Email = teacher.Email.Value,
                PhoneNumber = teacher.PhoneNumber.Value,
                EmployeeId = teacher.EmployeeId,
                DateOfJoining = teacher.DateOfJoining,
                DateOfLeaving = teacher.DateOfLeaving,
                Qualification = teacher.Qualification,
                Experience = teacher.Experience,
                TotalExperience = teacher.GetTotalYearsOfExperience(),
                DepartmentId = teacher.DepartmentId,
                DepartmentName = teacher.Department?.Name,
                IsActive = teacher.IsActive,
                Street = teacher.Address.Street,
                City = teacher.Address.City,
                State = teacher.Address.State,
                PostalCode = teacher.Address.PostalCode,
                Country = teacher.Address.Country,
                TotalTeachingAssignments = teacher.GetTotalTeachingAssignments(),
                TotalWeeklyPeriods = teacher.GetTotalWeeklyPeriods(),
                IsSenior = teacher.IsSeniorTeacher()
            };

            return Result<TeacherDto>.Success(teacherDto);
        }
    }
}
