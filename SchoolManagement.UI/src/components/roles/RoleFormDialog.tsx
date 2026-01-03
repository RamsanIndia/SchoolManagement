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
import { Textarea } from "@/components/ui/textarea";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import { Badge } from "@/components/ui/badge";

interface Role {
  id: number;
  name: string;
  description: string;
  type: "System" | "Custom";
}

interface RoleFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  role?: Role | null;
}

const permissionModules = [
  {
    id: "users",
    name: "🔐 User Management",
    permissions: [
      "View Users",
      "Create Users",
      "Edit Users",
      "Delete Users",
      "Reset Passwords",
      "Manage Roles",
      "View Activity Logs",
    ],
  },
  {
    id: "students",
    name: "👨‍🎓 Student Management",
    permissions: [
      "View All Students",
      "View Own Class Students",
      "Create Student Records",
      "Edit Student Records",
      "Delete Student Records",
      "Promote Students",
      "Transfer Students",
      "View Student Documents",
    ],
  },
  {
    id: "teachers",
    name: "👨‍🏫 Teacher Management",
    permissions: [
      "View Teachers",
      "Create Teachers",
      "Edit Teachers",
      "Delete Teachers",
      "Assign Classes",
      "View Teacher Attendance",
    ],
  },
  {
    id: "attendance",
    name: "📅 Attendance",
    permissions: [
      "Mark Attendance",
      "View Attendance",
      "Edit Attendance",
      "Generate Attendance Reports",
      "View Own Attendance",
    ],
  },
  {
    id: "exams",
    name: "📝 Exams & Assessments",
    permissions: [
      "Create Exams",
      "Edit Exams",
      "Delete Exams",
      "Enter Marks",
      "Edit Marks",
      "Publish Results",
      "View Results",
      "Generate Report Cards",
    ],
  },
  {
    id: "fees",
    name: "💰 Fees Management",
    permissions: [
      "View Fees Structure",
      "Edit Fees Structure",
      "Collect Fees",
      "Issue Receipts",
      "Apply Discounts",
      "View Fee Reports",
      "Send Fee Reminders",
      "Refund Fees",
    ],
  },
  {
    id: "library",
    name: "📚 Library Management",
    permissions: [
      "View Books",
      "Add Books",
      "Issue Books",
      "Return Books",
      "Manage Members",
      "View Library Reports",
    ],
  },
  {
    id: "transport",
    name: "🚌 Transport Management",
    permissions: [
      "View Routes",
      "Create Routes",
      "Assign Vehicles",
      "Manage Drivers",
      "Track Vehicles",
      "View Transport Reports",
    ],
  },
];

export function RoleFormDialog({ open, onOpenChange, role }: RoleFormDialogProps) {
  const [roleType, setRoleType] = useState<"System" | "Custom">(
    role?.type || "Custom"
  );
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  const isEdit = !!role;
  const totalPermissions = permissionModules.reduce(
    (acc, mod) => acc + mod.permissions.length,
    0
  );

  const togglePermission = (permission: string) => {
    setSelectedPermissions((prev) =>
      prev.includes(permission)
        ? prev.filter((p) => p !== permission)
        : [...prev, permission]
    );
  };

  const toggleModuleAll = (modulePermissions: string[]) => {
    const allSelected = modulePermissions.every((p) =>
      selectedPermissions.includes(p)
    );
    if (allSelected) {
      setSelectedPermissions((prev) =>
        prev.filter((p) => !modulePermissions.includes(p))
      );
    } else {
      setSelectedPermissions((prev) => [
        ...new Set([...prev, ...modulePermissions]),
      ]);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-5xl max-h-[90vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>
            {isEdit ? `Edit Role: ${role.name}` : "Create New Role"}
          </DialogTitle>
        </DialogHeader>

        <ScrollArea className="flex-1 pr-4">
          <div className="space-y-6 py-4">
            {/* Basic Information */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold">Basic Information</h3>
              
              <div className="space-y-2">
                <Label htmlFor="roleName">
                  Role Name <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="roleName"
                  placeholder="e.g., Class Teacher, Lab Assistant"
                  defaultValue={role?.name}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">
                  Role Description <span className="text-destructive">*</span>
                </Label>
                <Textarea
                  id="description"
                  placeholder="Describe the role and its responsibilities..."
                  rows={3}
                  defaultValue={role?.description}
                />
              </div>

              <div className="space-y-2">
                <Label>Role Type</Label>
                <RadioGroup value={roleType} onValueChange={(v: any) => setRoleType(v)}>
                  <div className="flex items-center space-x-2">
                    <RadioGroupItem value="System" id="system" />
                    <Label htmlFor="system" className="font-normal">
                      ⚙️ System Role (cannot be deleted)
                    </Label>
                  </div>
                  <div className="flex items-center space-x-2">
                    <RadioGroupItem value="Custom" id="custom" />
                    <Label htmlFor="custom" className="font-normal">
                      👤 Custom Role (can be modified/deleted)
                    </Label>
                  </div>
                </RadioGroup>
              </div>
            </div>

            {/* Permission Assignment */}
            <div className="space-y-4 pt-4 border-t">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold">Permission Assignment</h3>
                <Badge variant="outline">
                  {selectedPermissions.length} of {totalPermissions} selected
                </Badge>
              </div>

              <div className="flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() =>
                    setSelectedPermissions(
                      permissionModules.flatMap((m) => m.permissions)
                    )
                  }
                >
                  Select All
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setSelectedPermissions([])}
                >
                  Clear All
                </Button>
              </div>

              <Accordion type="multiple" className="w-full">
                {permissionModules.map((module) => {
                  const moduleSelectedCount = module.permissions.filter((p) =>
                    selectedPermissions.includes(p)
                  ).length;
                  
                  return (
                    <AccordionItem key={module.id} value={module.id}>
                      <AccordionTrigger>
                        <div className="flex items-center justify-between w-full pr-4">
                          <span>{module.name}</span>
                          <Badge variant="secondary" className="ml-2">
                            {moduleSelectedCount}/{module.permissions.length}
                          </Badge>
                        </div>
                      </AccordionTrigger>
                      <AccordionContent>
                        <div className="space-y-3 pl-6 pt-2">
                          <div className="flex items-center space-x-2">
                            <Checkbox
                              id={`select-all-${module.id}`}
                              checked={
                                module.permissions.every((p) =>
                                  selectedPermissions.includes(p)
                                )
                              }
                              onCheckedChange={() =>
                                toggleModuleAll(module.permissions)
                              }
                            />
                            <Label
                              htmlFor={`select-all-${module.id}`}
                              className="font-semibold"
                            >
                              Select All
                            </Label>
                          </div>
                          {module.permissions.map((permission) => (
                            <div
                              key={permission}
                              className="flex items-center space-x-2"
                            >
                              <Checkbox
                                id={permission}
                                checked={selectedPermissions.includes(permission)}
                                onCheckedChange={() => togglePermission(permission)}
                              />
                              <Label htmlFor={permission} className="font-normal">
                                {permission}
                              </Label>
                            </div>
                          ))}
                        </div>
                      </AccordionContent>
                    </AccordionItem>
                  );
                })}
              </Accordion>
            </div>
          </div>
        </ScrollArea>

        {/* Footer Actions */}
        <div className="flex justify-between pt-4 border-t">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <div className="flex gap-2">
            <Button variant="outline">Save as Template</Button>
            <Button>{isEdit ? "Update Role" : "Create Role"}</Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
