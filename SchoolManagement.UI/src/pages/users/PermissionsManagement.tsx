import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useToast } from "@/hooks/use-toast";
import { 
  Key, Plus, Search, Filter, MoreHorizontal, Pencil, Trash2,
  ArrowUpDown, ArrowUp, ArrowDown
} from "lucide-react";

interface Permission {
  id: string;
  key: string;
  name: string;
  module: string;
  description: string;
  status: "active" | "inactive";
  createdAt: string;
}

const mockPermissions: Permission[] = [
  { id: "1", key: "users.view", name: "View Users", module: "User Management", description: "View list of all users", status: "active", createdAt: "2024-01-01" },
  { id: "2", key: "users.create", name: "Create Users", module: "User Management", description: "Create new user accounts", status: "active", createdAt: "2024-01-01" },
  { id: "3", key: "users.edit", name: "Edit Users", module: "User Management", description: "Edit existing user accounts", status: "active", createdAt: "2024-01-01" },
  { id: "4", key: "users.delete", name: "Delete Users", module: "User Management", description: "Delete user accounts", status: "active", createdAt: "2024-01-01" },
  { id: "5", key: "students.view", name: "View Students", module: "Student Management", description: "View student records", status: "active", createdAt: "2024-01-01" },
  { id: "6", key: "students.create", name: "Create Students", module: "Student Management", description: "Add new students", status: "active", createdAt: "2024-01-01" },
  { id: "7", key: "students.export", name: "Export Students", module: "Student Management", description: "Export student data to CSV", status: "active", createdAt: "2024-02-15" },
  { id: "8", key: "fees.view", name: "View Fees", module: "Fee Management", description: "View fee structures", status: "active", createdAt: "2024-01-01" },
  { id: "9", key: "fees.collect", name: "Collect Fees", module: "Fee Management", description: "Process fee payments", status: "active", createdAt: "2024-01-01" },
  { id: "10", key: "fees.approve", name: "Approve Refunds", module: "Fee Management", description: "Approve fee refund requests", status: "active", createdAt: "2024-03-10" },
  { id: "11", key: "attendance.view", name: "View Attendance", module: "Attendance", description: "View attendance records", status: "active", createdAt: "2024-01-01" },
  { id: "12", key: "attendance.mark", name: "Mark Attendance", module: "Attendance", description: "Mark student attendance", status: "active", createdAt: "2024-01-01" },
  { id: "13", key: "exams.view", name: "View Exams", module: "Examinations", description: "View exam schedules", status: "active", createdAt: "2024-01-01" },
  { id: "14", key: "exams.create", name: "Create Exams", module: "Examinations", description: "Create new exams", status: "active", createdAt: "2024-01-01" },
  { id: "15", key: "exams.results", name: "Manage Results", module: "Examinations", description: "Enter and manage exam results", status: "active", createdAt: "2024-01-01" },
  { id: "16", key: "reports.view", name: "View Reports", module: "Reports", description: "Access system reports", status: "active", createdAt: "2024-01-01" },
  { id: "17", key: "reports.generate", name: "Generate Reports", module: "Reports", description: "Generate custom reports", status: "inactive", createdAt: "2024-04-20" },
];

const modules = ["All", "User Management", "Student Management", "Fee Management", "Attendance", "Examinations", "Reports"];

