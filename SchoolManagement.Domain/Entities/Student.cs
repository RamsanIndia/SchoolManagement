using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.Entities
{
    public class Student : BaseEntity
    {
        // ===== IDENTITY =====
        public string StudentCode { get; private set; } // Unique student identifier
        public string AdmissionNumber { get; private set; }
        public DateTime AdmissionDate { get; private set; }

        // ===== PERSONAL INFORMATION =====
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string MiddleName { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public Gender Gender { get; private set; }

        // ===== CONTACT INFORMATION =====
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public Address Address { get; private set; }

        // ===== ACADEMIC INFORMATION =====
        public Guid ClassId { get; private set; }
        public Guid SectionId { get; private set; }
        public StudentStatus Status { get; private set; }
        public string RollNumber { get; private set; }

        // ===== ADDITIONAL INFORMATION =====
        public string PhotoUrl { get; private set; }
        public BiometricInfo BiometricInfo { get; private set; }
        public string BloodGroup { get; private set; }
        public string Nationality { get; private set; }
        public string Religion { get; private set; }

        // ===== COMPUTED PROPERTIES =====
        public string FullName => string.IsNullOrWhiteSpace(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        public int Age => DateTime.Today.Year - DateOfBirth.Year -
            (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

        public bool IsEnrolled => Status == StudentStatus.Active;

        // ===== NAVIGATION PROPERTIES =====
        public virtual Class Class { get; private set; }
        public virtual Section Section { get; private set; }

        private readonly List<StudentParent> _studentParents = new();
        public virtual IReadOnlyCollection<StudentParent> StudentParents => _studentParents.AsReadOnly();

        private readonly List<Attendance> _attendances = new();
        public virtual IReadOnlyCollection<Attendance> Attendances => _attendances.AsReadOnly();

        private readonly List<FeePayment> _feePayments = new();
        public virtual IReadOnlyCollection<FeePayment> FeePayments => _feePayments.AsReadOnly();

        private readonly List<ExamResult> _examResults = new();
        public virtual IReadOnlyCollection<ExamResult> ExamResults => _examResults.AsReadOnly();

        // ===== EF CORE CONSTRUCTOR =====
        private Student() : base() { }

        // ===== FACTORY METHOD =====
        public static Student Create(
            Guid tenantId,
            Guid schoolId,
            string firstName,
            string lastName,
            string email,
            DateTime dateOfBirth,
            Gender gender,
            Guid classId,
            Guid sectionId,
            string middleName = null,
            string phone = null,
            Address address = null,
            string admissionNumber = null,
            DateTime? admissionDate = null,
            string createdBy = null,
            string createdIp = null)
        {
            // ✅ Validation
            if (tenantId == Guid.Empty)
                throw new ArgumentException("TenantId is required", nameof(tenantId));
            if (schoolId == Guid.Empty)
                throw new ArgumentException("SchoolId is required", nameof(schoolId));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (dateOfBirth >= DateTime.Today)
                throw new ArgumentException("Date of birth must be in the past", nameof(dateOfBirth));
            if (classId == Guid.Empty)
                throw new ArgumentException("ClassId is required", nameof(classId));
            if (sectionId == Guid.Empty)
                throw new ArgumentException("SectionId is required", nameof(sectionId));

            var student = new Student
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SchoolId = schoolId,
                StudentCode = GenerateStudentCode(),
                AdmissionNumber = admissionNumber ?? GenerateAdmissionNumber(),
                AdmissionDate = admissionDate ?? DateTime.UtcNow,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                MiddleName = middleName?.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                Phone = phone?.Trim(),
                DateOfBirth = dateOfBirth,
                Gender = gender,
                ClassId = classId,
                SectionId = sectionId,
                Status = StudentStatus.Active,
                Address = address,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Set audit info
            student.SetCreated(createdBy ?? "System", createdIp ?? "Unknown");
            student.Activate(createdBy ?? "System");

            // ✅ Domain event
            //student.AddDomainEvent(new StudentCreatedEvent(
            //    student.Id,
            //    student.TenantId,
            //    student.SchoolId,
            //    student.StudentCode,
            //    student.FullName,
            //    student.Email));

            return student;
        }

        // ===== DOMAIN METHODS =====

        /// <summary>
        /// Update student's personal information
        /// </summary>
        public void UpdatePersonalInfo(
            string firstName,
            string lastName,
            string middleName,
            string phone,
            Address address,
            string updatedBy = null,
            string updatedIp = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            MiddleName = middleName?.Trim();
            Phone = phone?.Trim();
            Address = address;

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new StudentProfileUpdatedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    FullName));
        }

        /// <summary>
        /// Update contact information
        /// </summary>
        public void UpdateContactInfo(
            string email,
            string phone,
            Address address,
            string updatedBy = null,
            string updatedIp = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            Email = email.Trim().ToLowerInvariant();
            Phone = phone?.Trim();
            Address = address;

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");
        }

        /// <summary>
        /// Update additional information
        /// </summary>
        public void UpdateAdditionalInfo(
            string bloodGroup,
            string nationality,
            string religion,
            string updatedBy = null,
            string updatedIp = null)
        {
            BloodGroup = bloodGroup?.Trim();
            Nationality = nationality?.Trim();
            Religion = religion?.Trim();

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");
        }

        /// <summary>
        /// Transfer student to different class/section
        /// </summary>
        public void TransferToClass(
            Guid newClassId,
            Guid newSectionId,
            string updatedBy = null,
            string updatedIp = null)
        {
            if (newClassId == Guid.Empty)
                throw new ArgumentException("ClassId is required", nameof(newClassId));
            if (newSectionId == Guid.Empty)
                throw new ArgumentException("SectionId is required", nameof(newSectionId));

            if (ClassId == newClassId && SectionId == newSectionId)
                throw new InvalidOperationException("Student is already in this class and section");

            var oldClassId = ClassId;
            var oldSectionId = SectionId;

            ClassId = newClassId;
            SectionId = newSectionId;
            RollNumber = null; // Reset roll number on transfer

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

        //    AddDomainEvent(new StudentTransferredEvent(
        //        Id,
        //        TenantId,
        //        SchoolId,
        //        oldClassId,
        //        oldSectionId,
        //        newClassId,
        //        newSectionId));
        }

        /// <summary>
        /// Assign roll number
        /// </summary>
        public void AssignRollNumber(
            string rollNumber,
            string updatedBy = null,
            string updatedIp = null)
        {
            if (string.IsNullOrWhiteSpace(rollNumber))
                throw new ArgumentException("Roll number is required", nameof(rollNumber));

            RollNumber = rollNumber.Trim();
            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");
        }

        /// <summary>
        /// Enroll biometric information
        /// </summary>
        public void EnrollBiometric(
            BiometricInfo biometricInfo,
            string updatedBy = null,
            string updatedIp = null)
        {
            BiometricInfo = biometricInfo ?? throw new ArgumentNullException(nameof(biometricInfo));
            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new StudentBiometricEnrolledEvent(
            //    Id,
            //    TenantId,
            //    SchoolId));
        }

        /// <summary>
        /// Update profile photo
        /// </summary>
        public void UpdatePhoto(
            string photoUrl,
            string updatedBy = null,
            string updatedIp = null)
        {
            PhotoUrl = photoUrl?.Trim();
            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");
        }

        /// <summary>
        /// Update student status
        /// </summary>
        public void UpdateStatus(
            StudentStatus newStatus,
            string reason = null,
            string updatedBy = null,
            string updatedIp = null)
        {
            if (Status == newStatus)
                return;

            var oldStatus = Status;
            Status = newStatus;

            // ✅ Auto-manage IsActive based on status
            switch (newStatus)
            {
                case StudentStatus.Active:
                    Activate(updatedBy ?? "System");
                    break;
                case StudentStatus.Suspended:
                case StudentStatus.Expelled:
                case StudentStatus.Withdrawn:
                case StudentStatus.Graduated:
                    Deactivate(updatedBy ?? "System");
                    break;
            }

            SetUpdated(updatedBy ?? "System", updatedIp ?? "Unknown");

            //AddDomainEvent(new StudentStatusChangedEvent(
            //    Id,
            //    TenantId,
            //    SchoolId,
            //    oldStatus,
            //    newStatus,
            //    reason));
        }

        /// <summary>
        /// Suspend student
        /// </summary>
        public void Suspend(
            string reason,
            string updatedBy = null,
            string updatedIp = null)
        {
            UpdateStatus(StudentStatus.Suspended, reason, updatedBy, updatedIp);
        }

        /// <summary>
        /// Reinstate suspended student
        /// </summary>
        public void Reinstate(
            string updatedBy = null,
            string updatedIp = null)
        {
            if (Status != StudentStatus.Suspended)
                throw new InvalidOperationException("Only suspended students can be reinstated");

            UpdateStatus(StudentStatus.Active, "Reinstated", updatedBy, updatedIp);
        }

        /// <summary>
        /// Graduate student
        /// </summary>
        public void Graduate(
            string updatedBy = null,
            string updatedIp = null)
        {
            UpdateStatus(StudentStatus.Graduated, "Completed studies", updatedBy, updatedIp);
        }

        /// <summary>
        /// Withdraw student
        /// </summary>
        public void Withdraw(
            string reason,
            string updatedBy = null,
            string updatedIp = null)
        {
            UpdateStatus(StudentStatus.Withdrawn, reason, updatedBy, updatedIp);
        }

        /// <summary>
        /// Add parent/guardian
        /// </summary>
        public void AddParent(StudentParent studentParent)
        {
            if (studentParent == null)
                throw new ArgumentNullException(nameof(studentParent));

            if (_studentParents.Any(sp => sp.ParentId == studentParent.ParentId))
                throw new InvalidOperationException("Parent is already linked to this student");

            _studentParents.Add(studentParent);
        }

        /// <summary>
        /// Remove parent/guardian
        /// </summary>
        public void RemoveParent(Guid parentId)
        {
            var studentParent = _studentParents.FirstOrDefault(sp => sp.ParentId == parentId);
            if (studentParent != null)
            {
                _studentParents.Remove(studentParent);
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        private static string GenerateStudentCode()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month;
            var uniqueId = Guid.NewGuid().ToString("N")[..6].ToUpper();

            return $"STD{year}{month:D2}{uniqueId}";
        }

        private static string GenerateAdmissionNumber()
        {
            return $"ADM{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        // ===== EQUALITY =====
        public override bool Equals(object obj) => obj is Student other && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}