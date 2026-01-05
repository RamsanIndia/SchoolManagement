import { memo } from "react";
import { useAuth, useUserFullName } from "@/contexts/AuthContext";
import { useSidebar } from "@/components/ui/sidebar";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";

export const SidebarUserProfile = memo(() => {
  const { user } = useAuth();
  const fullName = useUserFullName();
  const { state } = useSidebar();
  const isCollapsed = state === "collapsed";

  if (!user) return null;

  const initials = `${user.firstName[0]}${user.lastName[0]}`.toUpperCase();
  const primaryRole = user.roles[0];
  const avatarUrl = (user as any).avatar as string | undefined;

  if (isCollapsed) {
    return (
      <div className="p-4 border-t flex justify-center">
        <Avatar className="h-8 w-8">
          {avatarUrl && <AvatarImage src={avatarUrl} alt={fullName} />}
          <AvatarFallback className="text-xs bg-gradient-primary text-white">
            {initials}
          </AvatarFallback>
        </Avatar>
      </div>
    );
  }

  return (
    <div className="mt-auto p-4 border-t">
      <div className="flex items-center gap-3">
        <Avatar className="h-10 w-10 flex-shrink-0">
          {avatarUrl && <AvatarImage src={avatarUrl} alt={fullName} />}
          <AvatarFallback className="bg-gradient-primary text-white">
            {initials}
          </AvatarFallback>
        </Avatar>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">{fullName}</p>
          <div className="flex items-center gap-2">
            <Badge variant="outline" className="text-xs">
              {primaryRole}
            </Badge>
            {user.isEmailVerified && (
              <span className="text-xs text-green-600">✓</span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
});

SidebarUserProfile.displayName = "SidebarUserProfile";
