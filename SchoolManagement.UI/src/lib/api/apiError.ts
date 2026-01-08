// src/lib/api/apiError.ts

/**
 * Custom API Error class
 * Provides structured error handling with helpful methods
 */
export class ApiError extends Error {
  public statusCode: number;
  public errors: string[];
  public timestamp: string;

  constructor(message: string, statusCode: number, errors: string[] = []) {
    super(message);
    this.name = 'ApiError';
    this.statusCode = statusCode;
    this.errors = errors;
    this.timestamp = new Date().toISOString();

    // Maintains proper stack trace for where error was thrown
    if (Error.captureStackTrace) {
      Error.captureStackTrace(this, ApiError);
    }
  }

  /**
   * Check if error is authentication related
   */
  isAuthError(): boolean {
    return this.statusCode === 401 || this.statusCode === 403;
  }

  /**
   * Check if error is validation related
   */
  isValidationError(): boolean {
    return this.statusCode === 400 || this.statusCode === 422;
  }

  /**
   * Check if error is not found
   */
  isNotFoundError(): boolean {
    return this.statusCode === 404;
  }

  /**
   * Check if error is server related
   */
  isServerError(): boolean {
    return this.statusCode >= 500;
  }

  /**
   * Check if error is network related
   */
  isNetworkError(): boolean {
    return this.statusCode === 0;
  }

  /**
   * Check if error is a conflict
   */
  isConflictError(): boolean {
    return this.statusCode === 409;
  }

  /**
   * Get user-friendly error message
   * ✅ ADDED: This method was missing
   */
  getUserMessage(): string {
    // Network errors
    if (this.isNetworkError()) {
      return 'Network error. Please check your internet connection and try again.';
    }

    // Server errors
    if (this.isServerError()) {
      return 'Server error. Please try again later or contact support if the problem persists.';
    }

    // Authentication errors
    if (this.isAuthError()) {
      if (this.statusCode === 401) {
        return 'Authentication failed. Please login again.';
      }
      return 'You do not have permission to perform this action.';
    }

    // Not found errors
    if (this.isNotFoundError()) {
      return 'The requested resource was not found.';
    }

    // Validation errors - show specific errors if available
    if (this.isValidationError() && this.errors.length > 0) {
      return this.errors.join('. ');
    }

    // Conflict errors
    if (this.isConflictError()) {
      return this.message || 'A conflict occurred. The resource may already exist.';
    }

    // Default to the error message
    return this.message || 'An unexpected error occurred. Please try again.';
  }

  /**
   * Get all error messages as an array
   */
  getAllErrors(): string[] {
    if (this.errors.length > 0) {
      return this.errors;
    }
    return [this.message];
  }

  /**
   * Get formatted error for display
   */
  getFormattedError(): { title: string; description: string } {
    let title = 'Error';

    if (this.isNetworkError()) {
      title = 'Network Error';
    } else if (this.isServerError()) {
      title = 'Server Error';
    } else if (this.isAuthError()) {
      title = 'Authentication Error';
    } else if (this.isValidationError()) {
      title = 'Validation Error';
    } else if (this.isNotFoundError()) {
      title = 'Not Found';
    } else if (this.isConflictError()) {
      title = 'Conflict';
    }

    return {
      title,
      description: this.getUserMessage(),
    };
  }

  /**
   * Convert to plain object for logging
   */
  toJSON() {
    return {
      name: this.name,
      message: this.message,
      statusCode: this.statusCode,
      errors: this.errors,
      timestamp: this.timestamp,
      stack: this.stack,
    };
  }

  /**
   * Create ApiError from unknown error
   */
  static fromError(error: unknown): ApiError {
    if (error instanceof ApiError) {
      return error;
    }

    if (error instanceof Error) {
      return new ApiError(error.message, 0, []);
    }

    if (typeof error === 'string') {
      return new ApiError(error, 0, []);
    }

    return new ApiError('An unknown error occurred', 0, []);
  }

  /**
   * Create a validation error
   */
  static validationError(message: string, errors: string[] = []): ApiError {
    return new ApiError(message, 400, errors);
  }

  /**
   * Create an authentication error
   */
  static authError(message: string = 'Authentication failed'): ApiError {
    return new ApiError(message, 401, []);
  }

  /**
   * Create a forbidden error
   */
  static forbiddenError(message: string = 'Access forbidden'): ApiError {
    return new ApiError(message, 403, []);
  }

  /**
   * Create a not found error
   */
  static notFoundError(message: string = 'Resource not found'): ApiError {
    return new ApiError(message, 404, []);
  }

  /**
   * Create a conflict error
   */
  static conflictError(message: string = 'Resource already exists'): ApiError {
    return new ApiError(message, 409, []);
  }

  /**
   * Create a server error
   */
  static serverError(message: string = 'Internal server error'): ApiError {
    return new ApiError(message, 500, []);
  }

  /**
   * Create a network error
   */
  static networkError(message: string = 'Network error'): ApiError {
    return new ApiError(message, 0, []);
  }
}

export default ApiError;
