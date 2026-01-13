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
        // ===== VALUE OBJECTS =====
        public FullName Name { get; private set; }
        public Email Email { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public Address Address { get; private set; }

        // ===== IDENTITY PROPERTIES =====
        public string EmployeeCode { get; private set; } // Unique teacher identifier
        public DateTime DateOfJoining { get; private set; }
        public DateTime? DateOfLeaving { get; private set; }

        // ===== PROFESSIONAL PROPERTIES =====
        public string Qualification { get; private set; }
        public decimal PriorExperience { get; private set; } // Experience before joining (in years)
        public Guid? DepartmentId { get; private set; }
        public string Specialization { get; private set; }
        public decimal Salary { get; private set; }
        public string EmploymentType { get; private set; } // Full-time, Part-time, Contract

        // ===== ADDITIONAL PROPERTIES =====
        public string Gender { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public string BloodGroup { get; private set; }
        public string EmergencyContact { get; private set; }
        public string EmergencyContactPhone { get; private set; }
        public string PhotoUrl { get; private set; }

        // ===== COMPUTED PROPERTIES =====
        public string FullName => Name.FullNameString;

        public int TotalYearsOfExperience
        {
            get
            {
                var serviceYears = (DateTime.UtcNow - DateOfJoining).TotalDays / 365.25;
                return (int)Math.Floor(PriorExperience + (decimal)serviceYears);
            }
        }

        public bool IsCurrentlyEmployed => IsActive && !IsDeleted && !DateOfLeaving.HasValue;

        public bool IsSeniorTeacher => TotalYearsOfExperience >= 10;

        public int Age => DateTime.Today.Year - DateOfBirth.Year -
            (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        // ===== NAVIGATION PROPERTIES =====
        public virtual Department Department { get; private set; }

        // Teacher teaches subjects through SectionSubject (teaching assignments)
        private readonly List<SectionSubject> _teachingAssignments = new();
        public virtual IReadOnlyCollection<SectionSubject> TeachingAssignments => _teachingAssignments.AsReadOnly();

        // Sections where teacher is class teacher
        private readonly List<Section> _classTeacherSections = new();
        public virtual IReadOnlyCollection<Section> ClassTeacherSections => _classTeacherSections.AsReadOnly();

        // ===== EF CORE CONSTRUCTOR =====
        private Teacher() : base() { }

        // ===== FACTORY METHOD =====
        /// <summary>
        /// Creates a new teacher with validation
        /// </summary>
        public static Teacher Create(
            Guid tenantId,
            Guid schoolId,
            string firstName,
            string lastName,
            string email,
            string phoneNumber,
            string employeeCode,
            DateTime dateOfJoining,
            DateTime dateOfBirth,
            string gender,
            string qualification,
            decimal priorExperience,
            Address address,
            string specialization = null,
            decimal salary = 0,
            string employmentType = "Full-time",
            Guid? departmentId = null,
            string createdBy = null,
            string createdIp = null)
        {
            // ✅ Validation
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId is required", nameof(tenantId));
            if (schoolId == Guid.Empty)
                throw new ArgumentException("SchoolId is required", nameof(schoolId));

            ValidateEmployeeCode(employeeCode);
            ValidateQualification(qualification);
            ValidateExperience(priorExperience);
            ValidateDateOfJoining(dateOfJoining);
            ValidateDateOfBirth(dateOfBirth);

            var teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SchoolId = schoolId,
                Name = new FullName(firstName, lastName),
                Email = new Email(email),
                PhoneNumber = new PhoneNumber(phoneNumber),
                EmployeeCode = employeeCode.Trim().ToUpperInvariant(),
                DateOfJoining = dateOfJoining,
                DateOfBirth = dateOfBirth,
                Gender = gender?.Trim(),
                Qualification = qualification.Trim(),
                PriorExperience = priorExperience,
                Address = address,
                Specialization = specialization?.Trim(),
                Salary = salary,
                EmploymentType = employmentType?.Trim() ?? "Full-time",
                DepartmentId = departmentId,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Set audit info
            teacher.SetCreated(createdBy ?? "System", createdIp ?? "Unknown");
            teacher.Activate(createdBy ?? "System");

            // ✅ Domain event
            //teacher.AddDomainEvent(new TeacherCreatedEvent(
            //    teacher.Id,
            //    teacher.TenantId,
            //    teacher.SchoolId,
            //    teacher.FullName,
            //    teacher.Email.Value,
            //    teacher.EmployeeCode,
            //    dateOfJoining
            //));

            return teacher;
        }

        // ===== DOMAIN METHODS =====

        /// <summary>
        /// Updates teacher's personal information
        /// </summary>
        public void UpdatePersonalDetails(
            string firstName,
            string lastName,
            string phoneNumber,
            Address address,
            string gender = null,
            string bloodGroup = null,
            string updatedBy = null,
            string updatedIp = null)
        {
            Name = new FullName(firstName, lastName);
            PhoneNumber = new PhoneNumber(phoneNumber);
            Address = address;

            if (!string.IsNullOrWhiteSpace(gender))
                Gender = gender.Trim();

            if (!string.IsNullOrWhiteSpace(bloodGroup))
                BloodGroup = bloodGroup.Trim();

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherPersonalDetailsUpdatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    Name.FullNameDisplay,
            //    phoneNumber
            //));
        }

        /// <summary>
        /// Updates teacher's professional details
        /// </summary>
        public void UpdateProfessionalDetails(
            string qualification,
            decimal priorExperience,
            string specialization = null,
            Guid? departmentId = null,
            string updatedBy = null,
            string updatedIp = null)
        {
            ValidateQualification(qualification);
            ValidateExperience(priorExperience);

            var previousDepartment = DepartmentId;

            Qualification = qualification.Trim();
            PriorExperience = priorExperience;
            Specialization = specialization?.Trim();
            DepartmentId = departmentId;

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherProfessionalDetailsUpdatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    qualification,
            //    priorExperience,
            //    previousDepartment,
            //    departmentId
            //));
        }

        /// <summary>
        /// Updates teacher's email address
        /// </summary>
        public void UpdateEmail(string newEmail, string updatedBy = null, string updatedIp = null)
        {
            var oldEmail = Email.Value;
            Email = new Email(newEmail);

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherEmailUpdatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    oldEmail,
            //    newEmail
            //));
        }

        /// <summary>
        /// Updates teacher's salary
        /// </summary>
        public void UpdateSalary(decimal newSalary, string updatedBy = null, string updatedIp = null)
        {
            if (newSalary < 0)
                throw new ArgumentException("Salary cannot be negative", nameof(newSalary));

            var oldSalary = Salary;
            Salary = newSalary;

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherSalaryUpdatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    oldSalary,
            //    newSalary
            //));
        }

        /// <summary>
        /// Updates emergency contact information
        /// </summary>
        public void UpdateEmergencyContact(
            string contactName,
            string contactPhone,
            string updatedBy = null,
            string updatedIp = null)
        {
            EmergencyContact = contactName?.Trim();
            EmergencyContactPhone = contactPhone?.Trim();

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");
        }

        /// <summary>
        /// Updates profile photo
        /// </summary>
        public void UpdatePhoto(string photoUrl, string updatedBy = null, string updatedIp = null)
        {
            PhotoUrl = photoUrl?.Trim();
            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");
        }

        /// <summary>
        /// Assigns teacher to a department
        /// </summary>
        public void AssignToDepartment(Guid departmentId, string assignedBy = null, string assignedIp = null)
        {
            if (departmentId == Guid.Empty)
                throw new ArgumentException("Department ID is required", nameof(departmentId));

            if (DepartmentId == departmentId)
                throw new DomainException("Teacher is already assigned to this department");

            var previousDepartment = DepartmentId;
            DepartmentId = departmentId;

            SetUpdated(assignedBy ?? "System", assignedIp ?? "Unknown");

            //AddDomainEvent(new TeacherAssignedToDepartmentEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    previousDepartment,
            //    departmentId
            //));
        }

        /// <summary>
        /// Removes teacher from department
        /// </summary>
        public void RemoveFromDepartment(string removedBy = null, string removedIp = null)
        {
            if (!DepartmentId.HasValue)
                throw new DomainException("Teacher is not assigned to any department");

            var previousDepartment = DepartmentId;
            DepartmentId = null;

            SetUpdated(removedBy ?? "System", removedIp ?? "Unknown");

            //AddDomainEvent(new TeacherRemovedFromDepartmentEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    previousDepartment.Value
            //));
        }

        /// <summary>
        /// Increments teacher's prior experience
        /// </summary>
        public void IncrementPriorExperience(decimal years, string updatedBy = null, string updatedIp = null)
        {
            if (years <= 0)
                throw new ArgumentException("Experience increment must be positive", nameof(years));

            PriorExperience += years;
            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherExperienceUpdatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    PriorExperience
            //));
        }

        /// <summary>
        /// Activates an inactive teacher (overrides BaseEntity)
        /// </summary>
        public new void Activate(string activatedBy = null, string activatedIp = null)
        {
            if (IsActive && !IsDeleted)
                throw new TeacherAlreadyActiveException($"Teacher {FullName} is already active");

            base.Activate(activatedBy ?? "System");
            DateOfLeaving = null;
            SetUpdated(activatedBy ?? "System", activatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherActivatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    activatedBy ?? "System"
            //));
        }

        /// <summary>
        /// Deactivates a teacher - validates no active assignments (overrides BaseEntity)
        /// </summary>
        public new void Deactivate(string deactivatedBy = null, string deactivatedIp = null)
        {
            if (!IsActive)
                throw new TeacherAlreadyInactiveException($"Teacher {FullName} is already inactive");

            // ✅ Business Rule: Cannot deactivate if has teaching assignments
            if (HasActiveTeachingAssignments())
                throw new TeacherHasActiveAssignmentsException(
                    $"Cannot deactivate teacher {FullName}. Remove all teaching assignments first.");

            // ✅ Business Rule: Cannot deactivate if assigned as class teacher
            if (HasClassTeacherAssignment())
                throw new TeacherHasActiveAssignmentsException(
                    $"Cannot deactivate teacher {FullName}. Remove class teacher assignments first.");

            base.Deactivate(deactivatedBy ?? "System");
            DateOfLeaving = DateTime.UtcNow;
            SetUpdated(deactivatedBy ?? "System", deactivatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherDeactivatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    deactivatedBy ?? "System",
            //    DateOfLeaving.Value
            //));
        }

        /// <summary>
        /// Processes teacher resignation
        /// </summary>
        public void Resign(
            DateTime resignationDate,
            string reason,
            string processedBy = null,
            string processedIp = null)
        {
            if (!IsActive)
                throw new TeacherInactiveException("Teacher is already inactive");

            if (resignationDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Resignation date cannot be in the past");

            base.Deactivate(processedBy ?? "System");
            DateOfLeaving = resignationDate;
            SetUpdated(processedBy ?? "System", processedIp ?? "Unknown");

            //AddDomainEvent(new TeacherResignedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    FullName,
            //    resignationDate,
            //    reason
            //));
        }

        /// <summary>
        /// Terminates teacher employment
        /// </summary>
        public void Terminate(
            DateTime terminationDate,
            string reason,
            string terminatedBy = null,
            string terminatedIp = null)
        {
            if (!IsActive)
                throw new TeacherInactiveException("Teacher is already inactive");

            base.Deactivate(terminatedBy ?? "System");
            DateOfLeaving = terminationDate;
            SetUpdated(terminatedBy ?? "System", terminatedIp ?? "Unknown");

            //AddDomainEvent(new TeacherTerminatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    FullName,
            //    terminationDate,
            //    reason
            //));
        }

        // ===== QUERY METHODS =====

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

        // ===== PRIVATE VALIDATION METHODS =====

        private static void ValidateExperience(decimal experience)
        {
            if (experience < 0)
                throw new ArgumentException("Experience cannot be negative", nameof(experience));

            if (experience > 50)
                throw new ArgumentException("Experience cannot exceed 50 years", nameof(experience));
        }

        private static void ValidateQualification(string qualification)
        {
            if (string.IsNullOrWhiteSpace(qualification))
                throw new ArgumentException("Qualification is required", nameof(qualification));

            if (qualification.Length > 200)
                throw new ArgumentException("Qualification cannot exceed 200 characters", nameof(qualification));
        }

        private static void ValidateEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
                throw new ArgumentException("Employee code is required", nameof(employeeCode));

            if (employeeCode.Length > 20)
                throw new ArgumentException("Employee code cannot exceed 20 characters", nameof(employeeCode));
        }

        private static void ValidateDateOfJoining(DateTime dateOfJoining)
        {
            if (dateOfJoining > DateTime.Today)
                throw new ArgumentException("Date of joining cannot be in the future", nameof(dateOfJoining));
        }

        private static void ValidateDateOfBirth(DateTime dateOfBirth)
        {
            if (dateOfBirth >= DateTime.Today)
                throw new ArgumentException("Date of birth must be in the past", nameof(dateOfBirth));

            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (age < 18)
                throw new ArgumentException("Teacher must be at least 18 years old", nameof(dateOfBirth));

            if (age > 100)
                throw new ArgumentException("Invalid date of birth", nameof(dateOfBirth));
        }

        // ===== EQUALITY =====
        public override bool Equals(object obj) => obj is Teacher other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}