namespace SchoolManagement.Domain.Common
{
    public class Result
    {
        protected Result(bool status, string message, IEnumerable<string> errors = null)
        {
            Status = status;
            Message = message ?? (status ? "Operation completed successfully." : "Operation failed.");
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        public bool Status { get; }
        public string Message { get; }
        public string[] Errors { get; }

        public static Result Success(string message = "Operation completed successfully.")
        {
            return new Result(true, message);
        }

        public static Result Failure(string message, IEnumerable<string> errors = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty for a failure result.", nameof(message));

            return new Result(false, message, errors ?? Array.Empty<string>());
        }

        public static Result Failure(string message, string error)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty for a failure result.", nameof(message));

            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error cannot be null or empty.", nameof(error));

            return new Result(false, message, new[] { error });
        }

        public static implicit operator bool(Result result) => result.Status;

        public override string ToString()
        {
            if (Status)
                return $"Status: Success, Message: {Message}";

            var errorMsg = Errors.Any() ? $", Errors: {string.Join(", ", Errors)}" : string.Empty;
            return $"Status: Failed, Message: {Message}{errorMsg}";
        }
    }

    public class Result<T> : Result
    {
        protected Result(bool status, T data, string message, IEnumerable<string> errors = null)
            : base(status, message, errors)
        {
            Data = data;
        }

        public T Data { get; }

        public static Result<T> Success(T data, string message = "Operation completed successfully.")
        {
            return new Result<T>(true, data, message);
        }

        public static Result<T> Failure(string message, IEnumerable<string> errors = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty for a failure result.", nameof(message));

            return new Result<T>(false, default, message, errors ?? Array.Empty<string>());
        }

        public static Result<T> Failure(string message, string error)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty for a failure result.", nameof(message));

            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error cannot be null or empty.", nameof(error));

            return new Result<T>(false, default, message, new[] { error });
        }

        // Convert non-generic Result to generic Result<T>
        public static Result<T> FromResult(Result result, T data = default)
        {
            return result.Status
                ? Success(data, result.Message)
                : Failure(result.Message, result.Errors);
        }

        // Functional programming helpers
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return Status
                ? Result<TNew>.Success(mapper(Data), Message)
                : Result<TNew>.Failure(Message, Errors);
        }

        public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
        {
            return Status
                ? binder(Data)
                : Result<TNew>.Failure(Message, Errors);
        }

        public T GetValueOrDefault(T defaultValue = default)
        {
            return Status ? Data : defaultValue;
        }

        public T GetValueOrThrow()
        {
            if (!Status)
                throw new InvalidOperationException($"Cannot get value from failed result. Message: {Message}");

            return Data;
        }
    }
}