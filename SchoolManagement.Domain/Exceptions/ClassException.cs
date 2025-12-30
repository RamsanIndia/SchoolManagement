namespace SchoolManagement.Domain.Exceptions
{
    public class ClassException : DomainException
    {
        public ClassException(string message) : base(message) { }
        public ClassException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ClassAlreadyActiveException : ClassException
    {
        public ClassAlreadyActiveException(string message) : base(message) { }
    }

    public class ClassAlreadyInactiveException : ClassException
    {
        public ClassAlreadyInactiveException(string message) : base(message) { }
    }

    public class InvalidClassGradeException : ClassException
    {
        public InvalidClassGradeException(string message) : base(message) { }
    }

    public class ClassCapacityException : ClassException
    {
        public ClassCapacityException(string message) : base(message) { }
    }
}
