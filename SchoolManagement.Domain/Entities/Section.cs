using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using SchoolManagement.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.Entities
{
    public class Section : BaseEntity
    {
        public string Name { get; private set; }
        public Guid ClassId { get; private set; }
        public RoomNumber RoomNumber { get; private set; }
        public SectionCapacity Capacity { get; private set; }
        public Guid? ClassTeacherId { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual Class Class { get; private set; }

        private readonly List<Student> _students = new List<Student>();
        public virtual IReadOnlyCollection<Student> Students => _students.AsReadOnly();

        private readonly List<SectionSubject> _sectionSubjects = new List<SectionSubject>();
        public virtual IReadOnlyCollection<SectionSubject> SectionSubjects => _sectionSubjects.AsReadOnly();

        private readonly List<TimeTableEntry> _timeTableEntries = new List<TimeTableEntry>();
        public virtual IReadOnlyCollection<TimeTableEntry> TimeTableEntries => _timeTableEntries.AsReadOnly();

        // EF Core constructor
        private Section() { }

        // Factory method for creating a new section
        public static Section Create(
            Guid classId,
            string sectionName,
            int capacity,
            string roomNumber)
        {
            ValidateSectionName(sectionName);

            var section = new Section
            {
                Id = Guid.NewGuid(),
                ClassId = classId,
                Name = sectionName.Trim(),
                Capacity = new SectionCapacity(capacity, 0),
                RoomNumber = new RoomNumber(roomNumber),
                IsActive = true,
            };

            section.AddDomainEvent(new SectionCreatedEvent(
                section.Id,
                section.ClassId,
                section.Name,
                capacity,
                roomNumber
            ));

            return section;
        }

        public void UpdateDetails(string sectionName, int capacity, string roomNumber)
        {
            ValidateSectionName(sectionName);

            Name = sectionName.Trim();
            Capacity = Capacity.UpdateCapacity(capacity);
            RoomNumber = new RoomNumber(roomNumber);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new SectionUpdatedEvent(
                Id,
                Name,
                capacity,
                roomNumber
            ));
        }

        /// <summary>
        /// Checks if a class teacher is currently assigned to this section
        /// </summary>
        public bool HasClassTeacherAssigned()
        {
            return ClassTeacherId.HasValue && ClassTeacherId.Value != Guid.Empty;
        }

        /// <summary>
        /// Assigns a class teacher to this section (enforces one teacher per section rule)
        /// </summary>
        public void AssignClassTeacher(Guid teacherId, string assignedBy)
        {
            // Guard: Validate teacher ID
            if (teacherId == Guid.Empty)
                throw new DomainException("Teacher ID cannot be empty.");

            // Business Rule: One class teacher per section
            if (HasClassTeacherAssigned())
                throw new ClassTeacherAlreadyAssignedException(
                    $"Section '{Name}' already has a class teacher assigned. " +
                    $"Remove the existing teacher before assigning a new one."
                );

            // Business Rule: Cannot assign to inactive section
            if (!IsActive)
                throw new SectionException(
                    "Cannot assign class teacher to an inactive section."
                );

            ClassTeacherId = teacherId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = assignedBy;

            AddDomainEvent(new ClassTeacherAssignedEvent(Id, teacherId, assignedBy));
        }

        /// <summary>
        /// Changes the currently assigned class teacher to a new teacher
        /// </summary>
        public void ChangeClassTeacher(Guid newTeacherId, string changedBy)
        {
            // Guard: Validate new teacher ID
            if (newTeacherId == Guid.Empty)
                throw new DomainException("New teacher ID cannot be empty.");

            // Business Rule: Must have an existing teacher to change
            if (!HasClassTeacherAssigned())
                throw new NoClassTeacherAssignedException(
                    $"Section '{Name}' does not have a class teacher to change. " +
                    $"Use AssignClassTeacher to assign a new teacher."
                );

            // Business Rule: Cannot change to the same teacher
            if (ClassTeacherId == newTeacherId)
                throw new DomainException(
                    "The new teacher is already assigned as the class teacher."
                );

            // Business Rule: Cannot change teacher in inactive section
            if (!IsActive)
                throw new SectionException(
                    "Cannot change class teacher in an inactive section."
                );

            var previousTeacherId = ClassTeacherId;
            ClassTeacherId = newTeacherId;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = changedBy;

            AddDomainEvent(new ClassTeacherChangedEvent(
                Id,
                previousTeacherId.Value,
                newTeacherId,
                changedBy
            ));
        }

        /// <summary>
        /// Removes the currently assigned class teacher from this section
        /// </summary>
        public void RemoveClassTeacher(string removedBy)
        {
            // Business Rule: Cannot remove if no teacher is assigned
            if (!HasClassTeacherAssigned())
                throw new NoClassTeacherAssignedException(
                    $"Section '{Name}' does not have a class teacher to remove."
                );

            var previousTeacherId = ClassTeacherId;
            ClassTeacherId = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = removedBy;

            AddDomainEvent(new ClassTeacherRemovedEvent(
                Id,
                previousTeacherId,
                removedBy
            ));
        }

        public void EnrollStudent(Guid studentId, string enrolledBy)
        {
            // Business Rule: Check capacity before enrollment
            if (!Capacity.HasAvailableSeats())
                throw new SectionCapacityExceededException(
                    $"Section '{Name}' has reached maximum capacity of {Capacity.MaxCapacity} students."
                );

            // Business Rule: Cannot enroll in inactive section
            if (!IsActive)
                throw new SectionException(
                    "Cannot enroll students in an inactive section."
                );

            Capacity = Capacity.IncrementStrength();
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = enrolledBy;

            AddDomainEvent(new StudentEnrolledInSectionEvent(
                Id,
                studentId,
                Capacity.CurrentStrength,
                enrolledBy
            ));
        }

        public void UnenrollStudent(Guid studentId, string unenrolledBy)
        {
            // Business Rule: Cannot unenroll if no students enrolled
            if (Capacity.CurrentStrength <= 0)
                throw new SectionException(
                    "No students to unenroll from this section."
                );

            Capacity = Capacity.DecrementStrength();
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = unenrolledBy;

            AddDomainEvent(new StudentUnenrolledFromSectionEvent(
                Id,
                studentId,
                Capacity.CurrentStrength,
                unenrolledBy
            ));
        }

        public void Activate(string activatedBy)
        {
            if (IsActive)
                throw new SectionAlreadyActiveException(
                    $"Section '{Name}' is already active."
                );

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;

            AddDomainEvent(new SectionActivatedEvent(Id, activatedBy));
        }

        public void Deactivate(string deactivatedBy)
        {
            if (!IsActive)
                throw new SectionAlreadyInactiveException(
                    $"Section '{Name}' is already inactive."
                );

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;

            AddDomainEvent(new SectionDeactivatedEvent(Id, deactivatedBy));
        }

        /// <summary>
        /// Checks if the section has available seats for new students
        /// </summary>
        public bool HasAvailableSeats() => Capacity.HasAvailableSeats();

        /// <summary>
        /// Gets the number of available seats remaining in the section
        /// </summary>
        public int GetAvailableSeats() => Capacity.AvailableSeats();

        /// <summary>
        /// Gets the current enrollment percentage
        /// </summary>
        public decimal GetEnrollmentPercentage()
        {
            if (Capacity.MaxCapacity == 0) return 0;
            return (decimal)Capacity.CurrentStrength / Capacity.MaxCapacity * 100;
        }

        /// <summary>
        /// Checks if the section is full
        /// </summary>
        public bool IsFull() => !Capacity.HasAvailableSeats();

        /// <summary>
        /// Validates section name according to business rules
        /// </summary>
        private static void ValidateSectionName(string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
                throw new InvalidSectionNameException(
                    "Section name cannot be empty or whitespace."
                );

            if (sectionName.Length > 50)
                throw new InvalidSectionNameException(
                    "Section name cannot exceed 50 characters."
                );
        }
    }
}
