using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Domain.Entities
{
    /// <summary>
    /// Class aggregate root - DDD & SOLID compliant
    /// Represents a class/grade level within the school
    /// 
    /// DDD Compliance:
    /// - ✅ Aggregate Root with encapsulation
    /// - ✅ Factory method for controlled creation
    /// - ✅ Read-only collection exposure
    /// - ✅ Complete domain event publishing
    /// - ✅ Business rule enforcement
    /// - ✅ Comprehensive validation
    /// 
    /// SOLID Compliance:
    /// - ✅ Single Responsibility
    /// - ✅ Open/Closed (read-only collections)
    /// - ✅ Liskov Substitution
    /// - ✅ Interface Segregation
    /// - ✅ Dependency Inversion
    /// </summary>
    public class Class : BaseEntity
    {
        #region Properties

        public int Grade { get; private set; }
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
        public int Capacity { get; private set; }
        public bool IsActive { get; private set; }
        public Guid AcademicYearId { get; private set; }

        // Navigation Properties
        public virtual AcademicYear AcademicYear { get; private set; }

        // ✅ Read-only collections (DDD: Prevents external modification)
        private readonly List<Student> _students = new();
        public virtual IReadOnlyCollection<Student> Students => _students.AsReadOnly();

        private readonly List<Section> _sections = new();
        public virtual IReadOnlyCollection<Section> Sections => _sections.AsReadOnly();

        #endregion

        #region Constructors

        // EF Core constructor
        private Class() { }

        // Private constructor - enforce factory method
        private Class(
            string className,
            string classCode,
            int grade,
            string description,
            Guid academicYearId)
        {
            Id = Guid.NewGuid();
            Name = className.Trim();
            Code = classCode.Trim().ToUpper();
            Grade = grade;
            Description = description?.Trim() ?? string.Empty;
            AcademicYearId = academicYearId;
            IsActive = true;
            Capacity = 0;
            CreatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Factory method to create a new Class aggregate
        /// Enforces all invariants and raises domain event
        /// </summary>
        /// <exception cref="ArgumentException">When input validation fails</exception>
        public static Class Create(
            string className,
            string classCode,
            int grade,
            string description,
            Guid academicYearId,
            string createdBy = null)
        {
            // ✅ FIXED: Comprehensive validation
            ValidateInputs(className, classCode, grade, academicYearId);

            var classEntity = new Class(className, classCode, grade, description, academicYearId)
            {
                CreatedBy = createdBy ?? "System"
            };

            // ✅ FIXED: Event with complete audit data
            classEntity.AddDomainEvent(new ClassCreatedEvent(
                classEntity.Id,
                classEntity.Code,
                classEntity.Name,
                classEntity.Grade,
                academicYearId,
                classEntity.CreatedAt,
                classEntity.CreatedBy));

            return classEntity;
        }

        #endregion

        #region Domain Methods

        /// <summary>
        /// Updates class details with validation and audit trail
        /// Raises ClassUpdatedEvent with before/after values
        /// </summary>
        public void UpdateDetails(
            string className,
            string classCode,
            int grade,
            string description,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("UpdatedBy is required.", nameof(updatedBy));

            // ✅ FIXED: Validate inputs
            ValidateInputs(className, classCode, grade, AcademicYearId);

            // Check for changes
            var hasChanges = Name != className.Trim()
                || Code != classCode.Trim().ToUpper()
                || Grade != grade
                || Description != (description?.Trim() ?? string.Empty);

            if (!hasChanges)
                return;

            // ✅ FIXED: Capture previous values for audit
            var previousCode = Code;
            var previousName = Name;
            var previousGrade = Grade;

            // Update state
            Name = className.Trim();
            Code = classCode.Trim().ToUpper();
            Grade = grade;
            Description = description?.Trim() ?? string.Empty;

            // ✅ FIXED: Add audit trail
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            // ✅ FIXED: Raise event with before/after values
            AddDomainEvent(new ClassUpdatedEvent(
                Id,
                previousCode,
                Code,
                previousName,
                Name,
                previousGrade,
                Grade,
                (DateTime)UpdatedAt,
                updatedBy));
        }

        /// <summary>
        /// Updates class capacity with business rule validation
        /// Business Invariant: Capacity >= Students.Count
        /// </summary>
        public void UpdateCapacity(int capacity, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("UpdatedBy is required.", nameof(updatedBy));

            if (capacity < 0)
                throw new DomainException("Class capacity cannot be negative.");

            // ✅ Business invariant: Cannot reduce capacity below enrollment
            var currentEnrollment = _students.Count;
            if (capacity > 0 && capacity < currentEnrollment)
                throw new DomainException(
                    $"Cannot set capacity to {capacity}. Current enrollment is {currentEnrollment}.");

            if (Capacity == capacity)
                return;

            var previousCapacity = Capacity;
            Capacity = capacity;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new ClassCapacityUpdatedEvent(
                Id,
                Code,
                previousCapacity,
                capacity,
                (DateTime)UpdatedAt,
                updatedBy));
        }

        /// <summary>
        /// Activates the class
        /// </summary>
        public void Activate(string activatedBy)
        {
            if (string.IsNullOrWhiteSpace(activatedBy))
                throw new ArgumentException("ActivatedBy is required.", nameof(activatedBy));

            if (IsActive)
                throw new ClassAlreadyActiveException($"Class {Code} is already active.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;

            AddDomainEvent(new ClassActivatedEvent(
                Id,
                Code,
                Name,
                (DateTime)UpdatedAt,
                activatedBy));
        }

        /// <summary>
        /// Deactivates class with business rule enforcement
        /// Cannot deactivate if:
        /// - Has active sections
        /// - Has active students
        /// </summary>
        public void Deactivate(string deactivatedBy)
        {
            if (string.IsNullOrWhiteSpace(deactivatedBy))
                throw new ArgumentException("DeactivatedBy is required.", nameof(deactivatedBy));

            if (!IsActive)
                throw new ClassAlreadyInactiveException($"Class {Code} is already inactive.");

            // ✅ FIXED: Business rule validation
            var activeSections = _sections.Count(s => s.IsActive);
            if (activeSections > 0)
                throw new DomainException(
                    $"Cannot deactivate class. It has {activeSections} active section(s). " +
                    $"Please deactivate sections first.");

            var activeStudents = _students.Count(s => s.Status == StudentStatus.Active);
            if (activeStudents > 0)
                throw new DomainException(
                    $"Cannot deactivate class. It has {activeStudents} active student(s). " +
                    $"Please transfer or deactivate students first.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;

            AddDomainEvent(new ClassDeactivatedEvent(
                Id,
                Code,
                Name,
                (DateTime)UpdatedAt,
                deactivatedBy));
        }

        #endregion

        #region Query Methods (Safe State Access)

        /// <summary>
        /// Gets current enrollment count
        /// </summary>
        public int GetEnrollmentCount() => _students.Count;

        /// <summary>
        /// Gets available seats (int.MaxValue if unlimited)
        /// </summary>
        public int GetAvailableSeats()
        {
            if (Capacity == 0)
                return int.MaxValue;
            return Math.Max(0, Capacity - _students.Count);
        }

        /// <summary>
        /// Checks if class has available seats
        /// </summary>
        public bool HasAvailableSeats()
        {
            if (Capacity == 0)
                return true;
            return _students.Count < Capacity;
        }

        /// <summary>
        /// Gets enrollment percentage (0-100)
        /// </summary>
        public double GetEnrollmentPercentage()
        {
            if (Capacity == 0)
                return 0;
            return (_students.Count * 100.0) / Capacity;
        }

        /// <summary>
        /// Gets total section count
        /// </summary>
        public int GetSectionCount() => _sections.Count;

        /// <summary>
        /// Gets active section count
        /// </summary>
        public int GetActiveSectionCount() => _sections.Count(s => s.IsActive);

        /// <summary>
        /// Gets students in specific section
        /// </summary>
        public IEnumerable<Student> GetStudentsInSection(Guid sectionId)
            => _students.Where(s => s.SectionId == sectionId).ToList();

        /// <summary>
        /// Gets display name (e.g., "Class 10 - Science")
        /// </summary>
        public string GetDisplayName() => $"Class {Grade} - {Name}";

        /// <summary>
        /// Checks if class can accommodate new student
        /// </summary>
        public bool CanEnrollStudent() => HasAvailableSeats() && IsActive;

        #endregion

        #region Validation

        /// <summary>
        /// Validates all input parameters
        /// </summary>
        private static void ValidateInputs(
            string className,
            string classCode,
            int grade,
            Guid academicYearId)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentException("Class name is required.", nameof(className));

            if (string.IsNullOrWhiteSpace(classCode))
                throw new ArgumentException("Class code is required.", nameof(classCode));

            if (className.Length > 100)
                throw new ArgumentException("Class name cannot exceed 100 characters.", nameof(className));

            if (classCode.Length > 20)
                throw new ArgumentException("Class code cannot exceed 20 characters.", nameof(classCode));

            if (grade < 1 || grade > 12)
                throw new ArgumentException("Grade must be between 1 and 12.", nameof(grade));

            if (academicYearId == Guid.Empty)
                throw new ArgumentException("Academic year is required.", nameof(academicYearId));
        }

        #endregion
    }
}
