namespace SchoolManagement.Application.Shared
{
    /// <summary>
    /// Application-wide constant values for enterprise-grade operations
    /// </summary>
    public static class Constants
    {
        // ===== Default Values =====
        public const string UnknownIpAddress = "Unknown";
        public const string SystemUser = "System";
        public const string UnknownUserAgent = "Unknown";

        // ===== Token Settings =====
        public const int RefreshTokenExpiryDays = 7;
        public const int AccessTokenExpiryMinutes = 15; // ✅ Changed from 60 to 15 (security best practice)
        public const int MaxLoginAttempts = 5;
        public const int LockoutDurationMinutes = 15;
        public const int RefreshTokenLength = 64; // ✅ bytes for secure refresh tokens

        // ===== Header Names =====
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const string ForwardedForHeader = "X-Forwarded-For";
        public const string RealIpHeader = "X-Real-IP"; // ✅ Added
        public const string CloudflareIpHeader = "CF-Connecting-IP"; // ✅ Added
        public const string UserAgentHeader = "User-Agent"; // ✅ Added
        public const string AuthorizationHeader = "Authorization"; // ✅ Added

        // ===== Claim Types =====
        public static class ClaimTypes
        {
            public const string UserId = "userId";
            public const string Username = "username";
            public const string FullName = "fullName";
            public const string Email = "email";
            public const string PhoneNumber = "phoneNumber";
            public const string Role = "role";
        }

        // ===== Cache Keys =====
        public static class CacheKeys
        {
            public const string TokenBlacklist = "blacklist:{0}"; // {0} = JTI
            public const string UserById = "user:{0}"; // {0} = UserId
            public const string UserByEmail = "user:email:{0}"; // {0} = Email
            public const string RefreshToken = "refresh:{0}"; // {0} = Token
            public const string LoginAttempts = "login:attempts:{0}"; // {0} = Email
        }

        // ===== Cache Expiration (in minutes) =====
        public static class CacheExpiration
        {
            public const int UserCache = 30;
            public const int LoginAttemptsCache = 15;
            public const int ShortCache = 5;
            public const int MediumCache = 30;
            public const int LongCache = 60;
        }

        // ===== Validation =====
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 100;
        public const int MinUsernameLength = 3; // ✅ Added
        public const int MaxUsernameLength = 50;
        public const int MaxEmailLength = 255; // ✅ Added
        public const int MaxPhoneNumberLength = 20; // ✅ Added
        public const int MaxNameLength = 100; // ✅ Added
        public const int MaxDescriptionLength = 500; // ✅ Added
        public const int MaxClassCodeLength = 20; // ✅ Added

        // ===== Pagination =====
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int MinPageNumber = 1;

        // ===== File Upload =====
        public static class FileUpload
        {
            public const int MaxFileSizeInMB = 10;
            public const long MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
            public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
            public const string DefaultUploadPath = "uploads";
        }

        // ===== Date/Time Formats =====
        public static class DateTimeFormats
        {
            public const string DateOnly = "yyyy-MM-dd";
            public const string DateTime = "yyyy-MM-dd HH:mm:ss";
            public const string DateTimeWithMilliseconds = "yyyy-MM-dd HH:mm:ss.fff";
            public const string ISO8601 = "yyyy-MM-ddTHH:mm:ss.fffZ";
        }

        // ===== Regex Patterns =====
        public static class RegexPatterns
        {
            public const string Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            public const string PhoneNumber = @"^\+?[1-9]\d{1,14}$"; // E.164 format
            public const string Username = @"^[a-zA-Z0-9._-]{3,50}$";
            public const string ClassCode = @"^[A-Z0-9\-]+$";
            public const string AlphanumericWithSpaces = @"^[a-zA-Z0-9\s\-]+$";
            // ✅ Password: min 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char
            public const string StrongPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$";
        }

        // ===== Error Messages =====
        public static class ErrorMessages
        {
            // Authentication
            public const string UserNotFound = "User not found.";
            public const string InvalidCredentials = "Invalid email or password.";
            public const string AccountLocked = "Account is locked due to multiple failed login attempts. Please try again later.";
            public const string AccountDeactivated = "Account has been deactivated. Please contact administrator.";
            public const string InvalidRefreshToken = "Invalid refresh token.";
            public const string ExpiredRefreshToken = "Refresh token has expired. Please login again.";
            public const string RefreshTokenRequired = "Refresh token is required.";
            public const string EmailAlreadyExists = "User with this email already exists.";
            public const string UsernameAlreadyExists = "Username is already taken.";
            public const string UnauthorizedAccess = "You are not authorized to perform this action.";
            public const string InvalidToken = "Invalid or expired token.";

