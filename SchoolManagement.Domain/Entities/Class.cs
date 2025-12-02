using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SchoolManagement.Domain.Entities
{
    public class Class : BaseEntity
    {
        public int Grade { get; private set; }
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
        public int Capacity { get; private set; }
        public bool IsActive { get; private set; }
        public Guid AcademicYearId { get; private set; }

        
        // Navigation Properties
        public virtual ICollection<Student> Students { get; private set; } = new List<Student>();
        public virtual ICollection<Section> Sections { get; private set; } = new List<Section>();


        private Class()
        {
            Students = new List<Student>();
            Sections = new List<Section>();
        }

        public Class(string className, string classCode, int grade, string description, Guid academicYearId)
        {
            Name = className;
            Code = classCode;
            Grade = grade;
            Description = description;
            AcademicYearId = academicYearId;
            IsActive = true;
            Sections = new HashSet<Section>();
        }

        public void UpdateDetails(string className, string classCode, int grade, string description)
        {
            Name = className;
            Code = classCode;
            Grade = grade;
            Description = description;
            
        }

        public void Activate()
        {
            IsActive = true;
            
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            
        }
    }
}
