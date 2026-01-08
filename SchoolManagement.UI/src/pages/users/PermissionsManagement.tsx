import { useState, useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { useToast } from '@/hooks/use-toast';
import {
  Key,
  Plus,
  Search,
  Filter,
  MoreHorizontal,
  Pencil,
  Trash2,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  Loader2,
  RefreshCw,
  AlertCircle,
  Lock,
} from 'lucide-react';

// Import your API hooks
import { usePermissions } from '@/permissions/hooks/usePermissions';
import { useCreatePermission } from '@/permissions/hooks/useCreatePermission';
import { useUpdatePermission } from '@/permissions/hooks/useUpdatePermission';
import { useDeletePermission } from '@/permissions/hooks/useDeletePermission';
import type { Permission, PermissionDto, PermissionQueryParams } from '@/permissions/types/permission.types';

export default function PermissionsManagement() {
  const [searchTerm, setSearchTerm] = useState('');
  const [moduleFilter, setModuleFilter] = useState('All');
  
  const [sortColumn, setSortColumn] = useState<PermissionQueryParams['sortBy']>(undefined);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingPermission, setEditingPermission] = useState<Permission | null>(null);
  const [deletePermissionId, setDeletePermissionId] = useState<string | null>(null);
  
  const [formData, setFormData] = useState<PermissionDto>({
    name: '',
    displayName: '',
    description: '',
    module: '',
    action: '',
    resource: '',
  });

  const { toast } = useToast();

  // Fetch permissions from API
  const {
    data: permissionsData,
    isLoading,
    error,
    refetch,
    isFetching,
  } = usePermissions({
    params: {
      pageNumber,
      pageSize,
      searchTerm: searchTerm || undefined,
      sortBy: sortColumn,
      sortDirection: sortColumn ? sortDirection : undefined,
      module: moduleFilter !== 'All' ? moduleFilter : undefined,
    },
  });

  // Mutations
  const createMutation = useCreatePermission();
  const updateMutation = useUpdatePermission();
  const deleteMutation = useDeletePermission();

  // Extract data from API response
  const permissions = permissionsData?.items || [];
  const totalCount = permissionsData?.totalCount || 0;
  const totalPages = permissionsData?.totalPages || 0;
  const hasNextPage = permissionsData?.hasNextPage || false;
  const hasPreviousPage = permissionsData?.hasPreviousPage || false;

  // ✅ Fixed: Get unique modules with explicit typing
  const modules = useMemo<string[]>(() => {
  const moduleSet = new Set<string>();
  moduleSet.add('All');
  
  permissions.forEach((p) => {
    if (p.module && typeof p.module === 'string') {
      moduleSet.add(p.module);
    }
  });
  
  return Array.from(moduleSet).sort();
}, [permissions]);

  // Client-side sorting
  const sortedPermissions = useMemo(() => {
    if (!sortColumn) return permissions;

    return [...permissions].sort((a, b) => {
      let aValue: any = a[sortColumn as keyof Permission];
      let bValue: any = b[sortColumn as keyof Permission];

      if (typeof aValue === 'string') aValue = aValue.toLowerCase();
      if (typeof bValue === 'string') bValue = bValue.toLowerCase();

      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [permissions, sortColumn, sortDirection]);

  // Calculate stats
  const stats = useMemo(
    () => ({
      total: totalCount,
      systemPermissions: permissions.filter((p) => p.isSystemPermission).length,
      customPermissions: permissions.filter((p) => !p.isSystemPermission).length,
      modules: [...new Set(permissions.map((p) => p.module))].length,
    }),
    [permissions, totalCount]
  );

  const handleSort = (column: PermissionQueryParams['sortBy']) => {
    if (sortColumn === column) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(column);
      setSortDirection('asc');
    }
  };

  const getSortIcon = (column: string) => {
    if (sortColumn !== column) return <ArrowUpDown className="h-4 w-4 ml-1" />;
    return sortDirection === 'asc' ? (
      <ArrowUp className="h-4 w-4 ml-1" />
    ) : (
      <ArrowDown className="h-4 w-4 ml-1" />
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingPermission) {
        await updateMutation.mutateAsync({
          id: editingPermission.id,
          data: formData,
        });
      } else {
        await createMutation.mutateAsync(formData);
      }
      handleCloseDialog();
    } catch (error) {
      // Error handling is done by the mutation hooks
    }
  };

  const handleEdit = (permission: Permission) => {
    setEditingPermission(permission);
    setFormData({
      name: permission.name,
      displayName: permission.displayName,
      description: permission.description,
      module: permission.module,
      action: permission.action,
      resource: permission.resource,
    });
    setIsDialogOpen(true);
  };

  const handleDeleteClick = (permissionId: string) => {
    const permission = permissions.find((p) => p.id === permissionId);
    if (permission?.isSystemPermission) {
      toast({
        title: 'Cannot Delete',
        description: 'System permissions cannot be deleted',
        variant: 'destructive',
      });
      return;
    }
    setDeletePermissionId(permissionId);
  };

  const handleDeleteConfirm = () => {
    if (deletePermissionId) {
      deleteMutation.mutate(deletePermissionId);
      setDeletePermissionId(null);
    }
  };

  const handleCloseDialog = () => {
    setIsDialogOpen(false);
    setEditingPermission(null);
    setFormData({
      name: '',
      displayName: '',
      description: '',
      module: '',
      action: '',
      resource: '',
    });
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-96">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
        <span className="ml-2 text-muted-foreground">Loading permissions...</span>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="flex flex-col items-center justify-center h-96 gap-4">
        <AlertCircle className="h-12 w-12 text-destructive" />
        <div className="text-center">
          <p className="text-lg font-semibold text-destructive mb-2">
            Failed to Load Permissions
          </p>
          <p className="text-sm text-muted-foreground mb-4">
            {error.message || 'An error occurred while loading permissions'}
          </p>
        </div>
        <Button onClick={() => refetch()} variant="outline">
          <RefreshCw className="h-4 w-4 mr-2" />
          Retry
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Permissions</h1>
          <p className="text-muted-foreground">Manage system permissions</p>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => refetch()} variant="outline" disabled={isFetching}>
            <RefreshCw className={`h-4 w-4 mr-2 ${isFetching ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogTrigger asChild>
              <Button className="bg-primary hover:bg-primary/90">
                <Plus className="mr-2 h-4 w-4" />
                Add Permission
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[500px]">
              <DialogHeader>
                <DialogTitle>
                  {editingPermission ? 'Edit Permission' : 'Add New Permission'}
                </DialogTitle>
              </DialogHeader>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Permission Name *</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    placeholder="e.g., users.create"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="displayName">Display Name *</Label>
                  <Input
                    id="displayName"
                    value={formData.displayName}
                    onChange={(e) =>
                      setFormData({ ...formData, displayName: e.target.value })
                    }
                    placeholder="e.g., Create Users"
                    required
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="module">Module *</Label>
                    <Select
                      value={formData.module}
                      onValueChange={(value) =>
                        setFormData({ ...formData, module: value })
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select module" />
                      </SelectTrigger>
                      <SelectContent>
                        {/* ✅ Fixed: Explicit type assertion */}
                        {modules
                          .filter((m): m is string => m !== 'All')
                          .map((module: string) => (
                            <SelectItem key={module} value={module}>
                              {module}
                            </SelectItem>
                          ))}
                        <SelectItem value="Users">Users</SelectItem>
                        <SelectItem value="Roles">Roles</SelectItem>
                        <SelectItem value="Permissions">Permissions</SelectItem>
                        <SelectItem value="Students">Students</SelectItem>
                        <SelectItem value="Teachers">Teachers</SelectItem>
                        <SelectItem value="Classes">Classes</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="action">Action *</Label>
                    <Select
                      value={formData.action}
                      onValueChange={(value) =>
                        setFormData({ ...formData, action: value })
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select action" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Create">Create</SelectItem>
                        <SelectItem value="Read">Read</SelectItem>
                        <SelectItem value="Update">Update</SelectItem>
                        <SelectItem value="Delete">Delete</SelectItem>
                        <SelectItem value="View">View</SelectItem>
                        <SelectItem value="Manage">Manage</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="resource">Resource *</Label>
                  <Input
                    id="resource"
                    value={formData.resource}
                    onChange={(e) =>
                      setFormData({ ...formData, resource: e.target.value })
                    }
                    placeholder="e.g., User, Student, Class"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="description">Description</Label>
                  <Input
                    id="description"
                    value={formData.description}
                    onChange={(e) =>
                      setFormData({ ...formData, description: e.target.value })
                    }
                    placeholder="Brief description of the permission"
                  />
                </div>
                <div className="flex justify-end gap-3">
                  <Button type="button" variant="outline" onClick={handleCloseDialog}>
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    disabled={createMutation.isPending || updateMutation.isPending}
                  >
                    {(createMutation.isPending || updateMutation.isPending) && (
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    )}
                    {editingPermission ? 'Update' : 'Create'}
                  </Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
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
        <Card className="border-l-4 border-l-blue-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Lock className="h-4 w-4" />
              System Permissions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-600">
              {stats.systemPermissions}
            </div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-violet-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Key className="h-4 w-4" />
              Custom Permissions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-violet-600">
              {stats.customPermissions}
            </div>
          </CardContent>
        </Card>
        <Card className="border-l-4 border-l-emerald-500">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Filter className="h-4 w-4" />
              Modules
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-emerald-600">{stats.modules}</div>
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
              {isFetching && (
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              )}
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
                  {/* ✅ Fixed: Explicit type assertion */}
                  {modules.map((module: string) => (
                    <SelectItem key={module} value={module}>
                      {module}
                    </SelectItem>
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
                  <TableHead
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort('name')}
                  >
                    <div className="flex items-center">
                      Permission {getSortIcon('name')}
                    </div>
                  </TableHead>
                  <TableHead
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort('displayName')}
                  >
                    <div className="flex items-center">
                      Display Name {getSortIcon('displayName')}
                    </div>
                  </TableHead>
                  <TableHead
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort('module')}
                  >
                    <div className="flex items-center">
                      Module {getSortIcon('module')}
                    </div>
                  </TableHead>
                  <TableHead
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort('action')}
                  >
                    <div className="flex items-center">
                      Action {getSortIcon('action')}
                    </div>
                  </TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedPermissions.length === 0 ? (
                  <TableRow>
                    <TableCell
                      colSpan={6}
                      className="text-center text-muted-foreground py-8"
                    >
                      {searchTerm || moduleFilter !== 'All'
                        ? 'No permissions found matching your filters'
                        : 'No permissions available'}
                    </TableCell>
                  </TableRow>
                ) : (
                  sortedPermissions.map((permission) => (
                    <TableRow key={permission.id}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <code className="px-2 py-1 rounded bg-muted text-sm font-mono">
                            {permission.name}
                          </code>
                          {permission.isSystemPermission && (
                            <Badge
                              variant="outline"
                              className="text-xs bg-blue-500/10 text-blue-600 border-blue-500/30"
                            >
                              System
                            </Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="font-medium">
                        {permission.displayName}
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary">{permission.module}</Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{permission.action}</Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground max-w-xs truncate">
                        {permission.description || 'No description'}
                      </TableCell>
                      <TableCell className="text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end" className="w-40 bg-popover">
                            <DropdownMenuItem
                              onClick={() => handleEdit(permission)}
                              disabled={permission.isSystemPermission}
                            >
                              <Pencil className="h-4 w-4 mr-2" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              className="text-destructive focus:text-destructive"
                              onClick={() => handleDeleteClick(permission.id)}
                              disabled={permission.isSystemPermission}
                            >
                              <Trash2 className="h-4 w-4 mr-2" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between mt-4">
              <p className="text-sm text-muted-foreground">
                Showing page {pageNumber} of {totalPages} ({totalCount} total
                permissions)
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPageNumber((p) => p - 1)}
                  disabled={!hasPreviousPage || isFetching}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPageNumber((p) => p + 1)}
                  disabled={!hasNextPage || isFetching}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog
        open={!!deletePermissionId}
        onOpenChange={() => setDeletePermissionId(null)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the
              permission and remove it from all roles.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleteMutation.isPending}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteConfirm}
              disabled={deleteMutation.isPending}
              className="bg-destructive hover:bg-destructive/90"
            >
              {deleteMutation.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Deleting...
                </>
              ) : (
                'Delete'
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
