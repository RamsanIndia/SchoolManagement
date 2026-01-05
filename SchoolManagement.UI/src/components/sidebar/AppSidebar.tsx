import { memo } from "react";
import { GraduationCap, AlertCircle, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { useAuth } from "@/contexts/AuthContext";
import { useNavigation } from "@/contexts/NavigationContext";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  useSidebar,
} from "@/components/ui/sidebar";
import { SidebarNavItem } from "./SidebarNavItem";
import { SidebarUserProfile } from "./SidebarUserProfile";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";

export const AppSidebar = memo(() => {
  const { user } = useAuth();
  const { menuItems, isLoading, error, refreshMenu } = useNavigation();
  const { state } = useSidebar();
  const isCollapsed = state === "collapsed";

  if (!user) return null;

  return (
    <Sidebar className={cn(isCollapsed ? "w-16" : "w-64")} collapsible="icon">
      <SidebarContent>
        {/* Header */}
        <div className="p-4 border-b">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-gradient-primary rounded-lg flex items-center justify-center flex-shrink-0">
              <GraduationCap className="h-6 w-6 text-white" />
            </div>
            {!isCollapsed && (
              <div className="min-w-0 flex-1">
                <h2 className="text-lg font-semibold truncate">EduManage</h2>
                <p className="text-xs text-muted-foreground truncate">
                  School Management
                </p>
              </div>
            )}
          </div>
        </div>

        {/* Dynamic Navigation */}
        <SidebarGroup>
          {!isCollapsed && <SidebarGroupLabel>Navigation</SidebarGroupLabel>}
          <SidebarGroupContent>
            {isLoading ? (
              <div className="flex items-center justify-center p-8">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
              </div>
            ) : error ? (
              <div className="p-4">
                <Alert variant="destructive">
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription className="text-xs">
                    {error}
                  </AlertDescription>
                </Alert>
                {!isCollapsed && (
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full mt-2"
                    onClick={refreshMenu}
                  >
                    Retry
                  </Button>
                )}
              </div>
            ) : menuItems.length === 0 ? (
              !isCollapsed && (
                <div className="p-4 text-center text-sm text-muted-foreground">
                  No menu items available
                </div>
              )
            ) : (
              <SidebarMenu>
                {menuItems.map((item) => (
                  <SidebarNavItem key={item.id} item={item} />
                ))}
              </SidebarMenu>
            )}
          </SidebarGroupContent>
        </SidebarGroup>

        {/* User Profile */}
        <SidebarUserProfile />
      </SidebarContent>
    </Sidebar>
  );
});

AppSidebar.displayName = "AppSidebar";
