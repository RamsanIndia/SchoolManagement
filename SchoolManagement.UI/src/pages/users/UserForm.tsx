import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Switch } from "@/components/ui/switch";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { 
  ArrowLeft, Camera, User, Shield, KeyRound, Save, X,
  Mail, Phone, Building, Briefcase
} from "lucide-react";

const roles = ["Admin", "Teacher", "Accountant", "Librarian", "Transport Manager", "Receptionist"];
const departments = ["Administration", "Academics", "Finance", "Library", "Transport", "Support"];

const permissionOverrides = [
  { id: "users.manage", name: "User Management", category: "Admin" },
  { id: "students.export", name: "Export Students", category: "Students" },
  { id: "fees.approve", name: "Approve Fees", category: "Finance" },
  { id: "reports.generate", name: "Generate Reports", category: "Reports" },
  { id: "attendance.bulk", name: "Bulk Attendance", category: "Attendance" },
];

export default function UserForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;
  const { toast } = useToast();

  const [formData, setFormData] = useState({
    firstName: isEditing ? "Naveen" : "",
    lastName: isEditing ? "Kumar" : "",
    username: isEditing ? "naveen.k" : "",
    email: isEditing ? "naveen@example.com" : "",
    mobile: isEditing ? "+91 98765 43210" : "",
    role: isEditing ? "Admin" : "",
    department: isEditing ? "Administration" : "",
    status: isEditing ? "active" : "active",
    photo: isEditing ? "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150" : "",
    permissionOverrides: [] as string[],
    mfaRequired: false,
    resetPassword: false,
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    toast({
      title: "Success",
      description: isEditing ? "User updated successfully" : "User created successfully",
    });
    navigate("/users");
  };

  const handlePermissionToggle = (permissionId: string) => {
    setFormData(prev => ({
      ...prev,
      permissionOverrides: prev.permissionOverrides.includes(permissionId)
        ? prev.permissionOverrides.filter(p => p !== permissionId)
        : [...prev.permissionOverrides, permissionId]
    }));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/users")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">
            {isEditing ? "Edit User" : "Add New User"}
          </h1>
          <p className="text-muted-foreground">
            {isEditing ? "Update user information and access" : "Create a new user account"}
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit}>
        <Tabs defaultValue="basic" className="space-y-6">
          <TabsList className="grid w-full max-w-md grid-cols-3">
            <TabsTrigger value="basic" className="flex items-center gap-2">
              <User className="h-4 w-4" />
              Basic Details
            </TabsTrigger>
            <TabsTrigger value="access" className="flex items-center gap-2">
              <Shield className="h-4 w-4" />
              Role & Access
            </TabsTrigger>
            <TabsTrigger value="credentials" className="flex items-center gap-2">
              <KeyRound className="h-4 w-4" />
              Credentials
            </TabsTrigger>
          </TabsList>

          {/* Basic Details Tab */}
          <TabsContent value="basic">
            <Card>
              <CardHeader>
                <CardTitle>Basic Information</CardTitle>
                <CardDescription>Personal details and contact information</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Photo Upload */}
                <div className="flex items-center gap-6">
                  <Avatar className="h-24 w-24">
                    <AvatarImage src={formData.photo} />
                    <AvatarFallback className="text-2xl">
                      {formData.firstName?.[0]}{formData.lastName?.[0]}
                    </AvatarFallback>
                  </Avatar>
                  <div className="space-y-2">
                    <Button type="button" variant="outline" size="sm">
                      <Camera className="h-4 w-4 mr-2" />
                      Upload Photo
                    </Button>
                    <p className="text-xs text-muted-foreground">
                      JPG, PNG or GIF. Max size 2MB.
                    </p>
                  </div>
                </div>

                {/* Name Fields */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="firstName">First Name *</Label>
                    <Input
                      id="firstName"
                      value={formData.firstName}
                      onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                      placeholder="Enter first name"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="lastName">Last Name *</Label>
                    <Input
                      id="lastName"
                      value={formData.lastName}
                      onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                      placeholder="Enter last name"
                      required
                    />
                  </div>
                </div>

                {/* Username */}
                <div className="space-y-2">
                  <Label htmlFor="username">Username *</Label>
                  <div className="relative">
                    <User className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                      id="username"
                      value={formData.username}
                      onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                      placeholder="Enter username"
                      className="pl-9"
                      required
                    />
                  </div>
                </div>

                {/* Contact Info */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="email">Email Address *</Label>
                    <div className="relative">
                      <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        id="email"
                        type="email"
                        value={formData.email}
                        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        placeholder="Enter email address"
                        className="pl-9"
                        required
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="mobile">Mobile Number *</Label>
                    <div className="relative">
                      <Phone className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <Input
                        id="mobile"
                        value={formData.mobile}
                        onChange={(e) => setFormData({ ...formData, mobile: e.target.value })}
                        placeholder="+91 00000 00000"
                        className="pl-9"
                        required
                      />
                    </div>
                  </div>
                </div>

                {/* Department */}
                <div className="space-y-2">
                  <Label htmlFor="department">Department</Label>
                  <div className="relative">
                    <Building className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground z-10" />
                    <Select value={formData.department} onValueChange={(value) => setFormData({ ...formData, department: value })}>
                      <SelectTrigger className="pl-9">
                        <SelectValue placeholder="Select department" />
                      </SelectTrigger>
                      <SelectContent>
                        {departments.map(dept => (
                          <SelectItem key={dept} value={dept}>{dept}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Role & Access Tab */}
          <TabsContent value="access">
            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle>Role Assignment</CardTitle>
                  <CardDescription>Assign a role to define base permissions</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="role">Select Role *</Label>
                    <div className="relative">
                      <Briefcase className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground z-10" />
                      <Select value={formData.role} onValueChange={(value) => setFormData({ ...formData, role: value })}>
                        <SelectTrigger className="pl-9">
                          <SelectValue placeholder="Select role" />
                        </SelectTrigger>
                        <SelectContent>
                          {roles.map(role => (
                            <SelectItem key={role} value={role}>{role}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle>Permission Overrides</CardTitle>
                  <CardDescription>Grant additional permissions beyond the role</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {permissionOverrides.map(permission => (
                      <div key={permission.id} className="flex items-center space-x-3 p-3 rounded-lg border hover:bg-muted/50 transition-colors">
                        <Checkbox
                          id={permission.id}
                          checked={formData.permissionOverrides.includes(permission.id)}
                          onCheckedChange={() => handlePermissionToggle(permission.id)}
                        />
                        <div className="flex-1">
                          <Label htmlFor={permission.id} className="cursor-pointer font-medium">
                            {permission.name}
                          </Label>
                          <p className="text-xs text-muted-foreground">{permission.category}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle>Account Status</CardTitle>
                  <CardDescription>Control user access to the system</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-4 rounded-lg border">
                      <div>
                        <p className="font-medium">Active</p>
                        <p className="text-sm text-muted-foreground">User can log in and access the system</p>
                      </div>
                      <Switch
                        checked={formData.status === "active"}
                        onCheckedChange={(checked) => setFormData({ ...formData, status: checked ? "active" : "inactive" })}
                      />
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </TabsContent>

          {/* Credentials Tab */}
          <TabsContent value="credentials">
            <Card>
              <CardHeader>
                <CardTitle>Login Settings</CardTitle>
                <CardDescription>Manage password and security settings</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                {!isEditing && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="password">Password *</Label>
                      <Input
                        id="password"
                        type="password"
                        placeholder="Enter password"
                        required={!isEditing}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="confirmPassword">Confirm Password *</Label>
                      <Input
                        id="confirmPassword"
                        type="password"
                        placeholder="Confirm password"
                        required={!isEditing}
                      />
                    </div>
                  </div>
                )}

                <div className="space-y-4">
                  {isEditing && (
                    <div className="flex items-center justify-between p-4 rounded-lg border">
                      <div>
                        <p className="font-medium">Force Password Reset</p>
                        <p className="text-sm text-muted-foreground">User must change password on next login</p>
                      </div>
                      <Switch
                        checked={formData.resetPassword}
                        onCheckedChange={(checked) => setFormData({ ...formData, resetPassword: checked })}
                      />
                    </div>
                  )}

                  <div className="flex items-center justify-between p-4 rounded-lg border">
                    <div>
                      <p className="font-medium">Require MFA</p>
                      <p className="text-sm text-muted-foreground">Enable two-factor authentication</p>
                    </div>
                    <Switch
                      checked={formData.mfaRequired}
                      onCheckedChange={(checked) => setFormData({ ...formData, mfaRequired: checked })}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        {/* Form Actions */}
        <div className="flex items-center justify-end gap-3 pt-6">
          <Button type="button" variant="outline" onClick={() => navigate("/users")}>
            <X className="h-4 w-4 mr-2" />
            Cancel
          </Button>
          <Button type="submit" className="bg-primary hover:bg-primary/90">
            <Save className="h-4 w-4 mr-2" />
            {isEditing ? "Update User" : "Create User"}
          </Button>
        </div>
      </form>
    </div>
  );
}
