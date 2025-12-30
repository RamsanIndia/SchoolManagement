using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagement.Domain.Entities
{
    /// <summary>
    /// Teacher aggregate root - encapsulates teacher business logic and invariants
    /// </summary>
    public class Teacher : BaseEntity
    {
        // Value Objects for type safety and validation
        public FullName Name { get; private set; }
        public Email Email { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public Address Address { get; private set; }

        // Primitive properties
        public string EmployeeId { get; private set; }
        public DateTime DateOfJoining { get; private set; }
        public DateTime? DateOfLeaving { get; private set; }
        public string Qualification { get; private set; }
        public decimal Experience { get; private set; } // In years
        public Guid? DepartmentId { get; private set; }
        public bool IsActive { get; private set; }

        // Computed property
        public string FullName => Name.FullNameString;

        // Navigation Properties
        public virtual Department Department { get; private set; }

        // Teacher teaches subjects through SectionSubject (teaching assignments)
        private readonly List<SectionSubject> _teachingAssignments = new();
        public virtual IReadOnlyCollection<SectionSubject> TeachingAssignments => _teachingAssignments.AsReadOnly();

        // Sections where teacher is class teacher
        private readonly List<Section> _classTeacherSections = new();
        public virtual IReadOnlyCollection<Section> ClassTeacherSections => _classTeacherSections.AsReadOnly();

        // EF Core constructor
        private Teacher() { }

        /// <summary>
        /// Factory Method - Creates a new teacher with validation
        /// </summary>
        public static Teacher Create(
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            string employeeId,
            DateTime dateOfJoining,
            string qualification,
            decimal experience,
            Address address,
            Guid? departmentId = null)
        {
            ValidateExperience(experience);
            ValidateQualification(qualification);
            ValidateEmployeeId(employeeId);

            var teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                Name = new FullName(firstName, lastName),
                Email = new Email(email),
                PhoneNumber = new PhoneNumber(phoneNumber),
                EmployeeId = employeeId.Trim().ToUpper(),
                DateOfJoining = dateOfJoining,
                Qualification = qualification.Trim(),
                Experience = experience,
                Address = address,
                DepartmentId = departmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            teacher.AddDomainEvent(new TeacherCreatedEvent(
                teacher.Id,
                teacher.FullName,
                teacher.Email.Value,
                teacher.EmployeeId,
                dateOfJoining
            ));

            return teacher;
        }

        /// <summary>
        /// Updates teacher's personal information
        /// </summary>
        public void UpdatePersonalDetails(
            string firstName,
            string lastName,
            string phoneNumber,
            Address address,
            string updatedBy)
        {
            Name = new FullName(firstName, lastName);
            PhoneNumber = new PhoneNumber(phoneNumber);
            Address = address;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new TeacherPersonalDetailsUpdatedEvent(
                Id,
                Name.FullNameString,
                phoneNumber
            ));
        }

        /// <summary>
        /// Updates teacher's professional details
        /// </summary>
        public void UpdateProfessionalDetails(
            string qualification,
            decimal experience,
            Guid? departmentId,
            string updatedBy)
        {
            ValidateQualification(qualification);
            ValidateExperience(experience);

            var previousDepartment = DepartmentId;

            Qualification = qualification.Trim();
            Experience = experience;
            DepartmentId = departmentId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new TeacherProfessionalDetailsUpdatedEvent(
                Id,
                qualification,
                experience,
                previousDepartment,
                departmentId
            ));
        }

        /// <summary>
        /// Updates teacher's email address
        /// </summary>
        public void UpdateEmail(string newEmail, string updatedBy)
        {
            var oldEmail = Email.Value;
            Email = new Email(newEmail);
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new TeacherEmailUpdatedEvent(
                Id,
                oldEmail,
                newEmail
            ));
        }

        /// <summary>
        /// Assigns teacher to a department
        /// </summary>
        public void AssignToDepartment(Guid departmentId, string assignedBy)
        {
            if (DepartmentId == departmentId)
                throw new DomainException("Teacher is already assigned to this department.");

            var previousDepartment = DepartmentId;
            DepartmentId = departmentId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = assignedBy;

            AddDomainEvent(new TeacherAssignedToDepartmentEvent(
                Id,
                previousDepartment,
                departmentId
            ));
        }

        /// <summary>
        /// Removes teacher from department
        /// </summary>
        public void RemoveFromDepartment(string removedBy)
        {
            if (!DepartmentId.HasValue)
                throw new DomainException("Teacher is not assigned to any department.");

            var previousDepartment = DepartmentId;
            DepartmentId = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = removedBy;

            AddDomainEvent(new TeacherRemovedFromDepartmentEvent(
                Id,
                previousDepartment.Value
            ));
        }

        /// <summary>
        /// Increments teacher's experience
        /// </summary>
        public void IncrementExperience(decimal years, string updatedBy)
        {
            if (years <= 0)
                throw new ArgumentException("Experience increment must be positive.", nameof(years));

            Experience += years;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new TeacherExperienceUpdatedEvent(
                Id,
                Experience
            ));
        }

        /// <summary>
        /// Activates an inactive teacher
        /// </summary>
        public void Activate(string activatedBy)
        {
            if (IsActive)
                throw new TeacherAlreadyActiveException($"Teacher {FullName} is already active.");

            IsActive = true;
            DateOfLeaving = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;

            AddDomainEvent(new TeacherActivatedEvent(Id, activatedBy));
        }

        /// <summary>
        /// Deactivates a teacher - validates no active assignments
        /// </summary>
        public void Deactivate(string deactivatedBy, DateTime? leavingDate = null)
        {
            if (!IsActive)
                throw new TeacherAlreadyInactiveException($"Teacher {FullName} is already inactive.");

            // Business Rule: Cannot deactivate if has teaching assignments
            if (HasActiveTeachingAssignments())
                throw new TeacherHasActiveAssignmentsException(
                    $"Cannot deactivate teacher {FullName}. Remove all teaching assignments first.");

            // Business Rule: Cannot deactivate if assigned as class teacher
            if (HasClassTeacherAssignment())
                throw new TeacherHasActiveAssignmentsException(
                    $"Cannot deactivate teacher {FullName}. Remove class teacher assignments first.");

            IsActive = false;
            DateOfLeaving = leavingDate ?? DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;

            AddDomainEvent(new TeacherDeactivatedEvent(Id, deactivatedBy, DateOfLeaving.Value));
        }

        /// <summary>
        /// Processes teacher resignation
        /// </summary>
        public void Resign(DateTime resignationDate, string reason, string processedBy)
        {
            if (!IsActive)
                throw new TeacherInactiveException("Teacher is already inactive.");

            if (resignationDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Resignation date cannot be in the past.");

            IsActive = false;
            DateOfLeaving = resignationDate;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = processedBy;

            AddDomainEvent(new TeacherResignedEvent(
                Id,
                FullName,
                resignationDate,
                reason
            ));
        }

        /// <summary>
        /// Checks if teacher has any class teacher assignments
        /// </summary>
        public bool HasClassTeacherAssignment()
        {
            return _classTeacherSections.Any(s => s.IsActive);
        }

        /// <summary>
        /// Checks if teacher has any active teaching assignments
        /// </summary>
        public bool HasActiveTeachingAssignments()
        {
            return _teachingAssignments.Any();
        }

        /// <summary>
        /// Gets total number of teaching assignments
        /// </summary>
        public int GetTotalTeachingAssignments()
        {
            return _teachingAssignments.Count;
        }

        /// <summary>
        /// Gets total weekly periods across all assignments
        /// </summary>
        public int GetTotalWeeklyPeriods()
        {
            return _teachingAssignments.Sum(ta => ta.WeeklyPeriods);
        }

        /// <summary>
        /// Checks if teacher can take on more assignments (workload validation)
        /// </summary>
        public bool CanAcceptMoreAssignments(int maxWeeklyPeriods = 40)
        {
            return IsActive && GetTotalWeeklyPeriods() < maxWeeklyPeriods;
        }

        /// <summary>
        /// Gets all unique subjects being taught by the teacher
        /// </summary>
        public IEnumerable<Guid> GetAssignedSubjectIds()
        {
            return _teachingAssignments
                .Select(ta => ta.SubjectId)
                .Distinct();
        }

        /// <summary>
        /// Checks if teacher is teaching a specific subject
        /// </summary>
        public bool IsTeachingSubject(Guid subjectId)
        {
            return _teachingAssignments.Any(ta => ta.SubjectId == subjectId);
        }

        /// <summary>
        /// Gets all sections where teacher is teaching
        /// </summary>
        public IEnumerable<Guid> GetTeachingSectionIds()
        {
            return _teachingAssignments
                .Select(ta => ta.SectionId)
                .Distinct();
        }

        /// <summary>
        /// Checks if teacher is teaching in a specific section
        /// </summary>
        public bool IsTeachingInSection(Guid sectionId)
        {
            return _teachingAssignments.Any(ta => ta.SectionId == sectionId);
        }

        /// <summary>
        /// Gets teacher's teaching assignments for a specific section
        /// </summary>
        public IEnumerable<SectionSubject> GetAssignmentsForSection(Guid sectionId)
        {
            return _teachingAssignments.Where(ta => ta.SectionId == sectionId);
        }

        /// <summary>
        /// Calculates total years of experience including service time
        /// </summary>
        public int GetTotalYearsOfExperience()
        {
            var serviceYears = (DateTime.UtcNow - DateOfJoining).TotalDays / 365.25;
            return (int)Math.Floor(Experience + (decimal)serviceYears);
        }

        /// <summary>
        /// Checks if teacher qualifies as senior (based on experience)
        /// </summary>
        public bool IsSeniorTeacher(int seniorityThreshold = 10)
        {
            return GetTotalYearsOfExperience() >= seniorityThreshold;
        }

        private static void ValidateExperience(decimal experience)
        {
            if (experience < 0)
                throw new ArgumentException("Experience cannot be negative.", nameof(experience));

            if (experience > 50)
                throw new ArgumentException("Experience cannot exceed 50 years.", nameof(experience));
        }

        private static void ValidateQualification(string qualification)
        {
            if (string.IsNullOrWhiteSpace(qualification))
                throw new ArgumentException("Qualification is required.", nameof(qualification));

            if (qualification.Length > 200)
                throw new ArgumentException("Qualification cannot exceed 200 characters.", nameof(qualification));
        }

        private static void ValidateEmployeeId(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Employee ID is required.", nameof(employeeId));

            if (employeeId.Length > 20)
                throw new ArgumentException("Employee ID cannot exceed 20 characters.", nameof(employeeId));
        }
    }
}
