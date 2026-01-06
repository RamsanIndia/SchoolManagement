import { SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { AppSidebar } from "@/components/sidebar"; // Changed from "./Sidebar"
import { useAuth, useUserFullName } from "@/contexts/AuthContext";
import { Bell } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { useNavigate } from "react-router-dom";

export function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const fullName = useUserFullName();
  const navigate = useNavigate();

  if (!user) return null;

  const firstName = user.firstName || user.email.split('@')[0];
  const initials = `${user.firstName?.[0] || ''}${user.lastName?.[0] || ''}`.toUpperCase() || user.email[0].toUpperCase();
  const primaryRole = user.roles[0] || 'User';
  const avatarUrl = (user as any).avatar as string | undefined;

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <SidebarProvider>
      <div className="flex min-h-screen w-full bg-gradient-to-br from-background via-muted/30 to-brand-primary/5">
        <AppSidebar />
        
        <div className="flex-1 flex flex-col">
          <header className="h-16 bg-gradient-to-r from-card/95 to-background/95 backdrop-blur-sm border-b border-border/50 flex items-center justify-between px-6 relative">
            <div className="absolute inset-0 bg-gradient-to-r from-brand-primary/5 via-transparent to-brand-accent/5 pointer-events-none" />
            
            <div className="flex items-center space-x-4 relative z-10">
              <SidebarTrigger className="glow-on-hover" />
              <div>
                <h1 className="text-xl font-bold bg-gradient-primary bg-clip-text text-transparent">
                  Welcome back, {firstName}
                </h1>
                <div className="flex items-center gap-2">
                  <p className="text-sm text-muted-foreground font-medium">
                    {primaryRole} Dashboard
                  </p>
                  {user.isEmailVerified && (
                    <Badge variant="secondary" className="text-xs">
                      ✓ Verified
                    </Badge>
                  )}
                </div>
              </div>
            </div>

            <div className="flex items-center space-x-3 relative z-10">
              <Button 
                variant="ghost" 
                size="sm" 
                className="glow-on-hover relative"
                onClick={() => navigate('/notifications')}
              >
                <Bell className="h-4 w-4" />
                <span className="absolute -top-1 -right-1 h-3 w-3 bg-education-orange rounded-full border-2 border-background animate-pulse-glow" />
              </Button>
              
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="sm" className="glow-on-hover">
                    <Avatar className="h-8 w-8">
                      {avatarUrl && <AvatarImage src={avatarUrl} alt={fullName} />}
                      <AvatarFallback className="bg-gradient-primary text-white font-semibold text-sm">
                        {initials}
                      </AvatarFallback>
                    </Avatar>
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <DropdownMenuLabel>
                    <div className="flex flex-col space-y-1">
                      <p className="text-sm font-medium leading-none">{fullName}</p>
                      <p className="text-xs leading-none text-muted-foreground">{user.email}</p>
                    </div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={() => navigate(`/users/profile/${user.id}`)}>
                    Profile
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => navigate('/settings')}>
                    Settings
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout} className="text-destructive">
                    Logout
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </header>

          <main className="flex-1 p-6 relative overflow-auto">
            <div className="absolute inset-0 opacity-30 bg-[radial-gradient(circle_at_50%_50%,_hsl(var(--brand-primary)/0.05)_0%,_transparent_50%)] pointer-events-none" />
            <div className="relative z-10">
              {children}
            </div>
          </main>
        </div>
      </div>
    </SidebarProvider>
  );
}
