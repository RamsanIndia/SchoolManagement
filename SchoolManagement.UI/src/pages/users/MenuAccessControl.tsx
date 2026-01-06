import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { useToast } from "@/hooks/use-toast";
import { 
  Menu, ChevronRight, ChevronDown, Save, RefreshCcw,
  LayoutDashboard, Users, GraduationCap, BookOpen, Calendar,
  CreditCard, ClipboardList, Settings, BarChart2, Bell
} from "lucide-react";

interface MenuPermissions {
  view: boolean;
  create: boolean;
  edit: boolean;
  delete: boolean;
  approve: boolean;
}

interface MenuItem {
  id: string;
  name: string;
  icon: React.ReactNode;
  permissions: MenuPermissions;
  children?: MenuItem[];
}

const roles = ["Admin", "Teacher", "Accountant", "Librarian", "Transport Manager", "Receptionist"];

const initialMenuStructure: MenuItem[] = [
  {
    id: "dashboard",
    name: "Dashboard",
    icon: <LayoutDashboard className="h-4 w-4" />,
    permissions: { view: true, create: false, edit: false, delete: false, approve: false },
  },
  {
    id: "users",
    name: "User Management",
    icon: <Users className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: true, delete: true, approve: false },
    children: [
      { id: "users-list", name: "Users List", icon: null, permissions: { view: true, create: true, edit: true, delete: true, approve: false } },
      { id: "users-roles", name: "Roles", icon: null, permissions: { view: true, create: true, edit: true, delete: true, approve: false } },
      { id: "users-permissions", name: "Permissions", icon: null, permissions: { view: true, create: true, edit: true, delete: true, approve: false } },
    ],
  },
  {
    id: "students",
    name: "Students",
    icon: <GraduationCap className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: true, delete: false, approve: false },
    children: [
      { id: "students-list", name: "Student List", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
      { id: "students-admission", name: "Admission", icon: null, permissions: { view: true, create: true, edit: false, delete: false, approve: true } },
      { id: "students-promotion", name: "Promotion", icon: null, permissions: { view: true, create: false, edit: true, delete: false, approve: true } },
    ],
  },
  {
    id: "academics",
    name: "Academics",
    icon: <BookOpen className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: true, delete: false, approve: false },
    children: [
      { id: "academics-classes", name: "Classes", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
      { id: "academics-subjects", name: "Subjects", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
      { id: "academics-timetable", name: "Timetable", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
    ],
  },
  {
    id: "attendance",
    name: "Attendance",
    icon: <Calendar className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: true, delete: false, approve: false },
  },
  {
    id: "examinations",
    name: "Examinations",
    icon: <ClipboardList className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: true, delete: false, approve: true },
    children: [
      { id: "exams-schedule", name: "Exam Schedule", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
      { id: "exams-marks", name: "Marks Entry", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
      { id: "exams-results", name: "Results", icon: null, permissions: { view: true, create: false, edit: false, delete: false, approve: true } },
    ],
  },
  {
    id: "fees",
    name: "Fee Management",
    icon: <CreditCard className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: true, delete: false, approve: true },
    children: [
      { id: "fees-structure", name: "Fee Structure", icon: null, permissions: { view: true, create: true, edit: true, delete: false, approve: false } },
      { id: "fees-collection", name: "Collection", icon: null, permissions: { view: true, create: true, edit: false, delete: false, approve: false } },
      { id: "fees-reports", name: "Reports", icon: null, permissions: { view: true, create: false, edit: false, delete: false, approve: false } },
    ],
  },
  {
    id: "reports",
    name: "Reports",
    icon: <BarChart2 className="h-4 w-4" />,
    permissions: { view: true, create: false, edit: false, delete: false, approve: false },
  },
  {
    id: "notifications",
    name: "Notifications",
    icon: <Bell className="h-4 w-4" />,
    permissions: { view: true, create: true, edit: false, delete: true, approve: false },
  },
  {
    id: "settings",
    name: "Settings",
    icon: <Settings className="h-4 w-4" />,
    permissions: { view: true, create: false, edit: true, delete: false, approve: false },
  },
];

