import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Check, X, Save, RefreshCcw, Copy } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useToast } from "@/hooks/use-toast";

interface PermissionState {
  [module: string]: {
    [permissionType: string]: boolean;
  };
}

const modules = [
  { id: "users", name: "User Management" },
  { id: "students", name: "Student Management" },
  { id: "teachers", name: "Teacher Management" },
  { id: "attendance", name: "Attendance" },
  { id: "exams", name: "Exams & Results" },
  { id: "fees", name: "Fees Management" },
  { id: "library", name: "Library" },
  { id: "transport", name: "Transport" },
  { id: "inventory", name: "Inventory" },
  { id: "reports", name: "Reports" },
];

const permissionTypes = ["View", "Create", "Edit", "Delete", "Approve", "Export"];

const roles = [
  { id: "admin", name: "Admin" },
  { id: "principal", name: "Principal" },
  { id: "teacher", name: "Teacher" },
  { id: "accountant", name: "Accountant" },
  { id: "librarian", name: "Librarian" },
];

const mockPermissions: Record<string, PermissionState> = {
  admin: {
    users: { View: true, Create: true, Edit: true, Delete: true, Approve: true, Export: true },
    students: { View: true, Create: true, Edit: true, Delete: true, Approve: true, Export: true },
    teachers: { View: true, Create: true, Edit: true, Delete: true, Approve: true, Export: true },
    attendance: { View: true, Create: true, Edit: true, Delete: true, Approve: false, Export: true },
    exams: { View: true, Create: true, Edit: true, Delete: true, Approve: true, Export: true },
    fees: { View: true, Create: true, Edit: true, Delete: true, Approve: true, Export: true },
    library: { View: true, Create: true, Edit: true, Delete: true, Approve: false, Export: true },
    transport: { View: true, Create: true, Edit: true, Delete: true, Approve: false, Export: true },
    inventory: { View: true, Create: true, Edit: true, Delete: true, Approve: false, Export: true },
    reports: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: true },
  },
  teacher: {
    users: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    students: { View: true, Create: false, Edit: true, Delete: false, Approve: false, Export: true },
    teachers: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    attendance: { View: true, Create: true, Edit: true, Delete: false, Approve: false, Export: true },
    exams: { View: true, Create: true, Edit: true, Delete: false, Approve: false, Export: false },
    fees: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    library: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: true },
    transport: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    inventory: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    reports: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: true },
  },
  accountant: {
    users: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    students: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: true },
    teachers: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    attendance: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    exams: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    fees: { View: true, Create: true, Edit: true, Delete: false, Approve: true, Export: true },
    library: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    transport: { View: false, Create: false, Edit: false, Delete: false, Approve: false, Export: false },
    inventory: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: true },
    reports: { View: true, Create: false, Edit: false, Delete: false, Approve: false, Export: true },
  },
};

