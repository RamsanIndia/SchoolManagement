namespace SchoolManagement.Application.Shared
{
    /// <summary>
    /// Application-wide constant values
    /// </summary>
    public static class Constants
    {
        // Default values
        public const string UnknownIpAddress = "Unknown";
        public const string SystemUser = "System";

        // Token settings
        public const int RefreshTokenExpiryDays = 7;
        public const int AccessTokenExpiryMinutes = 60;
        public const int MaxLoginAttempts = 5;
        public const int LockoutDurationMinutes = 15;

        // Header names
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const string ForwardedForHeader = "X-Forwarded-For";

        // Validation
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 100;
        public const int MaxUsernameLength = 50;

        // Pagination
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        // Error messages
        public static class ErrorMessages
        {
            public const string UserNotFound = "User not found";
            public const string InvalidCredentials = "Invalid email or password";
            public const string AccountLocked = "Account is locked";
            public const string AccountDeactivated = "Account is deactivated";
            public const string InvalidRefreshToken = "Invalid refresh token";
            public const string ExpiredRefreshToken = "Invalid or expired refresh token";
            public const string RefreshTokenRequired = "Refresh token is required";
            public const string EmailAlreadyExists = "User with this email already exists";
        }

        // Success messages
        public static class SuccessMessages
        {
            public const string LoginSuccessful = "Login successful";
            public const string RegistrationSuccessful = "User registered successfully";
            public const string TokenRefreshed = "Token refreshed successfully";
            public const string OperationSuccessful = "Operation completed successfully";
        }
    }
}
