using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base("One or more validation errors occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base(BuildErrorMessage(errors))
        {
            Errors = errors;
        }

        private static string BuildErrorMessage(IDictionary<string, string[]> errors)
        {
            if (errors == null || !errors.Any())
                return "One or more validation errors occurred.";

            var errorMessages = errors
                .Select(e => $"{e.Key}: {string.Join(", ", e.Value)}")
                .ToList();

            return $"Validation failed: {string.Join("; ", errorMessages)}";
        }
    }

}
