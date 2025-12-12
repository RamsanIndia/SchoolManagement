using SchoolManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private SectionSubject() { }

        public SectionSubject(Guid sectionId, Guid subjectId, string subjectName, string subjectCode,
            Guid teacherId, string teacherName, int weeklyPeriods, bool isMandatory)
        {
            SectionId = sectionId;
            SubjectId = subjectId;
            SubjectName = subjectName;
            SubjectCode = subjectCode;
            TeacherId = teacherId;
            TeacherName = teacherName;
            WeeklyPeriods = weeklyPeriods;
            IsMandatory = isMandatory;
            
        }

        public void UpdateTeacher(Guid teacherId, string teacherName)
        {
            TeacherId = teacherId;
            TeacherName = teacherName;
            UpdatedAt = DateTime.UtcNow;
            //UpdatedBy = updatedBy;
        }

        public void UpdateWeeklyPeriods(int weeklyPeriods)
        {
            WeeklyPeriods = weeklyPeriods;
            UpdatedAt = DateTime.UtcNow;
            //UpdatedBy = updatedBy;
        }
    }
}