export default function MenuAccessControl() {
  const [selectedRole, setSelectedRole] = useState("Admin");
  const [menuStructure, setMenuStructure] = useState<MenuItem[]>(initialMenuStructure);
  const [expandedItems, setExpandedItems] = useState<string[]>(["users", "students", "academics", "examinations", "fees"]);
  const { toast } = useToast();

  const toggleExpanded = (itemId: string) => {
    setExpandedItems(prev => 
      prev.includes(itemId) 
        ? prev.filter(id => id !== itemId)
        : [...prev, itemId]
    );
  };

  const updatePermission = (itemId: string, permission: keyof MenuPermissions, value: boolean, parentId?: string) => {
    setMenuStructure(prev => {
      return prev.map(item => {
        if (parentId) {
          if (item.id === parentId && item.children) {
            return {
              ...item,
              children: item.children.map(child =>
                child.id === itemId 
                  ? { ...child, permissions: { ...child.permissions, [permission]: value } }
                  : child
              )
            };
          }
        } else if (item.id === itemId) {
          return { ...item, permissions: { ...item.permissions, [permission]: value } };
        }
        return item;
      });
    });
  };

  const handleSave = () => {
    toast({
      title: "Permissions Saved",
      description: `Menu access permissions for ${selectedRole} have been updated`,
    });
  };

  const handleReset = () => {
    setMenuStructure(initialMenuStructure);
    toast({
      title: "Reset Complete",
      description: "Menu permissions have been reset to default values",
    });
  };

  const renderMenuItem = (item: MenuItem, parentId?: string) => {
    const hasChildren = item.children && item.children.length > 0;
    const isExpanded = expandedItems.includes(item.id);

    return (
      <div key={item.id} className="space-y-1">
        <Collapsible open={isExpanded} onOpenChange={() => hasChildren && toggleExpanded(item.id)}>
          <div className={`flex items-center gap-3 p-3 rounded-lg border ${parentId ? "ml-6 bg-muted/30" : "bg-card"}`}>
            {hasChildren ? (
              <CollapsibleTrigger asChild>
                <Button variant="ghost" size="icon" className="h-6 w-6 p-0">
                  {isExpanded ? (
                    <ChevronDown className="h-4 w-4" />
                  ) : (
                    <ChevronRight className="h-4 w-4" />
                  )}
                </Button>
              </CollapsibleTrigger>
            ) : (
              <div className="w-6" />
            )}
            
            <div className="flex items-center gap-2 min-w-[180px]">
              {item.icon}
              <span className="font-medium">{item.name}</span>
            </div>

            <div className="flex items-center gap-6 ml-auto">
              <div className="flex items-center gap-2">
                <Checkbox
                  id={`${item.id}-view`}
                  checked={item.permissions.view}
                  onCheckedChange={(checked) => updatePermission(item.id, "view", checked as boolean, parentId)}
                />
                <label htmlFor={`${item.id}-view`} className="text-sm text-muted-foreground">View</label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id={`${item.id}-create`}
                  checked={item.permissions.create}
                  onCheckedChange={(checked) => updatePermission(item.id, "create", checked as boolean, parentId)}
                />
                <label htmlFor={`${item.id}-create`} className="text-sm text-muted-foreground">Create</label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id={`${item.id}-edit`}
                  checked={item.permissions.edit}
                  onCheckedChange={(checked) => updatePermission(item.id, "edit", checked as boolean, parentId)}
                />
                <label htmlFor={`${item.id}-edit`} className="text-sm text-muted-foreground">Edit</label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id={`${item.id}-delete`}
                  checked={item.permissions.delete}
                  onCheckedChange={(checked) => updatePermission(item.id, "delete", checked as boolean, parentId)}
                />
                <label htmlFor={`${item.id}-delete`} className="text-sm text-muted-foreground">Delete</label>
              </div>
              <div className="flex items-center gap-2">
                <Checkbox
                  id={`${item.id}-approve`}
                  checked={item.permissions.approve}
                  onCheckedChange={(checked) => updatePermission(item.id, "approve", checked as boolean, parentId)}
                />
                <label htmlFor={`${item.id}-approve`} className="text-sm text-muted-foreground">Approve</label>
              </div>
            </div>
          </div>

          {hasChildren && (
            <CollapsibleContent className="space-y-1 mt-1">
              {item.children!.map(child => renderMenuItem(child, item.id))}
            </CollapsibleContent>
          )}
        </Collapsible>
      </div>
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Menu Access Control</h1>
          <p className="text-muted-foreground">Configure menu visibility and actions for each role</p>
        </div>
      </div>

      {/* Role Selector & Actions */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Select Role</label>
              <Select value={selectedRole} onValueChange={setSelectedRole}>
                <SelectTrigger className="w-64">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {roles.map(role => (
                    <SelectItem key={role} value={role}>{role}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center gap-3">
              <Button variant="outline" onClick={handleReset}>
                <RefreshCcw className="h-4 w-4 mr-2" />
                Reset to Default
              </Button>
              <Button onClick={handleSave} className="bg-primary hover:bg-primary/90">
                <Save className="h-4 w-4 mr-2" />
                Save Changes
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Menu Tree */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Menu className="h-5 w-5" />
            Menu Structure
          </CardTitle>
          <CardDescription>
            Configure which menu items are visible and what actions are allowed for the selected role
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Column Headers */}
          <div className="flex items-center gap-3 p-3 mb-2 bg-muted/50 rounded-lg text-sm font-medium text-muted-foreground">
            <div className="w-6" />
            <div className="min-w-[180px]">Menu Item</div>
            <div className="flex items-center gap-6 ml-auto">
              <div className="w-16 text-center">View</div>
              <div className="w-16 text-center">Create</div>
              <div className="w-16 text-center">Edit</div>
              <div className="w-16 text-center">Delete</div>
              <div className="w-16 text-center">Approve</div>
            </div>
          </div>

          <div className="space-y-2">
            {menuStructure.map(item => renderMenuItem(item))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
