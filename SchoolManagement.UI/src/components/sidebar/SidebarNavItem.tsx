import { memo, useState, useCallback, useEffect } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { ChevronDown, ChevronRight } from "lucide-react";
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
  
  // Check if this specific route is active
  const isDirectlyActive = location.pathname === item.url;
  
  // Recursive function to check if any child is active
  const hasActiveChild = useCallback((navItem: NavItem): boolean => {
    if (!navItem.children || navItem.children.length === 0) {
      return false;
    }
    
    return navItem.children.some(child => {
      if (location.pathname === child.url) return true;
      return hasActiveChild(child);
    });
  }, [location.pathname]);
  
  const childIsActive = hasActiveChild(item);
  const isActive = isDirectlyActive || childIsActive;
  
  const [isOpen, setIsOpen] = useState(() => {
    return isActive;
  });

  useEffect(() => {
    if (isActive && !isOpen) {
      setIsOpen(true);
    }
  }, [isActive, isOpen]);

  const hasChildren = item.children && item.children.length > 0;
  const canView = item.permissions.canView;

  const handleToggle = useCallback(() => {
    setIsOpen((prev) => !prev);
  }, []);

  if (!canView) return null;

  // Parent item with children
  if (hasChildren) {
    return (
      <SidebarMenuItem>
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <SidebarMenuButton
                onClick={handleToggle}
                className={cn(
                  "w-full justify-between transition-all duration-200 group",
                  isActive ? [
                    "bg-gradient-to-r from-brand-primary to-brand-primary/80",
                    "text-white font-semibold",
                    "shadow-md shadow-brand-primary/30",
                    "hover:shadow-lg hover:shadow-brand-primary/40"
                  ] : [
                    "hover:bg-muted/80",
                    "hover:text-brand-primary"
                  ]
                )}
              >
                <div className="flex items-center gap-3">
                  <item.icon 
                    className={cn(
                      "h-4 w-4 flex-shrink-0 transition-all duration-200",
                      isActive && "scale-110 drop-shadow-sm"
                    )} 
                  />
                  {!isCollapsed && (
                    <span className="truncate">{item.title}</span>
                  )}
                </div>
                {!isCollapsed && (
                  <div className="flex items-center gap-2">
                    {isOpen ? (
                      <ChevronDown 
                        className={cn(
                          "h-4 w-4 flex-shrink-0 transition-transform duration-200",
                          isActive && "text-white"
                        )} 
                      />
                    ) : (
                      <ChevronRight 
                        className={cn(
                          "h-4 w-4 flex-shrink-0 transition-transform duration-200",
                          isActive && "text-white"
                        )} 
                      />
                    )}
                  </div>
                )}
              </SidebarMenuButton>
            </TooltipTrigger>
            {isCollapsed && (
              <TooltipContent side="right" className="font-medium">
                <p>{item.title}</p>
              </TooltipContent>
            )}
          </Tooltip>
        </TooltipProvider>
        
        {isOpen && !isCollapsed && (
          <div className="ml-4 mt-1 space-y-0.5 border-l-2 border-brand-primary/30 pl-3 relative">
            {isActive && (
              <div className="absolute left-0 top-0 bottom-0 w-0.5 bg-gradient-to-b from-brand-primary via-brand-primary/50 to-transparent animate-pulse" />
            )}
            {item.children.map((child) => (
              <SidebarNavItem key={child.id} item={child} level={level + 1} />
            ))}
          </div>
        )}
      </SidebarMenuItem>
    );
  }

  // Leaf item (Dashboard, single items, and child items)
  // FIXED: Use end prop to ensure exact match
  return (
    <SidebarMenuItem>
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <SidebarMenuButton asChild>
              <NavLink
                to={item.url}
                end // This ensures exact path matching
                className={({ isActive }) => {
                  // Debug log
                  if (item.title === "Dashboard") {
                    console.log("Dashboard active state:", isActive, "Current path:", location.pathname, "Item path:", item.url);
                  }
                  
                  return cn(
                    "flex items-center gap-3 w-full px-3 py-2 text-sm rounded-lg transition-all duration-200 relative group",
                    isActive ? [
                      "bg-gradient-to-r from-brand-primary to-brand-primary/90",
                      "text-white font-semibold",
                      "shadow-md shadow-brand-primary/30",
                      level === 0 ? "border-l-4 border-white/50" : "border-l-4 border-white",
                      "scale-[1.02]"
                    ] : [
                      "text-muted-foreground hover:text-brand-primary",
                      "hover:bg-brand-primary/5",
                      "hover:translate-x-1",
                      "hover:shadow-sm",
                      "border-l-2 border-transparent hover:border-brand-primary/30"
                    ]
                  );
                }}
              >
                {location.pathname === item.url && (
                  <div className="absolute -left-3 top-1/2 -translate-y-1/2 w-2 h-2 bg-white rounded-full shadow-md animate-pulse" />
                )}
                
                <item.icon 
                  className={cn(
                    "h-4 w-4 flex-shrink-0 transition-all duration-200",
                    location.pathname === item.url && "scale-110 drop-shadow-sm"
                  )} 
                />
                {!isCollapsed && (
                  <span className="truncate flex-1">{item.title}</span>
                )}
                
                {location.pathname === item.url && !isCollapsed && (
                  <div className="w-1.5 h-1.5 bg-white rounded-full animate-pulse" />
                )}
              </NavLink>
            </SidebarMenuButton>
          </TooltipTrigger>
          {isCollapsed && (
            <TooltipContent side="right" className="font-medium">
              <p>{item.title}</p>
            </TooltipContent>
          )}
        </Tooltip>
      </TooltipProvider>
    </SidebarMenuItem>
  );
});

SidebarNavItem.displayName = "SidebarNavItem";
