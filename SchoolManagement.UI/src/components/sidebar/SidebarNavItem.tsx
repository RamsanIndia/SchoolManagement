import { memo, useState, useCallback } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { ChevronDown, ChevronRight, Lock } from "lucide-react";
import { cn } from "@/lib/utils";
import { NavItem } from "@/types/navigation.types";
import {
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

interface SidebarNavItemProps {
  item: NavItem;
  level?: number;
}

export const SidebarNavItem = memo(({ item, level = 0 }: SidebarNavItemProps) => {
  const location = useLocation();
  const { state } = useSidebar();
  const isCollapsed = state === "collapsed";
  
  const [isOpen, setIsOpen] = useState(() => {
    if (item.children) {
      return item.children.some((child) => location.pathname === child.url);
    }
    return false;
  });

  const isActive = location.pathname === item.url;
  const hasChildren = item.children && item.children.length > 0;
  const canView = item.permissions.canView;

  const handleToggle = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  if (!canView) return null;

  if (hasChildren) {
    return (
      <SidebarMenuItem>
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <SidebarMenuButton
                onClick={handleToggle}
                className={cn(
                  "w-full justify-between transition-colors",
                  isActive && "bg-brand-primary text-white hover:bg-brand-primary/90"
                )}
              >
                <div className="flex items-center gap-3">
                  <item.icon className="h-4 w-4 flex-shrink-0" />
                  {!isCollapsed && (
                    <span className="truncate">{item.title}</span>
                  )}
                </div>
                {!isCollapsed && (
                  <div className="flex items-center gap-2">
                    {isOpen ? (
                      <ChevronDown className="h-4 w-4 flex-shrink-0" />
                    ) : (
                      <ChevronRight className="h-4 w-4 flex-shrink-0" />
                    )}
                  </div>
                )}
              </SidebarMenuButton>
            </TooltipTrigger>
            {isCollapsed && (
              <TooltipContent side="right">
                <p>{item.title}</p>
              </TooltipContent>
            )}
          </Tooltip>
        </TooltipProvider>
        
        {isOpen && !isCollapsed && (
          <div className="ml-4 mt-1 space-y-1 border-l pl-2">
            {item.children.map((child) => (
              <SidebarNavItem key={child.id} item={child} level={level + 1} />
            ))}
          </div>
        )}
      </SidebarMenuItem>
    );
  }

  return (
    <SidebarMenuItem>
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <SidebarMenuButton asChild>
              <NavLink
                to={item.url}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 w-full px-3 py-2 text-sm rounded-lg transition-colors",
                    isActive
                      ? "bg-brand-primary text-white"
                      : "text-foreground hover:bg-muted"
                  )
                }
              >
                <item.icon className="h-4 w-4 flex-shrink-0" />
                {!isCollapsed && (
                  <span className="truncate">{item.title}</span>
                )}
              </NavLink>
            </SidebarMenuButton>
          </TooltipTrigger>
          {isCollapsed && (
            <TooltipContent side="right">
              <p>{item.title}</p>
            </TooltipContent>
          )}
        </Tooltip>
      </TooltipProvider>
    </SidebarMenuItem>
  );
});

SidebarNavItem.displayName = "SidebarNavItem";
