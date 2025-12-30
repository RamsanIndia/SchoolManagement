using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Domain.Entities
{
    /// <summary>
    /// Department aggregate root representing academic or administrative departments
    /// </summary>
    public class Department : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Code { get; private set; }
        public Guid? HeadOfDepartmentId { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual Teacher HeadOfDepartment { get; private set; }

        private readonly List<Teacher> _teachers = new();
        public virtual IReadOnlyCollection<Teacher> Teachers => _teachers.AsReadOnly();

        // EF Core constructor
        private Department()
        {
        }

        /// <summary>
        /// Factory method to create a new department
        /// </summary>
        public static Department Create(
            string name,
            string code,
            string description = null,
            string createdBy = null)
        {
            ValidateName(name);
            ValidateCode(code);

            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Code = code.Trim().ToUpper(),
                Description = description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            department.AddDomainEvent(new DepartmentCreatedEvent(
                department.Id,
                department.Name,
                department.Code
            ));

            return department;
        }

        /// <summary>
        /// Updates department details
        /// </summary>
        public void UpdateDetails(string name, string code, string description, string updatedBy)
        {
            ValidateName(name);
            ValidateCode(code);

            var previousName = Name;
            var previousCode = Code;

            Name = name.Trim();
            Code = code.Trim().ToUpper();
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new DepartmentUpdatedEvent(
                Id,
                previousName,
                Name,
                previousCode,
                Code
            ));
        }

        /// <summary>
        /// Assigns a head of department
        /// </summary>
        public void AssignHeadOfDepartment(Guid teacherId, string assignedBy)
        {
            if (teacherId == Guid.Empty)
                throw new DomainException("Teacher ID cannot be empty.");

            if (!IsActive)
                throw new DepartmentException("Cannot assign head to an inactive department.");

            var previousHeadId = HeadOfDepartmentId;
            HeadOfDepartmentId = teacherId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = assignedBy;

            AddDomainEvent(new HeadOfDepartmentAssignedEvent(
                Id,
                Name,
                previousHeadId,
                teacherId
            ));
        }

        /// <summary>
        /// Removes the head of department
        /// </summary>
        public void RemoveHeadOfDepartment(string removedBy)
        {
            if (!HeadOfDepartmentId.HasValue)
                throw new DomainException("No head of department is currently assigned.");

            var previousHeadId = HeadOfDepartmentId;
            HeadOfDepartmentId = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = removedBy;

            AddDomainEvent(new HeadOfDepartmentRemovedEvent(
                Id,
                Name,
                previousHeadId.Value
            ));
        }

        /// <summary>
        /// Changes the current head of department
        /// </summary>
        public void ChangeHeadOfDepartment(Guid newTeacherId, string changedBy)
        {
            if (newTeacherId == Guid.Empty)
                throw new DomainException("New teacher ID cannot be empty.");

            if (!HeadOfDepartmentId.HasValue)
                throw new DomainException(
                    "No head of department is currently assigned. Use AssignHeadOfDepartment instead.");

            if (HeadOfDepartmentId == newTeacherId)
                throw new DomainException("The specified teacher is already the head of department.");

            var previousHeadId = HeadOfDepartmentId;
            HeadOfDepartmentId = newTeacherId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = changedBy;

            AddDomainEvent(new HeadOfDepartmentChangedEvent(
                Id,
                Name,
                previousHeadId.Value,
                newTeacherId
            ));
        }

        /// <summary>
        /// Activates the department
        /// </summary>
        public void Activate(string activatedBy)
        {
            if (IsActive)
                throw new DepartmentAlreadyActiveException($"Department '{Name}' is already active.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;

            AddDomainEvent(new DepartmentActivatedEvent(Id, Name));
        }

        /// <summary>
        /// Deactivates the department
        /// </summary>
        public void Deactivate(string deactivatedBy)
        {
            if (!IsActive)
                throw new DepartmentAlreadyInactiveException($"Department '{Name}' is already inactive.");

            // Business rule: Cannot deactivate if has active teachers
            if (HasActiveTeachers())
                throw new DepartmentHasActiveTeachersException(
                    $"Cannot deactivate department '{Name}' because it has {GetActiveTeacherCount()} active teacher(s). " +
                    $"Please reassign or deactivate all teachers first.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;

            AddDomainEvent(new DepartmentDeactivatedEvent(Id, Name));
        }

        // ========== Query Methods ==========

        /// <summary>
        /// Checks if department has a head assigned
        /// </summary>
        public bool HasHeadOfDepartment()
        {
            return HeadOfDepartmentId.HasValue && HeadOfDepartmentId.Value != Guid.Empty;
        }

        /// <summary>
        /// Checks if department has any active teachers
        /// </summary>
        public bool HasActiveTeachers()
        {
            return _teachers.Any(t => t.IsActive);
        }

        /// <summary>
        /// Gets count of active teachers in this department
        /// </summary>
        public int GetActiveTeacherCount()
        {
            return _teachers.Count(t => t.IsActive);
        }

        /// <summary>
        /// Gets count of all teachers in this department
        /// </summary>
        public int GetTotalTeacherCount()
        {
            return _teachers.Count;
        }

        /// <summary>
        /// Checks if a specific teacher is assigned to this department
        /// </summary>
        public bool HasTeacher(Guid teacherId)
        {
            return _teachers.Any(t => t.Id == teacherId);
        }

        /// <summary>
        /// Checks if department can be deleted
        /// </summary>
        public bool CanBeDeleted()
        {
            return !HasActiveTeachers() && !IsActive;
        }

        // ========== Validation Methods ==========

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name is required.", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Department name cannot exceed 100 characters.", nameof(name));
        }

        private static void ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Department code is required.", nameof(code));

            if (code.Length > 20)
                throw new ArgumentException("Department code cannot exceed 20 characters.", nameof(code));

            // Optional: Add regex validation for code format
            // Example: Only alphanumeric characters
            if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z0-9]+$"))
                throw new ArgumentException(
                    "Department code must contain only uppercase letters and numbers.",
                    nameof(code));
        }
    }
}