export default function PermissionMatrix() {
  const [selectedRole, setSelectedRole] = useState("admin");
  const [viewMode, setViewMode] = useState<"matrix" | "list">("matrix");
  const [permissions, setPermissions] = useState<PermissionState>(
    mockPermissions[selectedRole] || {}
  );
  const { toast } = useToast();

  const handleRoleChange = (roleId: string) => {
    setSelectedRole(roleId);
    setPermissions(mockPermissions[roleId] || {});
  };

  const togglePermission = (module: string, permissionType: string) => {
    setPermissions((prev) => ({
      ...prev,
      [module]: {
        ...prev[module],
        [permissionType]: !prev[module]?.[permissionType],
      },
    }));
  };

  const toggleRowAll = (module: string) => {
    const allEnabled = permissionTypes.every(
      (type) => permissions[module]?.[type]
    );
    const newState = !allEnabled;
    
    setPermissions((prev) => ({
      ...prev,
      [module]: permissionTypes.reduce(
        (acc, type) => ({ ...acc, [type]: newState }),
        {}
      ),
    }));
  };

  const toggleColumnAll = (permissionType: string) => {
    const allEnabled = modules.every(
      (module) => permissions[module.id]?.[permissionType]
    );
    const newState = !allEnabled;

    setPermissions((prev) => {
      const updated = { ...prev };
      modules.forEach((module) => {
        updated[module.id] = {
          ...updated[module.id],
          [permissionType]: newState,
        };
      });
      return updated;
    });
  };

  const countAssignedPermissions = () => {
    let count = 0;
    modules.forEach((module) => {
      permissionTypes.forEach((type) => {
        if (permissions[module.id]?.[type]) count++;
      });
    });
    return count;
  };

  const totalPermissions = modules.length * permissionTypes.length;
  const assignedCount = countAssignedPermissions();

  const handleSave = () => {
    toast({
      title: "Permissions Updated",
      description: `Successfully updated permissions for ${
        roles.find((r) => r.id === selectedRole)?.name
      }`,
    });
  };

  const handleReset = () => {
    setPermissions(mockPermissions[selectedRole] || {});
    toast({
      title: "Reset Complete",
      description: "Permissions have been reset to default values",
    });
  };

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Role-Permission Matrix
          </h1>
          <p className="text-muted-foreground mt-1">
            Dashboard {'>'} Roles {'>'} Permission Matrix
          </p>
        </div>
      </div>

      {/* Role Selector & Stats */}
      <Card>
        <CardHeader>
          <div className="flex flex-col lg:flex-row lg:items-center gap-4">
            <div className="flex-1">
              <CardTitle className="text-sm font-medium mb-2">
                Configure Permissions For
              </CardTitle>
              <Select value={selectedRole} onValueChange={handleRoleChange}>
                <SelectTrigger className="w-full lg:w-[300px]">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {roles.map((role) => (
                    <SelectItem key={role.id} value={role.id}>
                      {role.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center gap-2">
              <Badge variant="outline" className="text-base px-4 py-2">
                {assignedCount} / {totalPermissions} permissions
              </Badge>
            </div>
          </div>
        </CardHeader>
      </Card>

      {/* Quick Actions */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" size="sm">
              Apply Template
            </Button>
            <Button variant="outline" size="sm">
              <Copy className="h-4 w-4 mr-2" />
              Compare with Role
            </Button>
            <Button variant="outline" size="sm" onClick={handleReset}>
              <RefreshCcw className="h-4 w-4 mr-2" />
              Reset to Default
            </Button>
            <Button size="sm" onClick={handleSave} className="ml-auto">
              <Save className="h-4 w-4 mr-2" />
              Save Changes
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* View Toggle */}
      <Tabs value={viewMode} onValueChange={(v: any) => setViewMode(v)}>
        <TabsList>
          <TabsTrigger value="matrix">📊 Matrix View</TabsTrigger>
          <TabsTrigger value="list">📋 List View</TabsTrigger>
        </TabsList>

        {/* Matrix View */}
        <TabsContent value="matrix" className="mt-6">
          <Card>
            <CardContent className="p-6">
              <ScrollArea className="w-full">
                <div className="min-w-[800px]">
                  <Table>
                    <TableHeader>
                      <TableRow className="hover:bg-transparent">
                        <TableHead className="w-[200px] font-bold">
                          Module
                        </TableHead>
                        {permissionTypes.map((type) => (
                          <TableHead
                            key={type}
                            className="text-center min-w-[120px]"
                          >
                            <div className="flex flex-col items-center gap-2">
                              <span className="font-semibold">{type}</span>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-6 w-6"
                                onClick={() => toggleColumnAll(type)}
                              >
                                <Checkbox
                                  checked={modules.every(
                                    (m) => permissions[m.id]?.[type]
                                  )}
                                />
                              </Button>
                            </div>
                          </TableHead>
                        ))}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {modules.map((module) => (
                        <TableRow key={module.id}>
                          <TableCell className="font-medium">
                            <div className="flex items-center justify-between">
                              <span>{module.name}</span>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-6 w-6"
                                onClick={() => toggleRowAll(module.id)}
                              >
                                <Checkbox
                                  checked={permissionTypes.every(
                                    (type) => permissions[module.id]?.[type]
                                  )}
                                />
                              </Button>
                            </div>
                          </TableCell>
                          {permissionTypes.map((type) => {
                            const isEnabled = permissions[module.id]?.[type];
                            return (
                              <TableCell
                                key={`${module.id}-${type}`}
                                className="text-center"
                              >
                                <button
                                  className={`
                                    w-full h-12 rounded-md flex items-center justify-center
                                    transition-all duration-200 cursor-pointer
                                    border-2
                                    ${
                                      isEnabled
                                        ? "bg-green-500/10 border-green-500/50 hover:bg-green-500/20"
                                        : "bg-muted border-border hover:bg-muted/80"
                                    }
                                  `}
                                  onClick={() =>
                                    togglePermission(module.id, type)
                                  }
                                >
                                  {isEnabled ? (
                                    <Check className="h-5 w-5 text-green-600 dark:text-green-400" />
                                  ) : (
                                    <X className="h-5 w-5 text-muted-foreground" />
                                  )}
                                </button>
                              </TableCell>
                            );
                          })}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
                <ScrollBar orientation="horizontal" />
              </ScrollArea>

              {/* Legend */}
              <div className="flex items-center gap-6 mt-6 pt-6 border-t">
                <div className="flex items-center gap-2">
                  <div className="h-6 w-6 rounded border-2 border-green-500/50 bg-green-500/10 flex items-center justify-center">
                    <Check className="h-4 w-4 text-green-600" />
                  </div>
                  <span className="text-sm text-muted-foreground">
                    Permission Granted
                  </span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="h-6 w-6 rounded border-2 border-border bg-muted flex items-center justify-center">
                    <X className="h-4 w-4 text-muted-foreground" />
                  </div>
                  <span className="text-sm text-muted-foreground">
                    Permission Denied
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* List View */}
        <TabsContent value="list" className="mt-6">
          <Card>
            <CardContent className="p-6 space-y-4">
              {modules.map((module) => (
                <div
                  key={module.id}
                  className="border rounded-lg p-4 space-y-3"
                >
                  <div className="flex items-center justify-between">
                    <h3 className="font-semibold">{module.name}</h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => toggleRowAll(module.id)}
                    >
                      {permissionTypes.every(
                        (type) => permissions[module.id]?.[type]
                      )
                        ? "Deselect All"
                        : "Select All"}
                    </Button>
                  </div>
                  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
                    {permissionTypes.map((type) => {
                      const isEnabled = permissions[module.id]?.[type];
                      return (
                        <div
                          key={`${module.id}-${type}`}
                          className="flex items-center space-x-2"
                        >
                          <Checkbox
                            id={`${module.id}-${type}`}
                            checked={isEnabled}
                            onCheckedChange={() =>
                              togglePermission(module.id, type)
                            }
                          />
                          <label
                            htmlFor={`${module.id}-${type}`}
                            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                          >
                            {type}
                          </label>
                        </div>
                      );
                    })}
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
