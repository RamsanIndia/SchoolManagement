using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;

namespace SchoolManagement.Domain.Entities
{
    public class AcademicYear : BaseEntity
    {
        public string Name { get; private set; }
        public int StartYear { get; private set; }
        public int EndYear { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsCurrent { get; private set; }

        // Navigation Properties
        public virtual ICollection<Class> Classes { get; private set; }

        private AcademicYear() { }

        public static AcademicYear Create(
            string name,
            int startYear,
            int endYear,
            DateTime startDate,
            DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Academic year name is required.", nameof(name));

            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date.", nameof(startDate));

            if (endYear != startYear + 1)
                throw new ArgumentException("End year must be one year after start year.", nameof(endYear));

            return new AcademicYear
            {
                Id = Guid.NewGuid(),
                Name = name,
                StartYear = startYear,
                EndYear = endYear,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = true,
                IsCurrent = false,
                CreatedAt = DateTime.UtcNow,
                Classes = new List<Class>()
            };
        }

        // Add this method to AcademicYear entity
        public void Update(string name, DateTime startDate, DateTime endDate, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Academic year name is required.", nameof(name));

            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date.", nameof(startDate));

            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void SetAsCurrent(string updatedBy)
        {
            IsCurrent = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void RemoveAsCurrent(string updatedBy)
        {
            IsCurrent = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void Deactivate(string deactivatedBy)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
        }

        public void Activate(string activatedBy)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
        }
    }
}
