import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Upload, Eye, EyeOff } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Switch } from "@/components/ui/switch";

interface User {
  id: number;
  name: string;
  username: string;
  email: string;
  mobile: string;
  role: string;
  department: string;
  status: "Active" | "Inactive" | "Locked";
}

interface UserFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user?: User | null;
}

export function UserFormDialog({ open, onOpenChange, user }: UserFormDialogProps) {
  const [showPassword, setShowPassword] = useState(false);
  const [activeStatus, setActiveStatus] = useState<"Active" | "Inactive" | "Locked">(
    user?.status || "Active"
  );

  const isEdit = !!user;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? `Edit User: ${user.name}` : "Add New User"}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* Profile Photo */}
          <div className="flex flex-col items-center gap-4">
            <Avatar className="h-24 w-24">
              <AvatarFallback className="text-2xl">
                {user?.name
                  ?.split(" ")
                  .map((n) => n[0])
                  .join("") || "NU"}
              </AvatarFallback>
            </Avatar>
            <Button variant="outline" size="sm" className="gap-2">
              <Upload className="h-4 w-4" />
              Upload Photo
            </Button>
            <p className="text-xs text-muted-foreground">
              JPG or PNG, max 2MB
            </p>
          </div>

          <Tabs defaultValue="basic" className="w-full">
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="basic">Basic Details</TabsTrigger>
              <TabsTrigger value="role">Role & Access</TabsTrigger>
              <TabsTrigger value="credentials">Credentials</TabsTrigger>
            </TabsList>

            <TabsContent value="basic" className="space-y-4 mt-4">
              {/* Basic Details */}
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">
                    First Name <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="firstName"
                    placeholder="Enter first name"
                    defaultValue={user?.name?.split(" ")[0]}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">
                    Last Name <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="lastName"
                    placeholder="Enter last name"
                    defaultValue={user?.name?.split(" ")[1]}
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="username">
                    Username <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="username"
                    placeholder="username"
                    defaultValue={user?.username}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="email">
                    Email <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="email"
                    type="email"
                    placeholder="email@school.com"
                    defaultValue={user?.email}
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="mobile">Mobile Number</Label>
                  <Input
                    id="mobile"
                    placeholder="+91 98765 43210"
                    defaultValue={user?.mobile}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="employeeId">Employee ID</Label>
                  <Input id="employeeId" placeholder="Auto-generated" />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="dob">Date of Birth</Label>
                  <Input id="dob" type="date" />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="gender">Gender</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select gender" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="male">Male</SelectItem>
                      <SelectItem value="female">Female</SelectItem>
                      <SelectItem value="other">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </TabsContent>

            <TabsContent value="role" className="space-y-4 mt-4">
              {/* Role & Permissions */}
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="role">
                    Primary Role <span className="text-destructive">*</span>
                  </Label>
                  <Select defaultValue={user?.role}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select role" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Admin">Admin</SelectItem>
                      <SelectItem value="Principal">Principal</SelectItem>
                      <SelectItem value="Teacher">Teacher</SelectItem>
                      <SelectItem value="Accountant">Accountant</SelectItem>
                      <SelectItem value="Librarian">Librarian</SelectItem>
                      <SelectItem value="Transport Manager">
                        Transport Manager
                      </SelectItem>
                      <SelectItem value="Receptionist">Receptionist</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="department">Department</Label>
                  <Select defaultValue={user?.department}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select department" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Administration">
                        Administration
                      </SelectItem>
                      <SelectItem value="Primary Section">
                        Primary Section
                      </SelectItem>
                      <SelectItem value="Secondary Section">
                        Secondary Section
                      </SelectItem>
                      <SelectItem value="Accounts">Accounts</SelectItem>
                      <SelectItem value="Library">Library</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* Status */}
              <div className="space-y-3">
                <Label>Account Status</Label>
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant={activeStatus === "Active" ? "default" : "outline"}
                    className={
                      activeStatus === "Active"
                        ? "bg-green-500 hover:bg-green-600"
                        : ""
                    }
                    onClick={() => setActiveStatus("Active")}
                  >
                    Active
                  </Button>
                  <Button
                    type="button"
                    variant={activeStatus === "Inactive" ? "default" : "outline"}
                    className={
                      activeStatus === "Inactive" ? "bg-muted" : ""
                    }
                    onClick={() => setActiveStatus("Inactive")}
                  >
                    Inactive
                  </Button>
                  <Button
                    type="button"
                    variant={activeStatus === "Locked" ? "default" : "outline"}
                    className={
                      activeStatus === "Locked"
                        ? "bg-destructive hover:bg-destructive/90"
                        : ""
                    }
                    onClick={() => setActiveStatus("Locked")}
                  >
                    Locked
                  </Button>
                </div>
              </div>

              {/* Account Settings */}
              <div className="space-y-3 pt-4 border-t">
                <Label>Account Settings</Label>
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="space-y-0.5">
                      <Label>Force password change on first login</Label>
                      <p className="text-xs text-muted-foreground">
                        User must reset password
                      </p>
                    </div>
                    <Switch />
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="space-y-0.5">
                      <Label>Require Two-Factor Authentication (2FA)</Label>
                      <p className="text-xs text-muted-foreground">
                        Enhanced security
                      </p>
                    </div>
                    <Switch />
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="space-y-0.5">
                      <Label>Send welcome email</Label>
                      <p className="text-xs text-muted-foreground">
                        Notify user via email
                      </p>
                    </div>
                    <Switch defaultChecked />
                  </div>
                  <div className="flex items-center justify-between">
                    <div className="space-y-0.5">
                      <Label>Email notifications enabled</Label>
                      <p className="text-xs text-muted-foreground">
                        System notifications
                      </p>
                    </div>
                    <Switch defaultChecked />
                  </div>
                </div>
              </div>
            </TabsContent>

            <TabsContent value="credentials" className="space-y-4 mt-4">
              {!isEdit && (
                <>
                  <div className="space-y-2">
                    <Label htmlFor="password">
                      Password <span className="text-destructive">*</span>
                    </Label>
                    <div className="relative">
                      <Input
                        id="password"
                        type={showPassword ? "text" : "password"}
                        placeholder="Enter password"
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="absolute right-0 top-0"
                        onClick={() => setShowPassword(!showPassword)}
                      >
                        {showPassword ? (
                          <EyeOff className="h-4 w-4" />
                        ) : (
                          <Eye className="h-4 w-4" />
                        )}
                      </Button>
                    </div>
                    <p className="text-xs text-muted-foreground">
                      Must be at least 8 characters with uppercase, number, and
                      special character
                    </p>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="confirmPassword">
                      Confirm Password <span className="text-destructive">*</span>
                    </Label>
                    <Input
                      id="confirmPassword"
                      type="password"
                      placeholder="Confirm password"
                    />
                  </div>

                  <Button type="button" variant="outline" className="w-full">
                    Generate Strong Password
                  </Button>

                  <div className="flex items-center space-x-2">
                    <Checkbox id="tempPassword" />
                    <Label htmlFor="tempPassword" className="text-sm font-normal">
                      Temporary password (user must change on first login)
                    </Label>
                  </div>
                </>
              )}

              {isEdit && (
                <div className="text-center py-8">
                  <p className="text-muted-foreground mb-4">
                    Password settings are managed separately
                  </p>
                  <Button variant="outline">Reset Password</Button>
                </div>
              )}
            </TabsContent>
          </Tabs>
        </div>

        {/* Footer Actions */}
        <div className="flex justify-between pt-4 border-t">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <div className="flex gap-2">
            <Button variant="outline">Save as Draft</Button>
            <Button>{isEdit ? "Update User" : "Create User"}</Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
