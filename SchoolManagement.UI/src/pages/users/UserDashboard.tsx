import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Users, Shield, Key, Menu, UserPlus, Settings, ArrowRight,
  TrendingUp, UserCheck, UserX, Lock, Clock, Activity
} from "lucide-react";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from "recharts";

const usersByRole = [
  { role: "Admin", count: 3 },
  { role: "Teacher", count: 25 },
  { role: "Accountant", count: 5 },
  { role: "Librarian", count: 2 },
  { role: "Transport", count: 3 },
  { role: "Receptionist", count: 4 },
];

const userStatusData = [
  { name: "Active", value: 38, color: "#10b981" },
  { name: "Inactive", value: 3, color: "#f59e0b" },
  { name: "Locked", value: 1, color: "#ef4444" },
];

const recentUsers = [
  { id: "1", name: "Sunita Rao", email: "sunita@example.com", role: "Receptionist", photo: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150", createdAt: "2024-07-22" },
  { id: "2", name: "Rajesh Gupta", email: "rajesh@example.com", role: "Transport Manager", photo: "https://images.unsplash.com/photo-1519345182560-3f2917c472ef?w=150", createdAt: "2024-06-18" },
  { id: "3", name: "Priya Patel", email: "priya@example.com", role: "Teacher", photo: "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150", createdAt: "2024-05-12" },
];

const recentActivity = [
  { action: "New user created", user: "Sunita Rao", time: "2 hours ago" },
  { action: "Role updated", user: "Rohan Verma", time: "5 hours ago" },
  { action: "Password reset", user: "Neha Singh", time: "1 day ago" },
  { action: "User locked", user: "Amit Sharma", time: "2 days ago" },
  { action: "Permissions modified", user: "Admin", time: "3 days ago" },
];

export default function UserDashboard() {
  const navigate = useNavigate();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">User Management</h1>
          <p className="text-muted-foreground">Overview of users, roles, and permissions</p>
        </div>
        <Button onClick={() => navigate("/users/add")} className="bg-primary hover:bg-primary/90">
          <UserPlus className="mr-2 h-4 w-4" />
          Add User
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate("/users")}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Users className="h-4 w-4" />
              Total Users
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div className="text-2xl font-bold">42</div>
              <Badge className="bg-emerald-500/15 text-emerald-600 border-emerald-500/30">
                <TrendingUp className="h-3 w-3 mr-1" />
                +3 this month
              </Badge>
            </div>
          </CardContent>
        </Card>
        <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate("/roles")}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Shield className="h-4 w-4" />
              Total Roles
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div className="text-2xl font-bold">6</div>
              <Badge variant="outline">3 System, 3 Custom</Badge>
            </div>
          </CardContent>
        </Card>
        <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate("/permissions")}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Key className="h-4 w-4" />
              Total Permissions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div className="text-2xl font-bold">45</div>
              <Badge variant="outline">10 Modules</Badge>
            </div>
          </CardContent>
        </Card>
        <Card className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => navigate("/users")}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Activity className="h-4 w-4" />
              Active Sessions
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-between">
              <div className="text-2xl font-bold">28</div>
              <Badge className="bg-blue-500/15 text-blue-600 border-blue-500/30">
                <Clock className="h-3 w-3 mr-1" />
                Live
              </Badge>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card className="cursor-pointer hover:shadow-md transition-all hover:scale-[1.02]" onClick={() => navigate("/users")}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-lg bg-primary/10">
                <Users className="h-6 w-6 text-primary" />
              </div>
              <div>
                <p className="font-semibold">Manage Users</p>
                <p className="text-sm text-muted-foreground">View and edit users</p>
              </div>
              <ArrowRight className="h-5 w-5 ml-auto text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
        <Card className="cursor-pointer hover:shadow-md transition-all hover:scale-[1.02]" onClick={() => navigate("/roles")}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-lg bg-violet-500/10">
                <Shield className="h-6 w-6 text-violet-500" />
              </div>
              <div>
                <p className="font-semibold">Manage Roles</p>
                <p className="text-sm text-muted-foreground">Configure roles</p>
              </div>
              <ArrowRight className="h-5 w-5 ml-auto text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
        <Card className="cursor-pointer hover:shadow-md transition-all hover:scale-[1.02]" onClick={() => navigate("/permission-matrix")}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-lg bg-amber-500/10">
                <Key className="h-6 w-6 text-amber-500" />
              </div>
              <div>
                <p className="font-semibold">Permission Matrix</p>
                <p className="text-sm text-muted-foreground">Role permissions</p>
              </div>
              <ArrowRight className="h-5 w-5 ml-auto text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
        <Card className="cursor-pointer hover:shadow-md transition-all hover:scale-[1.02]" onClick={() => navigate("/menu-access")}>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-lg bg-emerald-500/10">
                <Menu className="h-6 w-6 text-emerald-500" />
              </div>
              <div>
                <p className="font-semibold">Menu Access</p>
                <p className="text-sm text-muted-foreground">Control visibility</p>
              </div>
              <ArrowRight className="h-5 w-5 ml-auto text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Charts Row */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Users by Role */}
        <Card>
          <CardHeader>
            <CardTitle>Users by Role</CardTitle>
            <CardDescription>Distribution of users across different roles</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={usersByRole}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis dataKey="role" className="text-xs" />
                <YAxis className="text-xs" />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: 'hsl(var(--popover))', 
                    borderColor: 'hsl(var(--border))',
                    borderRadius: '8px'
                  }} 
                />
                <Bar dataKey="count" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* User Status */}
        <Card>
          <CardHeader>
            <CardTitle>User Status</CardTitle>
            <CardDescription>Current status of all users</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex items-center justify-center">
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={userStatusData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={5}
                    dataKey="value"
                  >
                    {userStatusData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <div className="flex justify-center gap-6 mt-4">
              {userStatusData.map((item) => (
                <div key={item.name} className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full" style={{ backgroundColor: item.color }} />
                  <span className="text-sm text-muted-foreground">{item.name}: {item.value}</span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent Users & Activity */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Recently Added Users */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Recently Added Users</CardTitle>
              <Button variant="ghost" size="sm" onClick={() => navigate("/users")}>
                View All
                <ArrowRight className="h-4 w-4 ml-1" />
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {recentUsers.map(user => (
              <div key={user.id} className="flex items-center gap-4 p-3 rounded-lg hover:bg-muted/50 transition-colors">
                <Avatar>
                  <AvatarImage src={user.photo} alt={user.name} />
                  <AvatarFallback>{user.name.split(' ').map(n => n[0]).join('')}</AvatarFallback>
                </Avatar>
                <div className="flex-1">
                  <p className="font-medium">{user.name}</p>
                  <p className="text-sm text-muted-foreground">{user.email}</p>
                </div>
                <Badge variant="outline">{user.role}</Badge>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Recent Activity */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {recentActivity.map((activity, index) => (
              <div key={index} className="flex items-start gap-4 p-3 rounded-lg hover:bg-muted/50 transition-colors">
                <div className="p-2 rounded-full bg-muted">
                  <Activity className="h-4 w-4 text-muted-foreground" />
                </div>
                <div className="flex-1">
                  <p className="font-medium">{activity.action}</p>
                  <p className="text-sm text-muted-foreground">{activity.user}</p>
                </div>
                <span className="text-xs text-muted-foreground">{activity.time}</span>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
