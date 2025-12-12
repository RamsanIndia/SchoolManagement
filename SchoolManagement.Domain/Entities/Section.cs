using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class Section : BaseEntity
    {
        public string Name { get; private set; }
        public Guid ClassId { get; private set; }
        public int Capacity { get; private set; }
        public string RoomNumber { get; private set; }
        public int CurrentStrength { get; private set; }
        public Guid? ClassTeacherId { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual Class Class { get; private set; }
        public virtual ICollection<Student> Students { get; private set; }
        public virtual ICollection<SectionSubject> SectionSubjects { get; private set; }
        public virtual ICollection<TimeTableEntry> TimeTableEntries { get; private set; }


        private Section()
        {
            Students = new List<Student>();
            SectionSubjects = new List<SectionSubject>();
            TimeTableEntries = new List<TimeTableEntry>();
        }

        public Section(Guid classId, string sectionName, int capacity, string roomNumber)
        {
            ClassId = classId;
            Name = sectionName;
            Capacity = capacity;
            RoomNumber = roomNumber;
            CurrentStrength = 0;
            IsActive = true;
            SectionSubjects = new HashSet<SectionSubject>();
            TimeTableEntries = new HashSet<TimeTableEntry>();
        }

        public void UpdateDetails(string sectionName, int capacity, string roomNumber)
        {
            Name = sectionName;
            Capacity = capacity;
            RoomNumber = roomNumber;
            UpdatedAt = DateTime.UtcNow;
            
        }

        public void AssignClassTeacher(Guid teacherId)
        {
            ClassTeacherId = teacherId;
            UpdatedAt = DateTime.UtcNow;
            
        }

        public void RemoveClassTeacher()
        {
            ClassTeacherId = null;
            UpdatedAt = DateTime.UtcNow;
            
        }

        public void UpdateStrength(int strength, string updatedBy)
        {
            CurrentStrength = strength;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void Activate(string updatedBy)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void Deactivate(string updatedBy)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public bool HasAvailableSeats() => CurrentStrength < Capacity;
    }
}