export default function PermissionsManagement() {
  const [permissions, setPermissions] = useState<Permission[]>(mockPermissions);
  const [searchTerm, setSearchTerm] = useState("");
  const [moduleFilter, setModuleFilter] = useState("All");
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingPermission, setEditingPermission] = useState<Permission | null>(null);
  const [formData, setFormData] = useState({ key: "", name: "", module: "", description: "" });
  const { toast } = useToast();

  const handleSort = (column: string) => {
    if (sortColumn === column) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortColumn(column);
      setSortDirection("asc");
    }
  };

  const getSortIcon = (column: string) => {
    if (sortColumn !== column) return <ArrowUpDown className="h-4 w-4 ml-1" />;
    return sortDirection === "asc" ? <ArrowUp className="h-4 w-4 ml-1" /> : <ArrowDown className="h-4 w-4 ml-1" />;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingPermission) {
      setPermissions(permissions.map(p => 
        p.id === editingPermission.id 
          ? { ...p, ...formData }
          : p
      ));
      toast({ title: "Success", description: "Permission updated successfully" });
    } else {
      const newPermission: Permission = {
        id: Date.now().toString(),
        ...formData,
        status: "active",
        createdAt: new Date().toISOString().split('T')[0],
      };
      setPermissions([...permissions, newPermission]);
      toast({ title: "Success", description: "Permission created successfully" });
    }
    handleCloseDialog();
  };

  const handleEdit = (permission: Permission) => {
    setEditingPermission(permission);
    setFormData({
      key: permission.key,
      name: permission.name,
      module: permission.module,
      description: permission.description,
    });
    setIsDialogOpen(true);
  };

  const handleDelete = (permissionId: string) => {
    setPermissions(permissions.filter(p => p.id !== permissionId));
    toast({ title: "Success", description: "Permission deleted successfully" });
  };

  const handleCloseDialog = () => {
    setIsDialogOpen(false);
    setEditingPermission(null);
    setFormData({ key: "", name: "", module: "", description: "" });
  };

  const filteredPermissions = permissions
    .filter(permission => {
      const matchesSearch = 
        permission.key.toLowerCase().includes(searchTerm.toLowerCase()) ||
        permission.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        permission.description.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesModule = moduleFilter === "All" || permission.module === moduleFilter;
      return matchesSearch && matchesModule;
    })
    .sort((a, b) => {
      if (!sortColumn) return 0;
      let aValue: any = a[sortColumn as keyof typeof a];
      let bValue: any = b[sortColumn as keyof typeof b];
      if (typeof aValue === "string") aValue = aValue.toLowerCase();
      if (typeof bValue === "string") bValue = bValue.toLowerCase();
      if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
      if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });

  const stats = {
    total: permissions.length,
    active: permissions.filter(p => p.status === "active").length,
    modules: [...new Set(permissions.map(p => p.module))].length,
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Permissions</h1>
          <p className="text-muted-foreground">Manage system permissions</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button className="bg-primary hover:bg-primary/90">
              <Plus className="mr-2 h-4 w-4" />
              Add Permission
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[500px]">
            <DialogHeader>
              <DialogTitle>{editingPermission ? "Edit Permission" : "Add New Permission"}</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="key">Permission Key *</Label>
                <Input
                  id="key"
                  value={formData.key}
                  onChange={(e) => setFormData({ ...formData, key: e.target.value })}
                  placeholder="e.g., users.create"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="name">Display Name *</Label>
                <Input
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., Create Users"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="module">Module *</Label>
                <Select value={formData.module} onValueChange={(value) => setFormData({ ...formData, module: value })}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select module" />
                  </SelectTrigger>
                  <SelectContent>
                    {modules.filter(m => m !== "All").map(module => (
                      <SelectItem key={module} value={module}>{module}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Brief description of the permission"
                />
              </div>
              <div className="flex justify-end gap-3">
                <Button type="button" variant="outline" onClick={handleCloseDialog}>Cancel</Button>
                <Button type="submit">{editingPermission ? "Update" : "Create"}</Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card className="border-l-4 border-l-primary">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Key className="h-4 w-4" />
              Total Permissions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-emerald-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Active Permissions</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600">{stats.active}</div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-violet-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Modules</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-violet-600">{stats.modules}</div>
          </CardContent>
        </Card>
      </div>

      {/* Permissions Table */}
      <Card>
        <CardHeader>
          <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
            <CardTitle className="flex items-center gap-2">
              <Key className="h-5 w-5" />
              Permissions List
            </CardTitle>
            <div className="flex flex-wrap items-center gap-3">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search permissions..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-9 w-64"
                />
              </div>
              <Select value={moduleFilter} onValueChange={setModuleFilter}>
                <SelectTrigger className="w-48">
                  <Filter className="h-4 w-4 mr-2" />
                  <SelectValue placeholder="Module" />
                </SelectTrigger>
                <SelectContent>
                  {modules.map(module => (
                    <SelectItem key={module} value={module}>{module}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("key")}>
                    <div className="flex items-center">Permission Key {getSortIcon("key")}</div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("module")}>
                    <div className="flex items-center">Module {getSortIcon("module")}</div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("description")}>
                    <div className="flex items-center">Description {getSortIcon("description")}</div>
                  </TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredPermissions.map((permission) => (
                  <TableRow key={permission.id}>
                    <TableCell>
                      <code className="px-2 py-1 rounded bg-muted text-sm font-mono">{permission.key}</code>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">{permission.module}</Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground max-w-xs truncate">{permission.description}</TableCell>
                    <TableCell>
                      <Badge className={permission.status === "active" 
                        ? "bg-emerald-500/15 text-emerald-600 border-emerald-500/30" 
                        : "bg-amber-500/15 text-amber-600 border-amber-500/30"
                      }>
                        {permission.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end" className="w-40 bg-popover">
                          <DropdownMenuItem onClick={() => handleEdit(permission)}>
                            <Pencil className="h-4 w-4 mr-2" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuItem 
                            className="text-destructive focus:text-destructive"
                            onClick={() => handleDelete(permission.id)}
                          >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
