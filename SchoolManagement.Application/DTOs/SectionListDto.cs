using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Application.DTOs
{
    public class SectionListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentStrength { get; set; }
        public string RoomNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
