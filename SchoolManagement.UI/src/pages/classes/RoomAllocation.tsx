/**
 * Classroom Allocation Optimizer
 * Assigns rooms based on capacity and subject requirements
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Building, Users, Zap, CheckCircle, AlertTriangle, BarChart3, Settings2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "@/hooks/use-toast";

interface Room {
  id: string;
  name: string;
  type: "classroom" | "lab" | "special";
  capacity: number;
  facilities: string[];
  currentAllocation?: string;
  utilization: number;
}

interface ClassSection {
  id: string;
  name: string;
  section: string;
  strength: number;
  requiredRoom: "classroom" | "lab" | "special";
  currentRoom?: string;
  status: "allocated" | "pending" | "conflict";
}

const mockRooms: Room[] = [
  { id: "1", name: "Room 101", type: "classroom", capacity: 40, facilities: ["Projector", "AC"], currentAllocation: "Grade 1-A", utilization: 80 },
  { id: "2", name: "Room 102", type: "classroom", capacity: 35, facilities: ["Projector"], currentAllocation: "Grade 1-B", utilization: 86 },
  { id: "3", name: "Room 103", type: "classroom", capacity: 40, facilities: ["Projector", "AC"], currentAllocation: "Grade 2-A", utilization: 75 },
  { id: "4", name: "Room 104", type: "classroom", capacity: 45, facilities: ["Projector", "AC", "Smart Board"], utilization: 0 },
  { id: "5", name: "Science Lab 1", type: "lab", capacity: 30, facilities: ["Lab Equipment", "Projector"], currentAllocation: "Science Classes", utilization: 60 },
  { id: "6", name: "Science Lab 2", type: "lab", capacity: 25, facilities: ["Lab Equipment"], utilization: 40 },
  { id: "7", name: "Computer Lab", type: "lab", capacity: 35, facilities: ["Computers", "Projector", "AC"], currentAllocation: "Computer Classes", utilization: 70 },
  { id: "8", name: "Art Room", type: "special", capacity: 30, facilities: ["Art Supplies", "Display Boards"], currentAllocation: "Art Classes", utilization: 50 },
  { id: "9", name: "Music Room", type: "special", capacity: 25, facilities: ["Musical Instruments", "Soundproofing"], utilization: 30 },
  { id: "10", name: "Library", type: "special", capacity: 50, facilities: ["Books", "Reading Area", "AC"], utilization: 45 },
];

const mockClassSections: ClassSection[] = [
  { id: "1", name: "Grade 1", section: "A", strength: 32, requiredRoom: "classroom", currentRoom: "Room 101", status: "allocated" },
  { id: "2", name: "Grade 1", section: "B", strength: 30, requiredRoom: "classroom", currentRoom: "Room 102", status: "allocated" },
  { id: "3", name: "Grade 2", section: "A", strength: 28, requiredRoom: "classroom", currentRoom: "Room 103", status: "allocated" },
  { id: "4", name: "Grade 2", section: "B", strength: 35, requiredRoom: "classroom", status: "pending" },
  { id: "5", name: "Grade 3", section: "A", strength: 42, requiredRoom: "classroom", status: "conflict" },
  { id: "6", name: "Grade 3", section: "B", strength: 38, requiredRoom: "classroom", status: "pending" },
];

export default function RoomAllocation() {
  const navigate = useNavigate();
  const [rooms, setRooms] = useState<Room[]>(mockRooms);
  const [classSections, setClassSections] = useState<ClassSection[]>(mockClassSections);
  const [isOptimizing, setIsOptimizing] = useState(false);
  const [progress, setProgress] = useState(0);
  const [viewMode, setViewMode] = useState<"rooms" | "classes">("rooms");
  const [optimizeCapacity, setOptimizeCapacity] = useState(true);
  const [minimizeMovement, setMinimizeMovement] = useState(true);
  const [prioritizeFacilities, setPrioritizeFacilities] = useState(false);

  const handleOptimize = () => {
    setIsOptimizing(true);
    setProgress(0);

    const interval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 100) {
          clearInterval(interval);
          setIsOptimizing(false);
          
          // Update allocations after optimization
          setClassSections(prev => prev.map(cs => ({
            ...cs,
            status: "allocated" as const,
            currentRoom: cs.currentRoom || `Room ${104 + Math.floor(Math.random() * 2)}`
          })));
          
          toast({
            title: "Optimization Complete",
            description: "All classes have been allocated to optimal rooms",
          });
          return 100;
        }
        return prev + 8;
      });
    }, 150);
  };

  const totalRooms = rooms.length;
  const allocatedRooms = rooms.filter(r => r.currentAllocation).length;
  const avgUtilization = Math.round(rooms.reduce((sum, r) => sum + r.utilization, 0) / rooms.length);
  const pendingAllocations = classSections.filter(cs => cs.status !== "allocated").length;
  const conflicts = classSections.filter(cs => cs.status === "conflict").length;

  const getRoomTypeColor = (type: string) => {
    switch (type) {
      case "classroom": return "bg-blue-500/20 text-blue-500";
      case "lab": return "bg-purple-500/20 text-purple-500";
      case "special": return "bg-orange-500/20 text-orange-500";
      default: return "bg-muted text-muted-foreground";
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "allocated": return "bg-green-500/20 text-green-500";
      case "pending": return "bg-yellow-500/20 text-yellow-500";
      case "conflict": return "bg-destructive/20 text-destructive";
      default: return "bg-muted text-muted-foreground";
    }
  };

  const getUtilizationColor = (util: number) => {
    if (util >= 80) return "text-green-500";
    if (util >= 50) return "text-yellow-500";
    if (util > 0) return "text-orange-500";
    return "text-muted-foreground";
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate("/classes")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Room Allocation</h1>
            <p className="text-muted-foreground mt-1">Optimize classroom assignments based on capacity and requirements</p>
          </div>
        </div>
        <Button
          onClick={handleOptimize}
          disabled={isOptimizing}
          className="bg-primary hover:bg-primary/90"
        >
          <Zap className="mr-2 h-4 w-4" />
          {isOptimizing ? "Optimizing..." : "Run Optimizer"}
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-5 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-primary/10">
                <Building className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{totalRooms}</p>
                <p className="text-xs text-muted-foreground">Total Rooms</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-green-500/10">
                <CheckCircle className="h-5 w-5 text-green-500" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{allocatedRooms}</p>
                <p className="text-xs text-muted-foreground">Allocated</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-blue-500/10">
                <BarChart3 className="h-5 w-5 text-blue-500" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{avgUtilization}%</p>
                <p className="text-xs text-muted-foreground">Avg Utilization</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-yellow-500/10">
                <Users className="h-5 w-5 text-yellow-500" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{pendingAllocations}</p>
                <p className="text-xs text-muted-foreground">Pending</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-destructive/10">
                <AlertTriangle className="h-5 w-5 text-destructive" />
              </div>
              <div>
                <p className="text-2xl font-bold text-foreground">{conflicts}</p>
                <p className="text-xs text-muted-foreground">Conflicts</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Progress Bar */}
      {isOptimizing && (
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Analyzing constraints and optimizing allocations...</span>
                <span className="text-foreground font-medium">{progress}%</span>
              </div>
              <Progress value={progress} className="h-2" />
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-12 gap-6">
        {/* Settings Panel */}
        <div className="col-span-3">
          <Card className="bg-card border-border">
            <CardHeader className="pb-3">
              <CardTitle className="text-foreground flex items-center gap-2">
                <Settings2 className="h-5 w-5" />
                Optimization Settings
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-foreground">Optimize Capacity</p>
                  <p className="text-xs text-muted-foreground">Match class size to room capacity</p>
                </div>
                <Switch checked={optimizeCapacity} onCheckedChange={setOptimizeCapacity} />
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-foreground">Minimize Movement</p>
                  <p className="text-xs text-muted-foreground">Keep classes in same building</p>
                </div>
                <Switch checked={minimizeMovement} onCheckedChange={setMinimizeMovement} />
              </div>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-foreground">Prioritize Facilities</p>
                  <p className="text-xs text-muted-foreground">Match subject needs to room facilities</p>
                </div>
                <Switch checked={prioritizeFacilities} onCheckedChange={setPrioritizeFacilities} />
              </div>
            </CardContent>
          </Card>

          {/* View Toggle */}
          <Card className="bg-card border-border mt-4">
            <CardContent className="p-4">
              <Select value={viewMode} onValueChange={(v) => setViewMode(v as "rooms" | "classes")}>
                <SelectTrigger className="bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-popover">
                  <SelectItem value="rooms">View by Rooms</SelectItem>
                  <SelectItem value="classes">View by Classes</SelectItem>
                </SelectContent>
              </Select>
            </CardContent>
          </Card>
        </div>

        {/* Main Content */}
        <div className="col-span-9">
          {viewMode === "rooms" ? (
            <Card className="bg-card border-border">
              <CardHeader className="pb-3">
                <CardTitle className="text-foreground">Room Inventory</CardTitle>
                <CardDescription>All available rooms with current allocations</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-border">
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Room</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Type</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Capacity</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Facilities</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Allocation</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Utilization</th>
                      </tr>
                    </thead>
                    <tbody>
                      {rooms.map(room => (
                        <tr key={room.id} className="border-b border-border hover:bg-muted/30">
                          <td className="p-3 text-foreground font-medium">{room.name}</td>
                          <td className="p-3">
                            <Badge variant="secondary" className={`text-xs capitalize ${getRoomTypeColor(room.type)}`}>
                              {room.type}
                            </Badge>
                          </td>
                          <td className="p-3 text-foreground">{room.capacity}</td>
                          <td className="p-3">
                            <div className="flex flex-wrap gap-1">
                              {room.facilities.slice(0, 2).map((f, i) => (
                                <Badge key={i} variant="outline" className="text-xs">
                                  {f}
                                </Badge>
                              ))}
                              {room.facilities.length > 2 && (
                                <Badge variant="outline" className="text-xs">
                                  +{room.facilities.length - 2}
                                </Badge>
                              )}
                            </div>
                          </td>
                          <td className="p-3 text-muted-foreground">
                            {room.currentAllocation || <span className="text-muted-foreground/50">Unassigned</span>}
                          </td>
                          <td className="p-3">
                            <div className="flex items-center gap-2">
                              <Progress value={room.utilization} className="h-2 w-16" />
                              <span className={`text-sm font-medium ${getUtilizationColor(room.utilization)}`}>
                                {room.utilization}%
                              </span>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card className="bg-card border-border">
              <CardHeader className="pb-3">
                <CardTitle className="text-foreground">Class Allocations</CardTitle>
                <CardDescription>Room assignments by class and section</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-border">
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Class</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Section</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Strength</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Required</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Assigned Room</th>
                        <th className="text-left p-3 text-muted-foreground text-sm font-medium">Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      {classSections.map(cs => (
                        <tr key={cs.id} className="border-b border-border hover:bg-muted/30">
                          <td className="p-3 text-foreground font-medium">{cs.name}</td>
                          <td className="p-3 text-foreground">Section {cs.section}</td>
                          <td className="p-3 text-foreground">{cs.strength} students</td>
                          <td className="p-3">
                            <Badge variant="secondary" className={`text-xs capitalize ${getRoomTypeColor(cs.requiredRoom)}`}>
                              {cs.requiredRoom}
                            </Badge>
                          </td>
                          <td className="p-3 text-muted-foreground">
                            {cs.currentRoom || <span className="text-muted-foreground/50">Not assigned</span>}
                          </td>
                          <td className="p-3">
                            <Badge variant="secondary" className={`text-xs capitalize ${getStatusColor(cs.status)}`}>
                              {cs.status}
                            </Badge>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
