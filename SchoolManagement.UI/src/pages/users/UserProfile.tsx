import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useToast } from "@/hooks/use-toast";
import { 
  ArrowLeft, Mail, Phone, Building, Calendar, Clock, Shield,
  KeyRound, Pencil, Monitor, LogOut, Check, X, MapPin, Globe
} from "lucide-react";

const mockUser = {
  id: "1",
  photo: "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150",
  firstName: "Naveen",
  lastName: "Kumar",
  username: "naveen.k",
  email: "naveen@example.com",
  mobile: "+91 98765 43210",
  role: "Admin",
  department: "Administration",
  status: "active",
  lastLogin: "2024-12-04T10:30:00",
  createdAt: "2024-01-15",
  location: "Mumbai, India",
};

const mockPermissions = {
  "User Management": ["View Users", "Create Users", "Edit Users", "Delete Users"],
  "Student Management": ["View Students", "Create Students", "Edit Students", "Export Students"],
  "Fee Management": ["View Fees", "Collect Fees", "Approve Fees", "Generate Reports"],
  "Attendance": ["View Attendance", "Mark Attendance", "Edit Attendance"],
  "Examinations": ["View Exams", "Create Exams", "Manage Results"],
};

const mockActivityLog = [
  { id: "1", action: "Logged in", timestamp: "2024-12-04T10:30:00", ip: "192.168.1.100", device: "Chrome / Windows" },
  { id: "2", action: "Updated student record #ST-2024-001", timestamp: "2024-12-04T10:15:00", ip: "192.168.1.100", device: "Chrome / Windows" },
  { id: "3", action: "Generated fee report", timestamp: "2024-12-04T09:45:00", ip: "192.168.1.100", device: "Chrome / Windows" },
  { id: "4", action: "Logged in", timestamp: "2024-12-03T14:20:00", ip: "192.168.1.105", device: "Safari / MacOS" },
  { id: "5", action: "Reset password for user rohan.v", timestamp: "2024-12-03T11:30:00", ip: "192.168.1.105", device: "Safari / MacOS" },
];

const mockSessions = [
  { id: "1", device: "Chrome / Windows 11", ip: "192.168.1.100", location: "Mumbai, India", lastActive: "2024-12-04T10:30:00", current: true },
  { id: "2", device: "Safari / MacOS", ip: "192.168.1.105", location: "Mumbai, India", lastActive: "2024-12-03T14:20:00", current: false },
  { id: "3", device: "Mobile App / iOS", ip: "192.168.1.110", location: "Mumbai, India", lastActive: "2024-12-02T09:15:00", current: false },
];

