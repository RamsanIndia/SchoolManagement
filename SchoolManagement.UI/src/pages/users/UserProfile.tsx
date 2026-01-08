// src/users/pages/UserProfile.tsx
import { useNavigate, useParams } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
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
  KeyRound, Pencil, Monitor, LogOut, Check, MapPin, Globe, Loader2,
  AlertCircle
} from "lucide-react";
import { userApi } from "@/users/api/userApi";
import { ApiError } from "@/lib/api/apiError";
import type { UserDetails, UserPermissions, ActivityLog, UserSession, UserStatistics } from "@/users/types/user.types";


export default function UserProfile() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { toast } = useToast();
  const queryClient = useQueryClient();


  // ==================== QUERIES ====================


  // Fetch user data
  const { 
    data: user, 
    isLoading: userLoading, 
    error: userError 
  } = useQuery<UserDetails>({
    queryKey: ['user', id],
    queryFn: () => userApi.getCurrentUser(),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });


  // Fetch user permissions (optional - may not exist yet)
  const { 
    data: permissions, 
    isLoading: permissionsLoading,
    isError: permissionsError 
  } = useQuery<UserPermissions>({
    queryKey: ['user', id, 'permissions'],
    queryFn: () => userApi.getPermissions(id!),
    enabled: !!id,
    staleTime: 10 * 60 * 1000,
    retry: false, // ✅ Don't retry on 404
    throwOnError: false, // ✅ Don't throw on error
  });


  // Fetch activity log (optional - may not exist yet)
  const { 
    data: activityLog, 
    isLoading: activityLoading,
    isError: activityError 
  } = useQuery<ActivityLog[]>({
    queryKey: ['user', id, 'activity'],
    queryFn: () => userApi.getActivityLog(id!),
    enabled: !!id,
    staleTime: 2 * 60 * 1000,
    retry: false, // ✅ Don't retry on 404
    throwOnError: false, // ✅ Don't throw on error
  });


  // Fetch sessions (optional - may not exist yet)
  const { 
    data: sessions, 
    isLoading: sessionsLoading,
    isError: sessionsError 
  } = useQuery<UserSession[]>({
    queryKey: ['user', id, 'sessions'],
    queryFn: () => userApi.getSessions(id!),
    enabled: !!id,
    refetchInterval: false, // ✅ Disable auto-refetch until endpoint exists
    retry: false, // ✅ Don't retry on 404
    throwOnError: false, // ✅ Don't throw on error
  });


  // Fetch statistics (optional - may not exist yet)
  const { 
    data: statistics,
    isError: statisticsError 
  } = useQuery<UserStatistics>({
    queryKey: ['user', id, 'statistics'],
    queryFn: () => userApi.getStatistics(id!),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    retry: false, // ✅ Don't retry on 404
    throwOnError: false, // ✅ Don't throw on error
  });


  // ==================== MUTATIONS ====================


  // Reset password mutation
  const resetPasswordMutation = useMutation({
    mutationFn: () => userApi.adminResetPassword(id!),
    onSuccess: () => {
      toast({
        title: "Password Reset",
        description: "Password reset link has been sent to user's email",
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: "Error",
        description: error.getUserMessage(),
        variant: "destructive",
      });
    },
  });


  // Terminate session mutation
  const terminateSessionMutation = useMutation({
    mutationFn: (sessionId: string) => userApi.terminateSession(id!, sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user', id, 'sessions'] });
      toast({
        title: "Session Terminated",
        description: "The session has been terminated successfully",
      });
    },
    onError: (error: ApiError) => {
      toast({
        title: "Error",
        description: error.getUserMessage(),
        variant: "destructive",
      });
    },
  });


  // ==================== HANDLERS ====================


  const handleResetPassword = () => {
    if (id) {
      resetPasswordMutation.mutate();
    }
  };


  const handleTerminateSession = (sessionId: string) => {
    terminateSessionMutation.mutate(sessionId);
  };


  // ==================== UTILITY FUNCTIONS ====================


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
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;
    if (diffMins < 1440) return `${Math.floor(diffMins / 60)} hr ago`;
    return `${Math.floor(diffMins / 1440)} days ago`;
  };


  const getStatusBadge = (status: 'active' | 'inactive' | 'locked') => {
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


  // ==================== LOADING STATE ====================


  if (userLoading) {
    return (
      <div className="flex items-center justify-center h-[calc(100vh-200px)]">
        <div className="text-center space-y-4">
          <Loader2 className="h-12 w-12 animate-spin mx-auto text-primary" />
          <p className="text-muted-foreground">Loading user profile...</p>
        </div>
      </div>
    );
  }


  // ==================== ERROR STATE ====================


  if (userError || !user) {
    return (
      <div className="flex flex-col items-center justify-center h-[calc(100vh-200px)] gap-4">
        <AlertCircle className="h-12 w-12 text-destructive" />
        <div className="text-center space-y-2">
          <h2 className="text-xl font-semibold">Failed to load user profile</h2>
          <p className="text-muted-foreground">
            {userError instanceof ApiError 
              ? userError.getUserMessage() 
              : 'An unexpected error occurred'}
          </p>
        </div>
        <Button onClick={() => navigate('/users')}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Users
        </Button>
      </div>
    );
  }
  // ==================== RENDER ====================
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
        <Button 
          variant="outline" 
          onClick={handleResetPassword}
          disabled={resetPasswordMutation.isPending}
        >
          {resetPasswordMutation.isPending ? (
            <Loader2 className="h-4 w-4 mr-2 animate-spin" />
          ) : (
            <KeyRound className="h-4 w-4 mr-2" />
          )}
          Reset Password
        </Button>
      </div>


      {/* Profile Header Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage src={user?.profilePhoto} alt={user?.fullName} />
              <AvatarFallback className="text-2xl">
                {user?.firstName?.[0]}{user?.lastName?.[0]}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1 space-y-2">
              <div className="flex items-center gap-3">
                <h2 className="text-2xl font-bold">{user.fullName}</h2>
                {getStatusBadge(user.status)}
              </div>
              <div className="flex flex-wrap items-center gap-4 text-muted-foreground">
                <span className="flex items-center gap-1">
                  <Mail className="h-4 w-4" />
                  {user.email}
                </span>
                <span className="flex items-center gap-1">
                  <Phone className="h-4 w-4" />
                  {user.phoneNumber}
                </span>
                {user.department && (
                  <span className="flex items-center gap-1">
                    <Building className="h-4 w-4" />
                    {user.department}
                  </span>
                )}
              </div>
              <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
                <Badge variant="outline" className="font-normal">
                  <Shield className="h-3 w-3 mr-1" />
                  {user.roles.join(', ')}
                </Badge>
                {user.lastLoginAt && (
                  <span className="flex items-center gap-1">
                    <Clock className="h-4 w-4" />
                    Last login: {getRelativeTime(user.lastLoginAt)}
                  </span>
                )}
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
                    <p className="font-medium">{user.fullName}</p>
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
                    <p className="text-sm text-muted-foreground">Phone</p>
                    <p className="font-medium">{user.phoneNumber}</p>
                  </div>
                  {user.department && (
                    <div>
                      <p className="text-sm text-muted-foreground">Department</p>
                      <p className="font-medium">{user.department}</p>
                    </div>
                  )}
                  {user.location && (
                    <div>
                      <p className="text-sm text-muted-foreground">Location</p>
                      <p className="font-medium">{user.location}</p>
                    </div>
                  )}
                  <div>
                    <p className="text-sm text-muted-foreground">Email Verified</p>
                    <p className="font-medium">
                      {user.isEmailVerified ? (
                        <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                          <Check className="h-3 w-3 mr-1" />
                          Verified
                        </Badge>
                      ) : (
                        <Badge variant="outline" className="bg-amber-50 text-amber-700 border-amber-200">
                          Pending
                        </Badge>
                      )}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Phone Verified</p>
                    <p className="font-medium">
                      {user.isPhoneVerified ? (
                        <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
                          <Check className="h-3 w-3 mr-1" />
                          Verified
                        </Badge>
                      ) : (
                        <Badge variant="outline" className="bg-amber-50 text-amber-700 border-amber-200">
                          Pending
                        </Badge>
                      )}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>


            <Card>
              <CardHeader>
                <CardTitle>Account Statistics</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {statisticsError ? (
                  <div className="text-center py-8 space-y-2">
                    <AlertCircle className="h-8 w-8 mx-auto text-muted-foreground" />
                    <p className="text-sm text-muted-foreground">Statistics not available</p>
                  </div>
                ) : statistics ? (
                  <div className="grid grid-cols-2 gap-4">
                    <div className="p-4 rounded-lg bg-muted/50">
                      <p className="text-2xl font-bold">{statistics.totalPermissions}</p>
                      <p className="text-sm text-muted-foreground">Permissions</p>
                    </div>
                    <div className="p-4 rounded-lg bg-muted/50">
                      <p className="text-2xl font-bold">{statistics.actionsToday}</p>
                      <p className="text-sm text-muted-foreground">Actions Today</p>
                    </div>
                    <div className="p-4 rounded-lg bg-muted/50">
                      <p className="text-2xl font-bold">{statistics.activeSessions}</p>
                      <p className="text-sm text-muted-foreground">Active Sessions</p>
                    </div>
                    <div className="p-4 rounded-lg bg-muted/50">
                      <p className="text-2xl font-bold">{statistics.loginSuccessRate}%</p>
                      <p className="text-sm text-muted-foreground">Login Success</p>
                    </div>
                  </div>
                ) : (
                  <div className="flex justify-center p-8">
                    <Loader2 className="h-6 w-6 animate-spin" />
                  </div>
                )}
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
              {permissionsLoading ? (
                <div className="flex justify-center p-8">
                  <Loader2 className="h-6 w-6 animate-spin" />
                </div>
              ) : permissionsError ? (
                <div className="text-center py-8 space-y-2">
                  <AlertCircle className="h-8 w-8 mx-auto text-muted-foreground" />
                  <p className="text-muted-foreground">Permissions endpoint not available</p>
                </div>
              ) : permissions && Object.keys(permissions).length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {Object.entries(permissions).map(([category, perms]) => (
                    <div key={category} className="space-y-3">
                      <h3 className="font-semibold text-sm uppercase tracking-wide text-muted-foreground">
                        {category}
                      </h3>
                      <div className="space-y-2">
                        {perms.map((permission: string) => (
                          <div key={permission} className="flex items-center gap-2 text-sm">
                            <Check className="h-4 w-4 text-emerald-500 flex-shrink-0" />
                            <span>{permission}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-muted-foreground text-center py-8">No permissions found</p>
              )}
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
              {activityLoading ? (
                <div className="flex justify-center p-8">
                  <Loader2 className="h-6 w-6 animate-spin" />
                </div>
              ) : activityError ? (
                <div className="text-center py-8 space-y-2">
                  <AlertCircle className="h-8 w-8 mx-auto text-muted-foreground" />
                  <p className="text-muted-foreground">Activity log endpoint not available</p>
                </div>
              ) : activityLog && activityLog.length > 0 ? (
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
                    {activityLog.map((log) => (
                      <TableRow key={log.id}>
                        <TableCell className="font-medium">{log.action}</TableCell>
                        <TableCell>{formatDateTime(log.timestamp)}</TableCell>
                        <TableCell className="text-muted-foreground">{log.ipAddress}</TableCell>
                        <TableCell className="text-muted-foreground">{log.device}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <p className="text-muted-foreground text-center py-8">No activity logs found</p>
              )}
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
              {sessionsLoading ? (
                <div className="flex justify-center p-8">
                  <Loader2 className="h-6 w-6 animate-spin" />
                </div>
              ) : sessionsError ? (
                <div className="text-center py-8 space-y-2">
                  <AlertCircle className="h-8 w-8 mx-auto text-muted-foreground" />
                  <p className="text-muted-foreground">Sessions endpoint not available</p>
                </div>
              ) : sessions && sessions.length > 0 ? (
                sessions.map((session) => (
                  <div key={session.id} className="flex items-center justify-between p-4 rounded-lg border">
                    <div className="flex items-center gap-4">
                      <div className="p-2 rounded-lg bg-muted">
                        <Monitor className="h-5 w-5" />
                      </div>
                      <div>
                        <div className="flex items-center gap-2">
                          <p className="font-medium">{session.device}</p>
                          {session.isCurrent && (
                            <Badge className="bg-emerald-500/15 text-emerald-600 border-emerald-500/30 text-xs">
                              Current
                            </Badge>
                          )}
                        </div>
                        <div className="flex items-center gap-3 text-sm text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <Globe className="h-3 w-3" />
                            {session.ipAddress}
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
                    {!session.isCurrent && (
                      <Button 
                        variant="outline" 
                        size="sm"
                        className="text-destructive hover:bg-destructive hover:text-destructive-foreground"
                        onClick={() => handleTerminateSession(session.id)}
                        disabled={terminateSessionMutation.isPending}
                      >
                        {terminateSessionMutation.isPending ? (
                          <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        ) : (
                          <LogOut className="h-4 w-4 mr-2" />
                        )}
                        Terminate
                      </Button>
                    )}
                  </div>
                ))
              ) : (
                <p className="text-muted-foreground text-center py-8">No active sessions</p>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
