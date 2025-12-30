using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Events;
using SchoolManagement.Domain.Exceptions;
using System;

namespace SchoolManagement.Domain.Entities
{
    public class SectionSubject : BaseEntity
    {
        public Guid SectionId { get; private set; }
        public Guid SubjectId { get; private set; }
        public string SubjectName { get; private set; }
        public string SubjectCode { get; private set; }
        public Guid TeacherId { get; private set; }
        public string TeacherName { get; private set; }
        public int WeeklyPeriods { get; private set; }
        public bool IsMandatory { get; private set; }

        // Navigation properties
        public virtual Section Section { get; private set; }
        public virtual Teacher Teacher { get; private set; }

        // EF Core constructor
        private SectionSubject() { }

        /// <summary>
        /// Factory method to create a section-subject assignment
        /// </summary>
        public static SectionSubject Create(
            Guid sectionId,
            Guid subjectId,
            string subjectName,
            string subjectCode,
            Guid teacherId,
            string teacherName,
            int weeklyPeriods,
            bool isMandatory
            )
        {
            ValidateWeeklyPeriods(weeklyPeriods);

            var sectionSubject = new SectionSubject
            {
                Id = Guid.NewGuid(),
                SectionId = sectionId,
                SubjectId = subjectId,
                SubjectName = subjectName,
                SubjectCode = subjectCode,
                TeacherId = teacherId,
                TeacherName = teacherName,
                WeeklyPeriods = weeklyPeriods,
                IsMandatory = isMandatory,
                //CreatedAt = DateTime.UtcNow
                
            };

            sectionSubject.AddDomainEvent(new SubjectAssignedToSectionEvent(
                sectionId,
                subjectId,
                teacherId,
                weeklyPeriods,
                isMandatory
            ));

            return sectionSubject;
        }

        /// <summary>
        /// Updates the teacher assigned to this subject
        /// </summary>
        public void UpdateTeacher(Guid teacherId, string teacherName, string updatedBy)
        {
            if (teacherId == Guid.Empty)
                throw new ArgumentException("Teacher ID cannot be empty.", nameof(teacherId));

            if (string.IsNullOrWhiteSpace(teacherName))
                throw new ArgumentException("Teacher name is required.", nameof(teacherName));

            var previousTeacherId = TeacherId;
            TeacherId = teacherId;
            TeacherName = teacherName;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new SectionSubjectTeacherChangedEvent(
                Id,
                SectionId,
                SubjectId,
                previousTeacherId,
                teacherId
            ));
        }

        /// <summary>
        /// Updates the weekly periods for this subject
        /// </summary>
        public void UpdateWeeklyPeriods(int weeklyPeriods, string updatedBy)
        {
            ValidateWeeklyPeriods(weeklyPeriods);

            var previousPeriods = WeeklyPeriods;
            WeeklyPeriods = weeklyPeriods;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new WeeklyPeriodsUpdatedEvent(
                Id,
                SectionId,
                SubjectId,
                previousPeriods,
                weeklyPeriods
            ));
        }

        /// <summary>
        /// Updates the mandatory status of this subject
        /// </summary>
        public void UpdateMandatoryStatus(bool isMandatory, string updatedBy)
        {
            IsMandatory = isMandatory;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;

            AddDomainEvent(new SubjectMandatoryStatusChangedEvent(
                Id,
                SectionId,
                SubjectId,
                isMandatory
            ));
        }

        /// <summary>
        /// Updates subject details (when subject is renamed/recoded)
        /// </summary>
        public void UpdateSubjectDetails(string subjectName, string subjectCode, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
                throw new ArgumentException("Subject name is required.", nameof(subjectName));

            if (string.IsNullOrWhiteSpace(subjectCode))
                throw new ArgumentException("Subject code is required.", nameof(subjectCode));

            SubjectName = subjectName;
            SubjectCode = subjectCode;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        private static void ValidateWeeklyPeriods(int weeklyPeriods)
        {
            if (weeklyPeriods <= 0)
                throw new ArgumentException("Weekly periods must be greater than zero.", nameof(weeklyPeriods));

            if (weeklyPeriods > 20)
                throw new ArgumentException("Weekly periods cannot exceed 20.", nameof(weeklyPeriods));
        }
    }
}
