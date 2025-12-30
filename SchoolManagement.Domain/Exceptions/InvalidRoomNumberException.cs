using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class InvalidRoomNumberException : DomainException
    {
        public string ProvidedRoomNumber { get; }

        public InvalidRoomNumberException(string providedRoomNumber, string reason)
            : base($"Room number '{providedRoomNumber}' is invalid: {reason}")
        {
            ProvidedRoomNumber = providedRoomNumber;
        }
    }
}
