import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Plus, Search, Route, MapPin, Clock, Users, Edit, Trash2, Eye } from "lucide-react";
import { toast } from "sonner";

const RouteManagement = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const routes = [
    { 
      id: "RT001", 
      name: "Route 1 - North Zone", 
      stops: 12, 
      distance: "18 km", 
      duration: "45 min",
      vehicle: "KA-01-AB-1234",
      driver: "Ramesh Kumar",
      students: 45,
      status: "Active",
      startTime: "7:00 AM",
      endTime: "7:45 AM"
    },
    { 
      id: "RT002", 
      name: "Route 2 - South Zone", 
      stops: 10, 
      distance: "15 km", 
      duration: "40 min",
      vehicle: "KA-01-CD-5678",
      driver: "Suresh Singh",
      students: 38,
      status: "Active",
      startTime: "7:00 AM",
      endTime: "7:40 AM"
    },
    { 
      id: "RT003", 
      name: "Route 3 - East Zone", 
      stops: 14, 
      distance: "22 km", 
      duration: "55 min",
      vehicle: "KA-01-EF-9012",
      driver: "Prakash Rao",
      students: 42,
      status: "Active",
      startTime: "6:45 AM",
      endTime: "7:40 AM"
    },
    { 
      id: "RT004", 
      name: "Route 4 - West Zone", 
      stops: 8, 
      distance: "12 km", 
      duration: "35 min",
      vehicle: "KA-01-GH-3456",
      driver: "Mohan Das",
      students: 35,
      status: "Active",
      startTime: "7:15 AM",
      endTime: "7:50 AM"
    },
    { 
      id: "RT005", 
      name: "Route 5 - Central", 
      stops: 6, 
      distance: "8 km", 
      duration: "25 min",
      vehicle: "Not Assigned",
      driver: "Not Assigned",
      students: 28,
      status: "Inactive",
      startTime: "7:30 AM",
      endTime: "7:55 AM"
    },
  ];

  const filteredRoutes = routes.filter(route =>
    route.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    route.id.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleCreateRoute = () => {
    toast.success("Route created successfully!");
    setIsDialogOpen(false);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Route Management</h1>
          <p className="text-muted-foreground mt-1">Create and manage transport routes</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Create Route
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Create New Route</DialogTitle>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Route Name</Label>
                  <Input placeholder="e.g., Route 6 - Downtown" />
                </div>
                <div className="space-y-2">
                  <Label>Route Code</Label>
                  <Input placeholder="e.g., RT006" />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label>Start Time</Label>
                  <Input type="time" defaultValue="07:00" />
                </div>
                <div className="space-y-2">
                  <Label>End Time</Label>
                  <Input type="time" defaultValue="07:45" />
                </div>
                <div className="space-y-2">
                  <Label>Estimated Duration</Label>
                  <Input placeholder="e.g., 45 min" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Assign Vehicle</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select vehicle" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="vh001">KA-01-AB-1234</SelectItem>
                      <SelectItem value="vh002">KA-01-CD-5678</SelectItem>
                      <SelectItem value="vh003">KA-01-EF-9012</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Assign Driver</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select driver" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="d001">Ramesh Kumar</SelectItem>
                      <SelectItem value="d002">Suresh Singh</SelectItem>
                      <SelectItem value="d003">Prakash Rao</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="space-y-2">
                <Label>Stops (comma separated)</Label>
                <Textarea placeholder="e.g., Main Gate, City Center, Park Avenue, School Lane..." />
              </div>
              <div className="space-y-2">
                <Label>Description</Label>
                <Textarea placeholder="Route description and special instructions..." />
              </div>
              <div className="flex justify-end gap-3 pt-4">
                <Button variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
                <Button onClick={handleCreateRoute}>Create Route</Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-50 rounded-lg">
                <Route className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">5</p>
                <p className="text-sm text-muted-foreground">Total Routes</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-50 rounded-lg">
                <MapPin className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">50</p>
                <p className="text-sm text-muted-foreground">Total Stops</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-50 rounded-lg">
                <Clock className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">75 km</p>
                <p className="text-sm text-muted-foreground">Total Distance</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-orange-50 rounded-lg">
                <Users className="h-5 w-5 text-orange-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">188</p>
                <p className="text-sm text-muted-foreground">Students Covered</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Routes Table */}
      <Card className="border-0 shadow-sm">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>All Routes</CardTitle>
            <div className="relative w-64">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search routes..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9"
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Route ID</TableHead>
                <TableHead>Route Name</TableHead>
                <TableHead>Stops</TableHead>
                <TableHead>Distance</TableHead>
                <TableHead>Time</TableHead>
                <TableHead>Vehicle</TableHead>
                <TableHead>Driver</TableHead>
                <TableHead>Students</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredRoutes.map((route) => (
                <TableRow key={route.id}>
                  <TableCell className="font-medium">{route.id}</TableCell>
                  <TableCell>{route.name}</TableCell>
                  <TableCell>{route.stops}</TableCell>
                  <TableCell>{route.distance}</TableCell>
                  <TableCell>{route.startTime} - {route.endTime}</TableCell>
                  <TableCell>
                    <span className={route.vehicle === "Not Assigned" ? "text-muted-foreground" : ""}>
                      {route.vehicle}
                    </span>
                  </TableCell>
                  <TableCell>
                    <span className={route.driver === "Not Assigned" ? "text-muted-foreground" : ""}>
                      {route.driver}
                    </span>
                  </TableCell>
                  <TableCell>{route.students}</TableCell>
                  <TableCell>
                    <Badge variant={route.status === "Active" ? "default" : "secondary"}>
                      {route.status}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" className="h-8 w-8">
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive">
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
};

export default RouteManagement;
