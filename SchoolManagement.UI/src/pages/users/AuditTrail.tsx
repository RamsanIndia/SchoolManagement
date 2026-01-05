import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Search,
  Download,
  Filter,
  Eye,
  LogIn,
  LogOut,
  Edit,
  Trash2,
  Plus,
  Shield,
  User,
  Clock,
  Monitor,
  MapPin,
  Activity,
} from "lucide-react";

interface AuditLog {
  id: string;
  timestamp: string;
  user: string;
  userId: string;
  action: string;
  category: string;
  module: string;
  description: string;
  ipAddress: string;
  userAgent: string;
  status: "success" | "failed" | "warning";
  details?: Record<string, unknown>;
}

interface LoginHistory {
  id: string;
  user: string;
  userId: string;
  timestamp: string;
  action: "login" | "logout" | "failed_login";
  ipAddress: string;
  location: string;
  device: string;
  browser: string;
  status: "success" | "failed";
}

const mockAuditLogs: AuditLog[] = [
  {
    id: "1",
    timestamp: "2024-01-15 14:32:45",
    user: "Naveen Kumar",
    userId: "U001",
    action: "UPDATE",
    category: "Data Modification",
    module: "Student Management",
    description: "Updated student record for Rahul Sharma (ST001)",
    ipAddress: "192.168.1.100",
    userAgent: "Chrome 120.0 / Windows 10",
    status: "success",
    details: { studentId: "ST001", fields: ["address", "phone"] },
  },
  {
    id: "2",
    timestamp: "2024-01-15 14:28:12",
    user: "Admin User",
    userId: "U000",
    action: "CREATE",
    category: "User Management",
    module: "Roles",
    description: "Created new role 'Department Head'",
    ipAddress: "192.168.1.50",
    userAgent: "Firefox 121.0 / MacOS",
    status: "success",
    details: { roleId: "R007", permissions: 15 },
  },
  {
    id: "3",
    timestamp: "2024-01-15 14:15:33",
    user: "Priya Sharma",
    userId: "U005",
    action: "DELETE",
    category: "Data Deletion",
    module: "Fee Management",
    description: "Deleted fee record FEE-2024-0123",
    ipAddress: "192.168.1.75",
    userAgent: "Safari 17.0 / iOS",
    status: "warning",
    details: { feeId: "FEE-2024-0123", amount: 5000 },
  },
  {
    id: "4",
    timestamp: "2024-01-15 13:45:20",
    user: "Unknown",
    userId: "U999",
    action: "LOGIN_ATTEMPT",
    category: "Security",
    module: "Authentication",
    description: "Failed login attempt with invalid credentials",
    ipAddress: "203.45.67.89",
    userAgent: "Unknown Browser",
    status: "failed",
  },
  {
    id: "5",
    timestamp: "2024-01-15 13:30:00",
    user: "Rohan Verma",
    userId: "U002",
    action: "EXPORT",
    category: "Data Export",
    module: "Reports",
    description: "Exported attendance report for January 2024",
    ipAddress: "192.168.1.120",
    userAgent: "Chrome 120.0 / Windows 11",
    status: "success",
  },
  {
    id: "6",
    timestamp: "2024-01-15 12:15:45",
    user: "Admin User",
    userId: "U000",
    action: "UPDATE",
    category: "Configuration",
    module: "Settings",
    description: "Updated system notification settings",
    ipAddress: "192.168.1.50",
    userAgent: "Chrome 120.0 / Windows 10",
    status: "success",
  },
];

const mockLoginHistory: LoginHistory[] = [
  {
    id: "1",
    user: "Naveen Kumar",
    userId: "U001",
    timestamp: "2024-01-15 14:30:00",
    action: "login",
    ipAddress: "192.168.1.100",
    location: "Mumbai, India",
    device: "Desktop",
    browser: "Chrome 120.0",
    status: "success",
  },
  {
    id: "2",
    user: "Priya Sharma",
    userId: "U005",
    timestamp: "2024-01-15 14:15:00",
    action: "login",
    ipAddress: "192.168.1.75",
    location: "Delhi, India",
    device: "Mobile",
    browser: "Safari 17.0",
    status: "success",
  },
  {
    id: "3",
    user: "Unknown",
    userId: "U999",
    timestamp: "2024-01-15 13:45:20",
    action: "failed_login",
    ipAddress: "203.45.67.89",
    location: "Unknown",
    device: "Unknown",
    browser: "Unknown",
    status: "failed",
  },
  {
    id: "4",
    user: "Rohan Verma",
    userId: "U002",
    timestamp: "2024-01-15 13:00:00",
    action: "logout",
    ipAddress: "192.168.1.120",
    location: "Bangalore, India",
    device: "Desktop",
    browser: "Chrome 120.0",
    status: "success",
  },
  {
    id: "5",
    user: "Rohan Verma",
    userId: "U002",
    timestamp: "2024-01-15 09:00:00",
    action: "login",
    ipAddress: "192.168.1.120",
    location: "Bangalore, India",
    device: "Desktop",
    browser: "Chrome 120.0",
    status: "success",
  },
  {
    id: "6",
    user: "Admin User",
    userId: "U000",
    timestamp: "2024-01-15 08:30:00",
    action: "login",
    ipAddress: "192.168.1.50",
    location: "Chennai, India",
    device: "Desktop",
    browser: "Firefox 121.0",
    status: "success",
  },
];

