import React, { createContext, useContext, useState, useEffect } from "react";
import { apiFetch } from "@/lib/apiFetch";

// Centralized role enum
export enum UserRole {
  Admin = "admin",
  Teacher = "teacher",
  Student = "student",
  HR = "hr",
  Accountant = "accountant",
}

// User interface matching backend response
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Load user safely from storage
  useEffect(() => {
    try {
      const storedUser = localStorage.getItem("sms_user");
      if (storedUser && storedUser !== "undefined") {
        const parsedUser = JSON.parse(storedUser) as User;
        setUser(parsedUser);
      }
    } catch (err) {
      console.error("Error parsing stored user", err);
      localStorage.removeItem("sms_user");
    } finally {
      setIsLoading(false);
    }
  }, []);

  // LOGIN FIXED
  const login = async (email: string, password: string) => {
    setIsLoading(true);
    try {
      const response = await apiFetch("/Auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      console.log("RAW RESPONSE:", response);

      // validate backend envelope
      if (!response.status || !response.data?.accessToken) {
        throw new Error("Invalid credentials");
      }

      const apiUser = response.data.user;

      const mappedUser: User = {
        id: apiUser.id,
        email: apiUser.email,
        firstName: apiUser.firstName,
        lastName: apiUser.lastName,
        roles: (apiUser.roles || []).map((r: string) => r.toLowerCase()),
      };

      // Save to state + storage
      setUser(mappedUser);

      localStorage.setItem("sms_user", JSON.stringify(mappedUser));
      localStorage.setItem("sms_token", response.data.accessToken);
      localStorage.setItem("sms_refresh_token", response.data.refreshToken);

    } catch (error) {
      console.error("Login failed:", error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // LOGOUT
  const logout = () => {
    setUser(null);
    localStorage.removeItem("sms_user");
    localStorage.removeItem("sms_token");
    localStorage.removeItem("sms_refresh_token");
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
