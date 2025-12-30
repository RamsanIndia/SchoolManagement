using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using System;

namespace SchoolManagement.Domain.Entities
{
    /// <summary>
    /// StudentParent aggregate root - represents a student's parent or guardian
    /// </summary>
    public class StudentParent : BaseEntity
    {
        public Guid StudentId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public ParentRelationship Relationship { get; private set; }
        public bool IsPrimaryContact { get; private set; }
        public Address Address { get; private set; }
        public string Occupation { get; private set; }
        public bool IsEmergencyContact { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual Student Student { get; private set; }

        // EF Core constructor
        private StudentParent() { }

        /// <summary>
        /// Factory method to create a new student parent
        /// </summary>
        public static StudentParent Create(
            Guid studentId,
            string firstName,
            string lastName,
            string email,
            string phone,
            ParentRelationship relationship,
            Address address = null,
            string occupation = null,
            bool isPrimaryContact = false,
            string createdBy = null)
        {
            ValidateInputs(firstName, lastName, email, phone);

            if (studentId == Guid.Empty)
                throw new ArgumentException("Student ID cannot be empty.", nameof(studentId));

            var parent = new StudentParent
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email?.Trim().ToLowerInvariant(),
                Phone = phone.Trim(),
                Relationship = relationship,
                Address = address,
                Occupation = occupation?.Trim(),
                IsPrimaryContact = isPrimaryContact,
                IsEmergencyContact = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            return parent;
        }

        /// <summary>
        /// Updates parent's personal information
        /// </summary>
        public void UpdatePersonalInfo(
            string firstName,
            string lastName,
            string occupation,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Occupation = occupation?.Trim();
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Updates parent's contact information
        /// </summary>
        public void UpdateContactInfo(
            string email,
            string phone,
            Address address,
            string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone number is required.", nameof(phone));

            Email = email.Trim().ToLowerInvariant();
            Phone = phone.Trim();
            Address = address;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Sets this parent as the primary contact
        /// </summary>
        public void SetAsPrimaryContact(string updatedBy)
        {
            IsPrimaryContact = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Removes this parent as the primary contact
        /// </summary>
        public void RemoveAsPrimaryContact(string updatedBy)
        {
            IsPrimaryContact = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Sets this parent as an emergency contact
        /// </summary>
        public void SetAsEmergencyContact(string updatedBy)
        {
            IsEmergencyContact = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Removes this parent as an emergency contact
        /// </summary>
        public void RemoveAsEmergencyContact(string updatedBy)
        {
            IsEmergencyContact = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Updates the relationship type with the student
        /// </summary>
        public void UpdateRelationship(ParentRelationship relationship, string updatedBy)
        {
            Relationship = relationship;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Deactivates this parent record
        /// </summary>
        public void Deactivate(string deactivatedBy)
        {
            if (!IsActive)
                throw new InvalidOperationException("Parent is already inactive.");

            // Cannot deactivate if primary contact - must reassign first
            if (IsPrimaryContact)
                throw new InvalidOperationException("Cannot deactivate primary contact. Reassign primary contact first.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
        }

        /// <summary>
        /// Activates this parent record
        /// </summary>
        public void Activate(string activatedBy)
        {
            if (IsActive)
                throw new InvalidOperationException("Parent is already active.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
        }

        /// <summary>
        /// Gets the full name of the parent
        /// </summary>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Gets a formatted display name with relationship
        /// </summary>
        public string GetDisplayName()
        {
            return $"{GetFullName()} ({Relationship})";
        }

        /// <summary>
        /// Checks if parent has complete contact information
        /// </summary>
        public bool HasCompleteContactInfo()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Phone) &&
                   Address != null;
        }

        private static void ValidateInputs(string firstName, string lastName, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone number is required.", nameof(phone));

            if (firstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));

            if (lastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));

            if (email.Length > 255)
                throw new ArgumentException("Email cannot exceed 255 characters.", nameof(email));

            if (phone.Length > 15)
                throw new ArgumentException("Phone number cannot exceed 15 characters.", nameof(phone));

            // Basic email validation
            if (!email.Contains("@"))
                throw new ArgumentException("Email is not in valid format.", nameof(email));
        }
    }
}