export default function AuditTrail() {
  const [searchTerm, setSearchTerm] = useState("");
  const [moduleFilter, setModuleFilter] = useState<string>("all");
  const [actionFilter, setActionFilter] = useState<string>("all");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);

  const filteredLogs = mockAuditLogs.filter((log) => {
    const matchesSearch =
      log.user.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.module.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesModule = moduleFilter === "all" || log.module === moduleFilter;
    const matchesAction = actionFilter === "all" || log.action === actionFilter;
    return matchesSearch && matchesModule && matchesAction;
  });

  const filteredLoginHistory = mockLoginHistory.filter((log) =>
    log.user.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getActionIcon = (action: string) => {
    switch (action) {
      case "CREATE":
        return <Plus className="h-4 w-4 text-green-400" />;
      case "UPDATE":
        return <Edit className="h-4 w-4 text-blue-400" />;
      case "DELETE":
        return <Trash2 className="h-4 w-4 text-red-400" />;
      case "LOGIN_ATTEMPT":
        return <Shield className="h-4 w-4 text-yellow-400" />;
      case "EXPORT":
        return <Download className="h-4 w-4 text-purple-400" />;
      default:
        return <Activity className="h-4 w-4 text-muted-foreground" />;
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "success":
        return <Badge className="bg-green-500/20 text-green-400">Success</Badge>;
      case "failed":
        return <Badge className="bg-red-500/20 text-red-400">Failed</Badge>;
      case "warning":
        return <Badge className="bg-yellow-500/20 text-yellow-400">Warning</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const getLoginActionBadge = (action: string) => {
    switch (action) {
      case "login":
        return (
          <Badge className="bg-green-500/20 text-green-400">
            <LogIn className="h-3 w-3 mr-1" /> Login
          </Badge>
        );
      case "logout":
        return (
          <Badge className="bg-blue-500/20 text-blue-400">
            <LogOut className="h-3 w-3 mr-1" /> Logout
          </Badge>
        );
      case "failed_login":
        return (
          <Badge className="bg-red-500/20 text-red-400">
            <Shield className="h-3 w-3 mr-1" /> Failed
          </Badge>
        );
      default:
        return <Badge variant="secondary">{action}</Badge>;
    }
  };

  const stats = {
    totalActions: mockAuditLogs.length,
    successfulLogins: mockLoginHistory.filter((l) => l.action === "login").length,
    failedLogins: mockLoginHistory.filter((l) => l.action === "failed_login").length,
    activeUsers: new Set(mockLoginHistory.filter((l) => l.action === "login").map((l) => l.userId)).size,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Audit Trail & Activity Logs</h1>
          <p className="text-muted-foreground">
            Track user actions, login history, and system events
          </p>
        </div>
        <Button variant="outline">
          <Download className="h-4 w-4 mr-2" />
          Export Logs
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Actions</p>
                <p className="text-2xl font-bold">{stats.totalActions}</p>
              </div>
              <Activity className="h-8 w-8 text-primary" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Successful Logins</p>
                <p className="text-2xl font-bold text-green-400">{stats.successfulLogins}</p>
              </div>
              <LogIn className="h-8 w-8 text-green-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Failed Logins</p>
                <p className="text-2xl font-bold text-red-400">{stats.failedLogins}</p>
              </div>
              <Shield className="h-8 w-8 text-red-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Active Users</p>
                <p className="text-2xl font-bold text-blue-400">{stats.activeUsers}</p>
              </div>
              <User className="h-8 w-8 text-blue-400" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="activity" className="space-y-4">
        <TabsList>
          <TabsTrigger value="activity">Activity Logs</TabsTrigger>
          <TabsTrigger value="login">Login History</TabsTrigger>
        </TabsList>

        <TabsContent value="activity" className="space-y-4">
          {/* Filters */}
          <Card className="bg-card border-border">
            <CardContent className="pt-6">
              <div className="flex flex-wrap gap-4">
                <div className="relative flex-1 min-w-[200px]">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    placeholder="Search logs..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-9"
                  />
                </div>
                <Select value={moduleFilter} onValueChange={setModuleFilter}>
                  <SelectTrigger className="w-[180px]">
                    <SelectValue placeholder="Module" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Modules</SelectItem>
                    <SelectItem value="Student Management">Student Management</SelectItem>
                    <SelectItem value="Fee Management">Fee Management</SelectItem>
                    <SelectItem value="Roles">Roles</SelectItem>
                    <SelectItem value="Reports">Reports</SelectItem>
                    <SelectItem value="Settings">Settings</SelectItem>
                    <SelectItem value="Authentication">Authentication</SelectItem>
                  </SelectContent>
                </Select>
                <Select value={actionFilter} onValueChange={setActionFilter}>
                  <SelectTrigger className="w-[150px]">
                    <SelectValue placeholder="Action" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Actions</SelectItem>
                    <SelectItem value="CREATE">Create</SelectItem>
                    <SelectItem value="UPDATE">Update</SelectItem>
                    <SelectItem value="DELETE">Delete</SelectItem>
                    <SelectItem value="EXPORT">Export</SelectItem>
                    <SelectItem value="LOGIN_ATTEMPT">Login Attempt</SelectItem>
                  </SelectContent>
                </Select>
                <Input
                  type="date"
                  value={dateFrom}
                  onChange={(e) => setDateFrom(e.target.value)}
                  className="w-[150px]"
                  placeholder="From"
                />
                <Input
                  type="date"
                  value={dateTo}
                  onChange={(e) => setDateTo(e.target.value)}
                  className="w-[150px]"
                  placeholder="To"
                />
              </div>
            </CardContent>
          </Card>

          {/* Activity Logs Table */}
          <Card className="bg-card border-border">
            <CardHeader>
              <CardTitle>Activity Logs</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Timestamp</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>Module</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead>IP Address</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Details</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredLogs.map((log) => (
                    <TableRow key={log.id}>
                      <TableCell className="text-sm">
                        <div className="flex items-center gap-2">
                          <Clock className="h-4 w-4 text-muted-foreground" />
                          {log.timestamp}
                        </div>
                      </TableCell>
                      <TableCell className="font-medium">{log.user}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {getActionIcon(log.action)}
                          <span>{log.action}</span>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{log.module}</Badge>
                      </TableCell>
                      <TableCell className="max-w-[200px] truncate">
                        {log.description}
                      </TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {log.ipAddress}
                      </TableCell>
                      <TableCell>{getStatusBadge(log.status)}</TableCell>
                      <TableCell>
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() => {
                            setSelectedLog(log);
                            setIsDetailDialogOpen(true);
                          }}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="login" className="space-y-4">
          {/* Login History Table */}
          <Card className="bg-card border-border">
            <CardHeader>
              <CardTitle>Login History</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Timestamp</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>IP Address</TableHead>
                    <TableHead>Location</TableHead>
                    <TableHead>Device</TableHead>
                    <TableHead>Browser</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredLoginHistory.map((log) => (
                    <TableRow key={log.id}>
                      <TableCell className="text-sm">
                        <div className="flex items-center gap-2">
                          <Clock className="h-4 w-4 text-muted-foreground" />
                          {log.timestamp}
                        </div>
                      </TableCell>
                      <TableCell className="font-medium">{log.user}</TableCell>
                      <TableCell>{getLoginActionBadge(log.action)}</TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {log.ipAddress}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1">
                          <MapPin className="h-3 w-3 text-muted-foreground" />
                          {log.location}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1">
                          <Monitor className="h-3 w-3 text-muted-foreground" />
                          {log.device}
                        </div>
                      </TableCell>
                      <TableCell className="text-sm">{log.browser}</TableCell>
                      <TableCell>{getStatusBadge(log.status)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Detail Dialog */}
      <Dialog open={isDetailDialogOpen} onOpenChange={setIsDetailDialogOpen}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Log Details</DialogTitle>
          </DialogHeader>
          {selectedLog && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">Timestamp:</span>
                  <p className="font-medium">{selectedLog.timestamp}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">User:</span>
                  <p className="font-medium">{selectedLog.user}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Action:</span>
                  <p className="font-medium">{selectedLog.action}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Module:</span>
                  <p className="font-medium">{selectedLog.module}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Category:</span>
                  <p className="font-medium">{selectedLog.category}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Status:</span>
                  <p>{getStatusBadge(selectedLog.status)}</p>
                </div>
                <div className="col-span-2">
                  <span className="text-muted-foreground">Description:</span>
                  <p className="font-medium">{selectedLog.description}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">IP Address:</span>
                  <p className="font-medium">{selectedLog.ipAddress}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">User Agent:</span>
                  <p className="font-medium text-xs">{selectedLog.userAgent}</p>
                </div>
              </div>
              {selectedLog.details && (
                <div>
                  <span className="text-muted-foreground">Additional Details:</span>
                  <pre className="mt-2 p-3 bg-muted rounded-lg text-xs overflow-auto">
                    {JSON.stringify(selectedLog.details, null, 2)}
                  </pre>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}