            // Validation
            public const string InvalidEmailFormat = "Invalid email format.";
            public const string InvalidPhoneFormat = "Invalid phone number format.";
            public const string WeakPassword = "Password must contain uppercase, lowercase, number, and special character.";
            public const string PasswordTooShort = "Password must be at least 8 characters long.";
            public const string RequiredFieldMissing = "{0} is required.";

            // Operations
            public const string OperationFailed = "Operation failed. Please try again.";
            public const string RecordNotFound = "{0} not found.";
            public const string DuplicateRecord = "{0} already exists.";
            public const string DeleteFailed = "Failed to delete {0}.";
            public const string UpdateFailed = "Failed to update {0}.";
            public const string CreateFailed = "Failed to create {0}.";

            // File Upload
            public const string FileSizeExceeded = "File size exceeds maximum allowed size of {0}MB.";
            public const string InvalidFileType = "Invalid file type. Allowed types: {0}";
            public const string FileUploadFailed = "File upload failed. Please try again.";

            // Generic
            public const string InternalServerError = "An error occurred while processing your request.";
            public const string BadRequest = "Invalid request data.";
            public const string ConcurrencyError = "The record has been modified by another user. Please refresh and try again.";
        }

        // ===== Success Messages =====
        public static class SuccessMessages
        {
            // Authentication
            public const string LoginSuccessful = "Login successful.";
            public const string LogoutSuccessful = "Logout successful.";
            public const string RegistrationSuccessful = "User registered successfully.";
            public const string TokenRefreshed = "Token refreshed successfully.";
            public const string PasswordChanged = "Password changed successfully.";
            public const string PasswordReset = "Password reset successfully.";

            // Operations
            public const string OperationSuccessful = "Operation completed successfully.";
            public const string RecordCreated = "{0} created successfully.";
            public const string RecordUpdated = "{0} updated successfully.";
            public const string RecordDeleted = "{0} deleted successfully.";
            public const string RecordActivated = "{0} activated successfully.";
            public const string RecordDeactivated = "{0} deactivated successfully.";

            // File Upload
            public const string FileUploaded = "File uploaded successfully.";
            public const string FileDeleted = "File deleted successfully.";
        }

        // ===== HTTP Status Messages =====
        public static class HttpStatusMessages
        {
            public const string Ok = "Request completed successfully.";
            public const string Created = "Resource created successfully.";
            public const string NoContent = "Request completed successfully with no content.";
            public const string BadRequest = "Invalid request parameters.";
            public const string Unauthorized = "Authentication required.";
            public const string Forbidden = "You do not have permission to access this resource.";
            public const string NotFound = "Resource not found.";
            public const string Conflict = "Resource already exists.";
            public const string InternalServerError = "Internal server error occurred.";
        }

        // ===== User Roles =====
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Teacher = "Teacher";
            public const string Student = "Student";
            public const string Parent = "Parent";
            public const string Staff = "Staff";

            public static readonly string[] All = { Admin, Teacher, Student, Parent, Staff };
            public static readonly string[] AdminAndTeacher = { Admin, Teacher };
            public static readonly string[] AdminOnly = { Admin };
        }

        // ===== Entity Names (for logging/messages) =====
        public static class EntityNames
        {
            public const string User = "User";
            public const string Class = "Class";
            public const string Student = "Student";
            public const string Teacher = "Teacher";
            public const string Subject = "Subject";
            public const string AcademicYear = "Academic Year";
            public const string Attendance = "Attendance";
            public const string Grade = "Grade";
            public const string Section = "Section";
        }

        // ===== Logging Event IDs =====
        public static class LogEventIds
        {
            public const int UserRegistered = 1001;
            public const int UserLogin = 1002;
            public const int UserLogout = 1003;
            public const int TokenRefreshed = 1004;
            public const int PasswordChanged = 1005;
            public const int AccountLocked = 1006;
            public const int UnauthorizedAccess = 1007;
            public const int ValidationFailed = 1008;
            public const int OperationFailed = 1009;
            public const int ConcurrencyConflict = 1010;
        }

        // ===== Environment Names =====
        public static class Environments
        {
            public const string Development = "Development";
            public const string Staging = "Staging";
            public const string Production = "Production";
        }

        // ===== Configuration Keys =====
        public static class ConfigKeys
        {
            public const string JwtSecretKey = "Jwt:SecretKey";
            public const string JwtIssuer = "Jwt:Issuer";
            public const string JwtAudience = "Jwt:Audience";
            public const string ConnectionString = "ConnectionStrings:DefaultConnection";
            public const string RedisConnection = "ConnectionStrings:Redis";
            public const string AzureStorageConnection = "ConnectionStrings:AzureStorage";
        }
    }
}