export default function UserProfile() {
  const navigate = useNavigate();
  const { id } = useParams();
  const { toast } = useToast();
  const [user] = useState(mockUser);

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-IN', {
      dateStyle: 'medium',
      timeStyle: 'short'
    });
  };

  const getRelativeTime = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 60) return `${diffMins} min ago`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)} hr ago`;
    return `${Math.floor(diffMins / 1440)} days ago`;
  };

  const handleResetPassword = () => {
    toast({ title: "Password Reset", description: "Password reset link has been sent to user's email" });
  };

  const handleTerminateSession = (sessionId: string) => {
    toast({ title: "Session Terminated", description: "The session has been terminated successfully" });
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "active":
        return <Badge className="bg-emerald-500/15 text-emerald-600 border-emerald-500/30">Active</Badge>;
      case "inactive":
        return <Badge className="bg-amber-500/15 text-amber-600 border-amber-500/30">Inactive</Badge>;
      case "locked":
        return <Badge className="bg-red-500/15 text-red-600 border-red-500/30">Locked</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/users")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold text-foreground">User Profile</h1>
          <p className="text-muted-foreground">View and manage user details</p>
        </div>
        <Button variant="outline" onClick={() => navigate(`/users/edit/${id}`)}>
          <Pencil className="h-4 w-4 mr-2" />
          Edit User
        </Button>
        <Button variant="outline" onClick={handleResetPassword}>
          <KeyRound className="h-4 w-4 mr-2" />
          Reset Password
        </Button>
      </div>

      {/* Profile Header Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage src={user.photo} alt={`${user.firstName} ${user.lastName}`} />
              <AvatarFallback className="text-2xl">{user.firstName[0]}{user.lastName[0]}</AvatarFallback>
            </Avatar>
            <div className="flex-1 space-y-2">
              <div className="flex items-center gap-3">
                <h2 className="text-2xl font-bold">{user.firstName} {user.lastName}</h2>
                {getStatusBadge(user.status)}
              </div>
              <div className="flex flex-wrap items-center gap-4 text-muted-foreground">
                <span className="flex items-center gap-1">
                  <Mail className="h-4 w-4" />
                  {user.email}
                </span>
                <span className="flex items-center gap-1">
                  <Phone className="h-4 w-4" />
                  {user.mobile}
                </span>
                <span className="flex items-center gap-1">
                  <Building className="h-4 w-4" />
                  {user.department}
                </span>
              </div>
              <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
                <Badge variant="outline" className="font-normal">
                  <Shield className="h-3 w-3 mr-1" />
                  {user.role}
                </Badge>
                <span className="flex items-center gap-1">
                  <Clock className="h-4 w-4" />
                  Last login: {getRelativeTime(user.lastLogin)}
                </span>
                <span className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  Joined: {new Date(user.createdAt).toLocaleDateString()}
                </span>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Tabs */}
      <Tabs defaultValue="overview" className="space-y-6">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="permissions">Permissions</TabsTrigger>
          <TabsTrigger value="activity">Activity Log</TabsTrigger>
          <TabsTrigger value="sessions">Sessions</TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Personal Information</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Full Name</p>
                    <p className="font-medium">{user.firstName} {user.lastName}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Username</p>
                    <p className="font-medium">@{user.username}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Email</p>
                    <p className="font-medium">{user.email}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Mobile</p>
                    <p className="font-medium">{user.mobile}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Department</p>
                    <p className="font-medium">{user.department}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Location</p>
                    <p className="font-medium">{user.location}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Account Statistics</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="p-4 rounded-lg bg-muted/50">
                    <p className="text-2xl font-bold">24</p>
                    <p className="text-sm text-muted-foreground">Permissions</p>
                  </div>
                  <div className="p-4 rounded-lg bg-muted/50">
                    <p className="text-2xl font-bold">156</p>
                    <p className="text-sm text-muted-foreground">Actions Today</p>
                  </div>
                  <div className="p-4 rounded-lg bg-muted/50">
                    <p className="text-2xl font-bold">3</p>
                    <p className="text-sm text-muted-foreground">Active Sessions</p>
                  </div>
                  <div className="p-4 rounded-lg bg-muted/50">
                    <p className="text-2xl font-bold">98%</p>
                    <p className="text-sm text-muted-foreground">Login Success</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Permissions Tab */}
        <TabsContent value="permissions">
          <Card>
            <CardHeader>
              <CardTitle>Effective Permissions</CardTitle>
              <CardDescription>Combined permissions from role and individual overrides</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {Object.entries(mockPermissions).map(([category, permissions]) => (
                  <div key={category} className="space-y-3">
                    <h3 className="font-semibold text-sm uppercase tracking-wide text-muted-foreground">
                      {category}
                    </h3>
                    <div className="space-y-2">
                      {permissions.map(permission => (
                        <div key={permission} className="flex items-center gap-2 text-sm">
                          <Check className="h-4 w-4 text-emerald-500" />
                          <span>{permission}</span>
                          <Badge variant="outline" className="text-xs ml-auto">From Role</Badge>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Activity Log Tab */}
        <TabsContent value="activity">
          <Card>
            <CardHeader>
              <CardTitle>Recent Activity</CardTitle>
              <CardDescription>User actions and login history</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Action</TableHead>
                    <TableHead>Timestamp</TableHead>
                    <TableHead>IP Address</TableHead>
                    <TableHead>Device</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mockActivityLog.map(log => (
                    <TableRow key={log.id}>
                      <TableCell className="font-medium">{log.action}</TableCell>
                      <TableCell>{formatDateTime(log.timestamp)}</TableCell>
                      <TableCell className="text-muted-foreground">{log.ip}</TableCell>
                      <TableCell className="text-muted-foreground">{log.device}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Sessions Tab */}
        <TabsContent value="sessions">
          <Card>
            <CardHeader>
              <CardTitle>Active Sessions</CardTitle>
              <CardDescription>Devices currently logged into this account</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {mockSessions.map(session => (
                <div key={session.id} className="flex items-center justify-between p-4 rounded-lg border">
                  <div className="flex items-center gap-4">
                    <div className="p-2 rounded-lg bg-muted">
                      <Monitor className="h-5 w-5" />
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <p className="font-medium">{session.device}</p>
                        {session.current && (
                          <Badge className="bg-emerald-500/15 text-emerald-600 border-emerald-500/30 text-xs">
                            Current
                          </Badge>
                        )}
                      </div>
                      <div className="flex items-center gap-3 text-sm text-muted-foreground">
                        <span className="flex items-center gap-1">
                          <Globe className="h-3 w-3" />
                          {session.ip}
                        </span>
                        <span className="flex items-center gap-1">
                          <MapPin className="h-3 w-3" />
                          {session.location}
                        </span>
                        <span className="flex items-center gap-1">
                          <Clock className="h-3 w-3" />
                          {getRelativeTime(session.lastActive)}
                        </span>
                      </div>
                    </div>
                  </div>
                  {!session.current && (
                    <Button 
                      variant="outline" 
                      size="sm"
                      className="text-destructive hover:bg-destructive hover:text-destructive-foreground"
                      onClick={() => handleTerminateSession(session.id)}
                    >
                      <LogOut className="h-4 w-4 mr-2" />
                      Terminate
                    </Button>
                  )}
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
