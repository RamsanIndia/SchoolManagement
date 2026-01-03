import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useToast } from "@/hooks/use-toast";
import { 
  Shield, Plus, Search, ArrowUpDown, ArrowUp, ArrowDown,
  MoreHorizontal, Pencil, Trash2, Key, Users
} from "lucide-react";

interface Role {
  id: string;
  name: string;
  description: string;
  totalUsers: number;
  permissions: number;
  createdAt: string;
  isSystem: boolean;
}

const mockRoles: Role[] = [
  { id: "1", name: "Admin", description: "Full system access with all permissions", totalUsers: 3, permissions: 45, createdAt: "2024-01-01", isSystem: true },
  { id: "2", name: "Teacher", description: "Access to academic modules and student management", totalUsers: 25, permissions: 18, createdAt: "2024-01-01", isSystem: true },
  { id: "3", name: "Accountant", description: "Fee management and financial reports", totalUsers: 5, permissions: 12, createdAt: "2024-01-01", isSystem: true },
  { id: "4", name: "Librarian", description: "Library management and book tracking", totalUsers: 2, permissions: 8, createdAt: "2024-02-15", isSystem: false },
  { id: "5", name: "Transport Manager", description: "Vehicle and route management", totalUsers: 3, permissions: 10, createdAt: "2024-03-10", isSystem: false },
  { id: "6", name: "Receptionist", description: "Front desk and visitor management", totalUsers: 4, permissions: 6, createdAt: "2024-04-20", isSystem: false },
];

export default function RolesList() {
  const [roles, setRoles] = useState<Role[]>(mockRoles);
  const [searchTerm, setSearchTerm] = useState("");
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const { toast } = useToast();
  const navigate = useNavigate();

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

  const handleDelete = (roleId: string) => {
    const role = roles.find(r => r.id === roleId);
    if (role?.isSystem) {
      toast({ title: "Cannot Delete", description: "System roles cannot be deleted", variant: "destructive" });
      return;
    }
    setRoles(roles.filter(r => r.id !== roleId));
    toast({ title: "Success", description: "Role deleted successfully" });
  };

  const filteredRoles = roles
    .filter(role => 
      role.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      role.description.toLowerCase().includes(searchTerm.toLowerCase())
    )
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
    totalRoles: roles.length,
    systemRoles: roles.filter(r => r.isSystem).length,
    customRoles: roles.filter(r => !r.isSystem).length,
    totalUsers: roles.reduce((sum, r) => sum + r.totalUsers, 0),
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Roles</h1>
          <p className="text-muted-foreground">Manage roles and their permissions</p>
        </div>
        <Button onClick={() => navigate("/roles/add")} className="bg-primary hover:bg-primary/90">
          <Plus className="mr-2 h-4 w-4" />
          Add Role
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card className="border-l-4 border-l-primary">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Shield className="h-4 w-4" />
              Total Roles
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalRoles}</div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-blue-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Key className="h-4 w-4" />
              System Roles
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-600">{stats.systemRoles}</div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-violet-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Shield className="h-4 w-4" />
              Custom Roles
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-violet-600">{stats.customRoles}</div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-emerald-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Users className="h-4 w-4" />
              Total Users
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600">{stats.totalUsers}</div>
          </CardContent>
        </Card>
      </div>

      {/* Roles Table */}
      <Card>
        <CardHeader>
          <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
            <CardTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Roles List
            </CardTitle>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search roles..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9 w-64"
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("name")}>
                    <div className="flex items-center">Role Name {getSortIcon("name")}</div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("description")}>
                    <div className="flex items-center">Description {getSortIcon("description")}</div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("totalUsers")}>
                    <div className="flex items-center">Users {getSortIcon("totalUsers")}</div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("permissions")}>
                    <div className="flex items-center">Permissions {getSortIcon("permissions")}</div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("createdAt")}>
                    <div className="flex items-center">Created On {getSortIcon("createdAt")}</div>
                  </TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredRoles.map((role) => (
                  <TableRow key={role.id}>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{role.name}</span>
                        {role.isSystem && (
                          <Badge variant="outline" className="text-xs bg-blue-500/10 text-blue-600 border-blue-500/30">
                            System
                          </Badge>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="text-muted-foreground max-w-xs truncate">{role.description}</TableCell>
                    <TableCell>
                      <Badge variant="outline" className="font-normal">
                        <Users className="h-3 w-3 mr-1" />
                        {role.totalUsers}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline" className="font-normal">
                        <Key className="h-3 w-3 mr-1" />
                        {role.permissions}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {new Date(role.createdAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell className="text-right">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon">
                            <MoreHorizontal className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end" className="w-48 bg-popover">
                          <DropdownMenuItem onClick={() => navigate(`/roles/edit/${role.id}`)}>
                            <Pencil className="h-4 w-4 mr-2" />
                            Edit Role
                          </DropdownMenuItem>
                          <DropdownMenuItem onClick={() => navigate("/permission-matrix")}>
                            <Key className="h-4 w-4 mr-2" />
                            Manage Permissions
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem 
                            className="text-destructive focus:text-destructive"
                            onClick={() => handleDelete(role.id)}
                            disabled={role.isSystem}
                          >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Delete Role
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
