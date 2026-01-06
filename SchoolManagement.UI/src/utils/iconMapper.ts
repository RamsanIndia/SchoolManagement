import {
  BarChart3,
  BookOpen,
  GraduationCap,
  LayoutGrid,
  Wand2,
  Building,
  Upload,
  Clock,
  Users,
  UserPlus,
  ArrowUpCircle,
  FileText,
  ClipboardList,
  CheckCircle,
  UserCheck,
  Calendar,
  CreditCard,
  DollarSign,
  Settings,
  Shield,
  Key,
  Menu,
  Home,
  MessageSquare,
  Bus,
  Route,
  MapPin,
  Building2,
  Utensils,
  Bell,
  UserCheck2,
  type LucideIcon,
} from "lucide-react";

// Map icon names from API to Lucide components
const iconMap: Record<string, LucideIcon> = {
  BarChart3,
  BookOpen,
  GraduationCap,
  LayoutGrid,
  Wand2,
  Building,
  Upload,
  Clock,
  Users,
  UserPlus,
  ArrowUpCircle,
  FileText,
  ClipboardList,
  CheckCircle,
  UserCheck,
  UserCheck2,
  Calendar,
  CreditCard,
  DollarSign,
  Settings,
  Shield,
  Key,
  Menu,
  Home,
  MessageSquare,
  Bus,
  Route,
  MapPin,
  Building2,
  Utensils,
  Bell,
};

/**
 * Get icon component by name
 * @param iconName - Name of the icon from API
 * @returns LucideIcon component
 */
export function getIconComponent(iconName: string): LucideIcon {
  const icon = iconMap[iconName];
  
  if (!icon) {
    console.warn(`Icon "${iconName}" not found, using default BarChart3`);
    return BarChart3;
  }
  
  return icon;
}

/**
 * Get all available icon names
 * @returns Array of icon names
 */
export function getAvailableIcons(): string[] {
  return Object.keys(iconMap);
}

/**
 * Check if icon exists
 * @param iconName - Name of the icon
 * @returns boolean
 */
export function hasIcon(iconName: string): boolean {
  return iconName in iconMap;
}
