using AutoMapper;
using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Employees.Queries;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.Employees.Handler.Queries
{
    public class GetEmployeeByIdQueryHandler
        : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Get entity
            var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(request.Id);

            if (employee == null)
            {
                return Result<EmployeeDto>.Failure("Employee not found");
            }

            // 2. Map entity → DTO
            var dto = _mapper.Map<EmployeeDto>(employee);

            // 3. Return wrapped result
            return Result<EmployeeDto>.Success(dto);
        }
    }
}
