using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class SectionDto
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int Capacity { get; set; }
        public int CurrentStrength { get; set; }
        public int AvailableSeats { get; set; }
        public string RoomNumber { get; set; }
        public Guid? ClassTeacherId { get; set; }
        public bool IsActive { get; set; }
        public int TotalSubjects { get; set; }
        public int MaxCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
