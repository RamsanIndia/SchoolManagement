import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { useToast } from "@/hooks/use-toast";
import { ArrowLeft, Save, X, Shield, CheckSquare, Square } from "lucide-react";

const permissionModules = [
  {
    name: "User Management",
    permissions: [
      { id: "users.view", name: "View Users" },
      { id: "users.create", name: "Create Users" },
      { id: "users.edit", name: "Edit Users" },
      { id: "users.delete", name: "Delete Users" },
    ]
  },
  {
    name: "Student Management",
    permissions: [
      { id: "students.view", name: "View Students" },
      { id: "students.create", name: "Create Students" },
      { id: "students.edit", name: "Edit Students" },
      { id: "students.delete", name: "Delete Students" },
      { id: "students.export", name: "Export Students" },
    ]
  },
  {
    name: "Teacher Management",
    permissions: [
      { id: "teachers.view", name: "View Teachers" },
      { id: "teachers.create", name: "Create Teachers" },
      { id: "teachers.edit", name: "Edit Teachers" },
      { id: "teachers.delete", name: "Delete Teachers" },
    ]
  },
  {
    name: "Attendance",
    permissions: [
      { id: "attendance.view", name: "View Attendance" },
      { id: "attendance.mark", name: "Mark Attendance" },
      { id: "attendance.edit", name: "Edit Attendance" },
      { id: "attendance.reports", name: "View Reports" },
    ]
  },
  {
    name: "Examinations",
    permissions: [
      { id: "exams.view", name: "View Exams" },
      { id: "exams.create", name: "Create Exams" },
      { id: "exams.edit", name: "Edit Exams" },
      { id: "exams.results", name: "Manage Results" },
      { id: "exams.publish", name: "Publish Results" },
    ]
  },
  {
    name: "Fee Management",
    permissions: [
      { id: "fees.view", name: "View Fees" },
      { id: "fees.collect", name: "Collect Fees" },
      { id: "fees.edit", name: "Edit Fee Structure" },
      { id: "fees.approve", name: "Approve Refunds" },
      { id: "fees.reports", name: "Financial Reports" },
    ]
  },
  {
    name: "Timetable",
    permissions: [
      { id: "timetable.view", name: "View Timetable" },
      { id: "timetable.create", name: "Create Timetable" },
      { id: "timetable.edit", name: "Edit Timetable" },
    ]
  },
  {
    name: "Library",
    permissions: [
      { id: "library.view", name: "View Books" },
      { id: "library.manage", name: "Manage Books" },
      { id: "library.issue", name: "Issue/Return Books" },
    ]
  },
  {
    name: "Transport",
    permissions: [
      { id: "transport.view", name: "View Routes" },
      { id: "transport.manage", name: "Manage Routes" },
      { id: "transport.assign", name: "Assign Students" },
    ]
  },
  {
    name: "Inventory",
    permissions: [
      { id: "inventory.view", name: "View Inventory" },
      { id: "inventory.manage", name: "Manage Items" },
      { id: "inventory.purchase", name: "Purchase Orders" },
    ]
  },
];

export default function RoleForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;
  const { toast } = useToast();

  const [formData, setFormData] = useState({
    name: isEditing ? "Teacher" : "",
    description: isEditing ? "Access to academic modules and student management" : "",
    permissions: isEditing ? ["students.view", "students.edit", "attendance.view", "attendance.mark", "exams.view", "exams.create"] : [] as string[],
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    toast({
      title: "Success",
      description: isEditing ? "Role updated successfully" : "Role created successfully",
    });
    navigate("/roles");
  };

  const handlePermissionToggle = (permissionId: string) => {
    setFormData(prev => ({
      ...prev,
      permissions: prev.permissions.includes(permissionId)
        ? prev.permissions.filter(p => p !== permissionId)
        : [...prev.permissions, permissionId]
    }));
  };

  const handleModuleToggle = (module: typeof permissionModules[0]) => {
    const modulePermissionIds = module.permissions.map(p => p.id);
    const allSelected = modulePermissionIds.every(id => formData.permissions.includes(id));
    
    if (allSelected) {
      setFormData(prev => ({
        ...prev,
        permissions: prev.permissions.filter(p => !modulePermissionIds.includes(p))
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        permissions: [...new Set([...prev.permissions, ...modulePermissionIds])]
      }));
    }
  };

  const handleSelectAll = () => {
    const allPermissions = permissionModules.flatMap(m => m.permissions.map(p => p.id));
    const allSelected = allPermissions.every(id => formData.permissions.includes(id));
    
    setFormData(prev => ({
      ...prev,
      permissions: allSelected ? [] : allPermissions
    }));
  };

  const isModuleFullySelected = (module: typeof permissionModules[0]) => {
    return module.permissions.every(p => formData.permissions.includes(p.id));
  };

  const isModulePartiallySelected = (module: typeof permissionModules[0]) => {
    const selected = module.permissions.filter(p => formData.permissions.includes(p.id));
    return selected.length > 0 && selected.length < module.permissions.length;
  };

  const allPermissions = permissionModules.flatMap(m => m.permissions.map(p => p.id));
  const allSelected = allPermissions.every(id => formData.permissions.includes(id));

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/roles")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">
            {isEditing ? "Edit Role" : "Add New Role"}
          </h1>
          <p className="text-muted-foreground">
            {isEditing ? "Modify role details and permissions" : "Create a new role with permissions"}
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Basic Details */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Role Details
            </CardTitle>
            <CardDescription>Basic information about the role</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="name">Role Name *</Label>
                <Input
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., Department Head"
                  required
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Describe the role and its responsibilities"
                rows={3}
                required
              />
            </div>
          </CardContent>
        </Card>

        {/* Permission Assignment */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle>Permission Assignment</CardTitle>
                <CardDescription>
                  {formData.permissions.length} of {allPermissions.length} permissions selected
                </CardDescription>
              </div>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleSelectAll}
              >
                {allSelected ? (
                  <>
                    <Square className="h-4 w-4 mr-2" />
                    Deselect All
                  </>
                ) : (
                  <>
                    <CheckSquare className="h-4 w-4 mr-2" />
                    Select All
                  </>
                )}
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {permissionModules.map(module => (
                <div key={module.name} className="border rounded-lg p-4 space-y-3">
                  <div className="flex items-center justify-between">
                    <h3 className="font-semibold text-sm">{module.name}</h3>
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="h-6 px-2 text-xs"
                      onClick={() => handleModuleToggle(module)}
                    >
                      {isModuleFullySelected(module) ? "Deselect" : "Select All"}
                    </Button>
                  </div>
                  <div className="space-y-2">
                    {module.permissions.map(permission => (
                      <div key={permission.id} className="flex items-center space-x-2">
                        <Checkbox
                          id={permission.id}
                          checked={formData.permissions.includes(permission.id)}
                          onCheckedChange={() => handlePermissionToggle(permission.id)}
                        />
                        <Label htmlFor={permission.id} className="text-sm font-normal cursor-pointer">
                          {permission.name}
                        </Label>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Form Actions */}
        <div className="flex items-center justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate("/roles")}>
            <X className="h-4 w-4 mr-2" />
            Cancel
          </Button>
          <Button type="submit" className="bg-primary hover:bg-primary/90">
            <Save className="h-4 w-4 mr-2" />
            {isEditing ? "Update Role" : "Create Role"}
          </Button>
        </div>
      </form>
    </div>
  );
}
