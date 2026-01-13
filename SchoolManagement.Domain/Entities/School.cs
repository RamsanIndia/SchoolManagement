using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Domain.Entities
{
    public sealed class School : BaseEntity
    {
        // ✅ Core Value Objects
        public string Name { get; private set; }
        public string Code { get; private set; }

        // ✅ Essential School Properties
        public SchoolType Type { get; private set; }
        public string Address { get; private set; }
        public string ContactPhone { get; private set; }
        public string ContactEmail { get; private set; }
        public int MaxStudentCapacity { get; private set; }
        public SchoolStatus Status { get; private set; }
        public DateTime FoundedDate { get; private set; }

        // ✅ Navigation properties
        public List<User> Users { get; private set; } = new();
        public Tenant Tenant { get; private set; } = null!;

        // ✅ Aggregate Collections (using IDs for performance)
        private readonly List<Guid> _studentIds = new();
        private readonly List<Guid> _employeeIds = new();
        private readonly List<Guid> _classroomIds = new();
        private readonly List<Guid> _nonTeachingStaffIds = new();

        public IReadOnlyCollection<Guid> StudentIds => _studentIds.AsReadOnly();
        public IReadOnlyCollection<Guid> EmployeeIds => _employeeIds.AsReadOnly();
        public IReadOnlyCollection<Guid> ClassroomIds => _classroomIds.AsReadOnly();
        public IReadOnlyCollection<Guid> NonTeachingStaffIds => _nonTeachingStaffIds.AsReadOnly();

        // ✅ Read-only computed properties
        public int CurrentStudentCount => _studentIds.Count;
        public int CurrentEmployeeCount => _employeeIds.Count;
        public int CurrentNonTeachingStaffCount => _nonTeachingStaffIds.Count;
        public int TotalStaffCount => CurrentEmployeeCount + CurrentNonTeachingStaffCount;

        public decimal CapacityUtilization => MaxStudentCapacity > 0
            ? (decimal)CurrentStudentCount / MaxStudentCapacity * 100 : 0;

        public bool IsAtCapacity => CurrentStudentCount >= MaxStudentCapacity;
        public bool IsOperational => IsActive && !IsDeleted && Status == SchoolStatus.Active;

        // ✅ Private constructor for EF Core
        private School() : base() { }

        /// <summary>
        /// Factory method with comprehensive validation
        /// </summary>
        public static School Create(
            Guid tenantId,
            string name,
            string code,
            SchoolType type = SchoolType.Primary,
            string address = "",
            string contactPhone = "",
            string contactEmail = "",
            int maxStudentCapacity = 1000,
            DateTime? foundedDate = null,
            string createdBy = null,
            string createdIP = null)
        {
            // ✅ Validation
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId is required", nameof(tenantId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("School name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(code) || code.Length != 5)
                throw new ArgumentException("School code must be 5 characters", nameof(code));
            if (maxStudentCapacity < 50)
                throw new ArgumentException("Minimum capacity is 50 students", nameof(maxStudentCapacity));

            var school = new School
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SchoolId = null, // ✅ School entity doesn't have SchoolId (it IS the school)
                Name = name.Trim(),
                Code = code.ToUpperInvariant(),
                Type = type,
                Address = address?.Trim() ?? "",
                ContactPhone = contactPhone?.Trim() ?? "",
                ContactEmail = contactEmail?.Trim().ToLowerInvariant() ?? "",
                MaxStudentCapacity = maxStudentCapacity,
                FoundedDate = foundedDate ?? DateTime.UtcNow,
                Status = SchoolStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Set audit info
            school.SetCreated(createdBy ?? "System", createdIP ?? "Unknown");

            // ✅ Domain events
            //school.AddDomainEvent(new SchoolCreatedEvent(
            //    school.Id,
            //    school.TenantId,
            //    school.Name,
            //    school.Code,
            //    school.Type,
            //    school.MaxStudentCapacity));

            return school;
        }

        /// <summary>
        /// Update school details
        /// </summary>
        public void UpdateDetails(
            string name = null,
            string code = null,
            SchoolType? type = null,
            string address = null,
            string contactPhone = null,
            string contactEmail = null,
            int? maxStudentCapacity = null,
            string updatedBy = null,
            string updatedIP = null)
        {
            // ✅ Business rule validation
            if (Status == SchoolStatus.Closed)
                throw new InvalidOperationException("Cannot update closed school");

            if (!IsActive)
                throw new InvalidOperationException("Cannot update inactive school");

            // ✅ Update properties only if provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                if (code.Length != 5)
                    throw new ArgumentException("School code must be 5 characters", nameof(code));
                Code = code.ToUpperInvariant();
            }

            if (type.HasValue)
                Type = type.Value;

            if (address != null)
                Address = address.Trim();

            if (contactPhone != null)
                ContactPhone = contactPhone.Trim();

            if (contactEmail != null)
                ContactEmail = contactEmail.Trim().ToLowerInvariant();

            if (maxStudentCapacity.HasValue)
            {
                if (maxStudentCapacity < 50)
                    throw new ArgumentException("Minimum capacity is 50 students", nameof(maxStudentCapacity));

                if (maxStudentCapacity < CurrentStudentCount)
                    throw new ArgumentException(
                        $"Cannot reduce capacity to {maxStudentCapacity}. Current student count is {CurrentStudentCount}",
                        nameof(maxStudentCapacity));

                MaxStudentCapacity = maxStudentCapacity.Value;
            }

            // ✅ Set audit trail
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            // ✅ Domain event
            //AddDomainEvent(new SchoolUpdatedEvent(Id, TenantId, Name, Code));
        }

        /// <summary>
        /// Update contact information
        /// </summary>
        public void UpdateContactInfo(
            string contactPhone,
            string contactEmail,
            string address,
            string updatedBy = null,
            string updatedIP = null)
        {
            if (Status == SchoolStatus.Closed)
                throw new InvalidOperationException("Cannot update closed school");

            ContactPhone = contactPhone?.Trim() ?? ContactPhone;
            ContactEmail = contactEmail?.Trim().ToLowerInvariant() ?? ContactEmail;
            Address = address?.Trim() ?? Address;

            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");
        }

        /// <summary>
        /// Update capacity with validation
        /// </summary>
        public void UpdateCapacity(int newCapacity, string updatedBy = null, string updatedIP = null)
        {
            if (newCapacity < 50)
                throw new ArgumentException("Minimum capacity is 50 students", nameof(newCapacity));

            if (newCapacity < CurrentStudentCount)
                throw new InvalidOperationException(
                    $"Cannot reduce capacity to {newCapacity}. Current student count is {CurrentStudentCount}");

            MaxStudentCapacity = newCapacity;
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");
        }

        /// <summary>
        /// Enroll student (aggregate consistency)
        /// </summary>
        public void EnrollStudent(Guid studentId, string updatedBy = null, string updatedIP = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot enroll students in inactive school");

            if (Status != SchoolStatus.Active)
                throw new InvalidOperationException($"Cannot enroll students. School status is {Status}");

            if (IsAtCapacity)
                throw new InvalidOperationException(
                    $"School is at full capacity ({MaxStudentCapacity}). Cannot enroll more students.");

            if (_studentIds.Contains(studentId))
                throw new InvalidOperationException("Student is already enrolled in this school");

            _studentIds.Add(studentId);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new StudentEnrolledEvent(Id, TenantId, studentId));
        }

        /// <summary>
        /// Remove student from school
        /// </summary>
        public void RemoveStudent(Guid studentId, string updatedBy = null, string updatedIP = null)
        {
            if (!_studentIds.Contains(studentId))
                throw new InvalidOperationException("Student is not enrolled in this school");

            _studentIds.Remove(studentId);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new StudentRemovedEvent(Id, TenantId, studentId));
        }

        /// <summary>
        /// Hire employee
        /// </summary>
        public void HireEmployee(Guid employeeId, EmployeeType type, string updatedBy = null, string updatedIP = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot hire employees for inactive school");

            var targetList = type == EmployeeType.Teacher ? _employeeIds : _nonTeachingStaffIds;

            if (targetList.Contains(employeeId))
                throw new InvalidOperationException($"{type} is already hired in this school");

            targetList.Add(employeeId);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new EmployeeHiredEvent(Id, TenantId, employeeId, type));
        }

        /// <summary>
        /// Remove employee
        /// </summary>
        public void RemoveEmployee(Guid employeeId, EmployeeType type, string updatedBy = null, string updatedIP = null)
        {
            var targetList = type == EmployeeType.Teacher ? _employeeIds : _nonTeachingStaffIds;

            if (!targetList.Contains(employeeId))
                throw new InvalidOperationException($"{type} is not employed at this school");

            targetList.Remove(employeeId);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new EmployeeRemovedEvent(Id, TenantId, employeeId, type));
        }

        /// <summary>
        /// Add classroom
        /// </summary>
        public void AddClassroom(Guid classroomId, string updatedBy = null, string updatedIP = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot add classrooms to inactive school");

            if (_classroomIds.Contains(classroomId))
                throw new InvalidOperationException("Classroom already exists in this school");

            _classroomIds.Add(classroomId);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");
        }

        /// <summary>
        /// Remove classroom
        /// </summary>
        public void RemoveClassroom(Guid classroomId, string updatedBy = null, string updatedIP = null)
        {
            if (!_classroomIds.Contains(classroomId))
                throw new InvalidOperationException("Classroom does not exist in this school");

            _classroomIds.Remove(classroomId);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");
        }

        /// <summary>
        /// Change school status
        /// </summary>
        public void ChangeStatus(SchoolStatus newStatus, string reason = null, string updatedBy = null, string updatedIP = null)
        {
            var oldStatus = Status;
            Status = newStatus;

            // ✅ Auto-manage IsActive based on status
            switch (newStatus)
            {
                case SchoolStatus.Active:
                    if (!IsActive)
                        Activate(updatedBy);
                    break;
                case SchoolStatus.Inactive:
                case SchoolStatus.Suspended:
                case SchoolStatus.Closed:
                    if (IsActive)
                        Deactivate(updatedBy);
                    break;
            }

            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");
            //AddDomainEvent(new SchoolStatusChangedEvent(Id, TenantId, oldStatus, newStatus, reason));
        }

        /// <summary>
        /// Close school permanently
        /// </summary>
        public void Close(string reason, string updatedBy = null, string updatedIP = null)
        {
            if (CurrentStudentCount > 0)
                throw new InvalidOperationException(
                    $"Cannot close school with {CurrentStudentCount} enrolled students. Transfer students first.");

            Status = SchoolStatus.Closed;
            Deactivate(updatedBy);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new SchoolClosedEvent(Id, TenantId, reason));
        }

        /// <summary>
        /// Reopen closed school
        /// </summary>
        public void Reopen(string updatedBy = null, string updatedIP = null)
        {
            if (Status != SchoolStatus.Closed)
                throw new InvalidOperationException("Only closed schools can be reopened");

            Status = SchoolStatus.Active;
            Activate(updatedBy);
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new SchoolReopenedEvent(Id, TenantId));
        }

        /// <summary>
        /// Change school type
        /// </summary>
        public void ChangeType(SchoolType newType, string updatedBy = null, string updatedIP = null)
        {
            if (Status == SchoolStatus.Closed)
                throw new InvalidOperationException("Cannot change type of closed school");

            var oldType = Type;
            Type = newType;
            SetUpdated(updatedBy ?? "System", updatedIP ?? "Unknown");

            //AddDomainEvent(new SchoolTypeChangedEvent(Id, TenantId, oldType, newType));
        }

        // ✅ Domain invariants validation
        private void ValidateCapacity()
        {
            if (CurrentStudentCount > MaxStudentCapacity)
                throw new InvalidOperationException(
                    $"Invariant violation: Student count ({CurrentStudentCount}) exceeds capacity ({MaxStudentCapacity})");
        }

        /// <summary>
        /// Validate all business invariants
        /// </summary>
        public void ValidateInvariants()
        {
            ValidateCapacity();

            if (MaxStudentCapacity < 50)
                throw new InvalidOperationException("Invariant violation: Minimum capacity is 50 students");

            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Invariant violation: School name is required");

            if (string.IsNullOrWhiteSpace(Code) || Code.Length != 5)
                throw new InvalidOperationException("Invariant violation: School code must be 5 characters");
        }

        // ✅ Equality
        public override bool Equals(object obj) => obj is School other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}