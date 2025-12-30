using MediatR;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Domain.Common;
using System;

namespace SchoolManagement.Application.Classes.Commands
{
    /// <summary>
    /// Command to update an existing class
    /// </summary>
    public class UpdateClassCommand : IRequest<Result<ClassDto>>
    {
        public Guid Id { get; set; }
        public string ClassName { get; set; }
        public string ClassCode { get; set; }
        public int Grade { get; set; }
        public string Description { get; set; }
    }
}
