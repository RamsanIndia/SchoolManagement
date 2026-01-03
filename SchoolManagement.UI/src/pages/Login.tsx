import { useState, useEffect, useRef } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { GraduationCap, Eye, EyeOff, AlertCircle, Loader2 } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { AxiosError } from "axios";

interface ValidationErrors {
  email?: string;
  password?: string;
  general?: string;
}

export default function Login() {
  const { user, login, isLoading, isAuthenticated } = useAuth();
  const { toast } = useToast();
  
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [touched, setTouched] = useState({ email: false, password: false });
  const [attemptCount, setAttemptCount] = useState(0);
  const [isLocked, setIsLocked] = useState(false);
  const [lockTimeout, setLockTimeout] = useState<number | null>(null);
  
  const emailInputRef = useRef<HTMLInputElement>(null);
  const passwordInputRef = useRef<HTMLInputElement>(null);

  // Maximum login attempts before locking
  const MAX_ATTEMPTS = 5;
  const LOCK_DURATION = 5 * 60 * 1000; // 5 minutes

  // Focus on email input on mount
  useEffect(() => {
    emailInputRef.current?.focus();
  }, []);

  // Handle account lockout timer
  useEffect(() => {
    if (lockTimeout) {
      const timer = setTimeout(() => {
        setIsLocked(false);
        setAttemptCount(0);
        setLockTimeout(null);
        toast({
          title: "Account Unlocked",
          description: "You can now try logging in again.",
        });
      }, LOCK_DURATION);

      return () => clearTimeout(timer);
    }
  }, [lockTimeout, toast]);

  // Redirect if already authenticated
  if (isAuthenticated && user) {
    return <Navigate to="/dashboard" replace />;
  }

  // Validation function
  const validateForm = (): boolean => {
    const newErrors: ValidationErrors = {};

    // Email validation
    if (!email.trim()) {
      newErrors.email = "Email is required";
    } else if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(email)) {
      newErrors.email = "Please enter a valid email address";
    }

    // Password validation
    if (!password) {
      newErrors.password = "Password is required";
    } else if (password.length < 6) {
      newErrors.password = "Password must be at least 6 characters";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Real-time field validation
  const validateField = (field: 'email' | 'password', value: string) => {
    const newErrors = { ...errors };

    if (field === 'email') {
      if (!value.trim()) {
        newErrors.email = "Email is required";
      } else if (!/^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(value)) {
        newErrors.email = "Please enter a valid email address";
      } else {
        delete newErrors.email;
      }
    }

    if (field === 'password') {
      if (!value) {
        newErrors.password = "Password is required";
      } else if (value.length < 6) {
        newErrors.password = "Password must be at least 6 characters";
      } else {
        delete newErrors.password;
      }
    }

    setErrors(newErrors);
  };

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setEmail(value);
    if (touched.email) {
      validateField('email', value);
    }
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setPassword(value);
    if (touched.password) {
      validateField('password', value);
    }
  };

  const handleEmailBlur = () => {
    setTouched(prev => ({ ...prev, email: true }));
    validateField('email', email);
  };

  const handlePasswordBlur = () => {
    setTouched(prev => ({ ...prev, password: true }));
    validateField('password', password);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Check if account is locked
    if (isLocked) {
      toast({
        title: "Account Temporarily Locked",
        description: "Too many failed attempts. Please try again in 5 minutes.",
        variant: "destructive",
      });
      return;
    }

    // Mark all fields as touched
    setTouched({ email: true, password: true });

    // Validate form
    if (!validateForm()) {
      // Focus on first error field
      if (errors.email) {
        emailInputRef.current?.focus();
      } else if (errors.password) {
        passwordInputRef.current?.focus();
      }
      return;
    }

    try {
      await login(email.trim().toLowerCase(), password);
      
      // Reset attempt count on success
      setAttemptCount(0);
      
      toast({
        title: "Welcome back!",
        description: "You have successfully logged in.",
      });
    } catch (error) {
      // Increment attempt count
      const newAttemptCount = attemptCount + 1;
      setAttemptCount(newAttemptCount);

      // Lock account if max attempts reached
      if (newAttemptCount >= MAX_ATTEMPTS) {
        setIsLocked(true);
        setLockTimeout(Date.now());
        toast({
          title: "Account Locked",
          description: `Too many failed login attempts. Please try again in 5 minutes.`,
          variant: "destructive",
        });
        return;
      }

      // Parse error message
      let errorMessage = "Invalid email or password. Please try again.";
      let errorTitle = "Login Failed";

      if (error instanceof AxiosError) {
        if (error.response?.status === 401) {
          errorMessage = error.response?.data?.message || "Invalid credentials. Please check your email and password.";
        } else if (error.response?.status === 429) {
          errorTitle = "Too Many Requests";
          errorMessage = "Please wait a moment before trying again";
        } else if (error.response?.status >= 500) {
          errorTitle = "Server Error";
          errorMessage = "Our servers are experiencing issues. Please try again later.";
        } else if (!error.response) {
          errorTitle = "Network Error";
          errorMessage = "Unable to connect. Please check your internet connection.";
        } else {
          errorMessage = error.response?.data?.message || errorMessage;
        }
      }

      // Show remaining attempts
      const remainingAttempts = MAX_ATTEMPTS - newAttemptCount;
      if (remainingAttempts > 0 && remainingAttempts <= 2) {
        errorMessage += ` (${remainingAttempts} attempt${remainingAttempts === 1 ? '' : 's'} remaining)`;
      }

      setErrors({ general: errorMessage });

      toast({
        title: errorTitle,
        description: errorMessage,
        variant: "destructive",
      });

      // Clear password and focus for retry
      setPassword("");
      passwordInputRef.current?.focus();
    }
  };

  return (
    <div className="min-h-screen bg-gradient-primary flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <Card className="shadow-brand">
          <CardHeader className="text-center">
            <div className="mx-auto w-12 h-12 bg-gradient-accent rounded-lg flex items-center justify-center mb-4">
              <GraduationCap className="h-6 w-6 text-white" />
            </div>
            <CardTitle className="text-2xl font-bold">EduManage</CardTitle>
            <CardDescription>
              Sign in to your School Management System
            </CardDescription>
          </CardHeader>
          <CardContent>
            {/* General error alert */}
            {errors.general && (
              <Alert variant="destructive" className="mb-4">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{errors.general}</AlertDescription>
              </Alert>
            )}

            {/* Lockout warning */}
            {isLocked && (
              <Alert variant="destructive" className="mb-4">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  Account temporarily locked due to too many failed attempts. 
                  Please try again in 5 minutes.
                </AlertDescription>
              </Alert>
            )}

            <form onSubmit={handleSubmit} className="space-y-4" noValidate>
              {/* Email Field */}
              <div className="space-y-2">
                <Label htmlFor="email" className="required">
                  Email <span className="text-destructive">*</span>
                </Label>
                <Input
                  ref={emailInputRef}
                  id="email"
                  name="email"
                  type="email"
                  placeholder="Enter your email"
                  value={email}
                  onChange={handleEmailChange}
                  onBlur={handleEmailBlur}
                  disabled={isLoading || isLocked}
                  required
                  autoComplete="email"
                  aria-required="true"
                  aria-invalid={!!errors.email}
                  aria-describedby={errors.email ? "email-error" : undefined}
                  className={errors.email ? "border-destructive" : ""}
                />
                {errors.email && (
                  <p 
                    id="email-error" 
                    className="text-sm text-destructive flex items-center gap-1"
                    role="alert"
                  >
                    <AlertCircle className="h-3 w-3" />
                    {errors.email}
                  </p>
                )}
              </div>

              {/* Password Field */}
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Label htmlFor="password" className="required">
                    Password <span className="text-destructive">*</span>
                  </Label>
                  <a 
                    href="/forgot-password" 
                    className="text-xs text-primary hover:underline"
                    tabIndex={-1}
                  >
                    Forgot password?
                  </a>
                </div>
                <div className="relative">
                  <Input
                    ref={passwordInputRef}
                    id="password"
                    name="password"
                    type={showPassword ? "text" : "password"}
                    placeholder="Enter your password"
                    value={password}
                    onChange={handlePasswordChange}
                    onBlur={handlePasswordBlur}
                    disabled={isLoading || isLocked}
                    required
                    autoComplete="current-password"
                    aria-required="true"
                    aria-invalid={!!errors.password}
                    aria-describedby={errors.password ? "password-error" : undefined}
                    className={errors.password ? "border-destructive pr-10" : "pr-10"}
                  />
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                    onClick={() => setShowPassword(!showPassword)}
                    disabled={isLoading || isLocked}
                    aria-label={showPassword ? "Hide password" : "Show password"}
                    tabIndex={-1}
                  >
                    {showPassword ? (
                      <EyeOff className="h-4 w-4 text-muted-foreground" />
                    ) : (
                      <Eye className="h-4 w-4 text-muted-foreground" />
                    )}
                  </Button>
                </div>
                {errors.password && (
                  <p 
                    id="password-error" 
                    className="text-sm text-destructive flex items-center gap-1"
                    role="alert"
                  >
                    <AlertCircle className="h-3 w-3" />
                    {errors.password}
                  </p>
                )}
              </div>

              {/* Submit Button */}
              <Button
                type="submit"
                className="w-full bg-gradient-primary hover:opacity-90"
                disabled={isLoading || isLocked}
              >
                {isLoading ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Signing in...
                  </>
                ) : (
                  "Sign In"
                )}
              </Button>
            </form>

            {/* Registration Link */}
            <div className="mt-6 text-center">
              <p className="text-sm text-muted-foreground">
                Don't have an account?{" "}
                <a 
                  href="/register" 
                  className="text-primary font-medium hover:underline"
                >
                  Register here
                </a>
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
