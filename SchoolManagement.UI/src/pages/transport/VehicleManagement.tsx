import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Plus, Search, Bus, Fuel, Wrench, Calendar, Edit, Eye, Settings } from "lucide-react";
import { toast } from "sonner";

const VehicleManagement = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const vehicles = [
    { 
      id: "VH001", 
      number: "KA-01-AB-1234", 
      type: "Bus",
      make: "Tata Starbus",
      capacity: 50,
      year: 2022,
      route: "Route 1 - North Zone",
      driver: "Ramesh Kumar",
      fuelType: "Diesel",
      mileage: "8 km/l",
      lastService: "Nov 15, 2024",
      nextService: "Feb 15, 2025",
      insurance: "Dec 31, 2024",
      status: "Active"
    },
    { 
      id: "VH002", 
      number: "KA-01-CD-5678", 
      type: "Bus",
      make: "Ashok Leyland",
      capacity: 45,
      year: 2021,
      route: "Route 2 - South Zone",
      driver: "Suresh Singh",
      fuelType: "Diesel",
      mileage: "7.5 km/l",
      lastService: "Oct 20, 2024",
      nextService: "Jan 20, 2025",
      insurance: "Mar 15, 2025",
      status: "Active"
    },
    { 
      id: "VH003", 
      number: "KA-01-EF-9012", 
      type: "Mini Bus",
      make: "Force Traveller",
      capacity: 26,
      year: 2023,
      route: "Route 3 - East Zone",
      driver: "Prakash Rao",
      fuelType: "Diesel",
      mileage: "10 km/l",
      lastService: "Nov 28, 2024",
      nextService: "Feb 28, 2025",
      insurance: "Jun 30, 2025",
      status: "Active"
    },
    { 
      id: "VH004", 
      number: "KA-01-GH-3456", 
      type: "Bus",
      make: "Tata Starbus",
      capacity: 50,
      year: 2020,
      route: "Route 4 - West Zone",
      driver: "Mohan Das",
      fuelType: "Diesel",
      mileage: "7 km/l",
      lastService: "Sep 10, 2024",
      nextService: "Dec 10, 2024",
      insurance: "Jan 31, 2025",
      status: "Maintenance"
    },
    { 
      id: "VH005", 
      number: "KA-01-IJ-7890", 
      type: "Van",
      make: "Maruti Eeco",
      capacity: 8,
      year: 2022,
      route: "Not Assigned",
      driver: "Not Assigned",
      fuelType: "Petrol",
      mileage: "15 km/l",
      lastService: "Nov 5, 2024",
      nextService: "Feb 5, 2025",
      insurance: "Aug 15, 2025",
      status: "Available"
    },
  ];

  const filteredVehicles = vehicles.filter(vehicle =>
    vehicle.number.toLowerCase().includes(searchTerm.toLowerCase()) ||
    vehicle.make.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAddVehicle = () => {
    toast.success("Vehicle added successfully!");
    setIsDialogOpen(false);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Active":
        return <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Active</Badge>;
      case "Maintenance":
        return <Badge className="bg-amber-100 text-amber-700 hover:bg-amber-100">Maintenance</Badge>;
      case "Available":
        return <Badge className="bg-blue-100 text-blue-700 hover:bg-blue-100">Available</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Vehicle Management</h1>
          <p className="text-muted-foreground mt-1">Manage school transport fleet</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Add Vehicle
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Add New Vehicle</DialogTitle>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Vehicle Number</Label>
                  <Input placeholder="e.g., KA-01-XY-1234" />
                </div>
                <div className="space-y-2">
                  <Label>Vehicle Type</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="bus">Bus</SelectItem>
                      <SelectItem value="minibus">Mini Bus</SelectItem>
                      <SelectItem value="van">Van</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Make & Model</Label>
                  <Input placeholder="e.g., Tata Starbus" />
                </div>
                <div className="space-y-2">
                  <Label>Manufacturing Year</Label>
                  <Input type="number" placeholder="e.g., 2023" />
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label>Seating Capacity</Label>
                  <Input type="number" placeholder="e.g., 50" />
                </div>
                <div className="space-y-2">
                  <Label>Fuel Type</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select fuel" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="diesel">Diesel</SelectItem>
                      <SelectItem value="petrol">Petrol</SelectItem>
                      <SelectItem value="cng">CNG</SelectItem>
                      <SelectItem value="electric">Electric</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Mileage</Label>
                  <Input placeholder="e.g., 8 km/l" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Insurance Expiry</Label>
                  <Input type="date" />
                </div>
                <div className="space-y-2">
                  <Label>Last Service Date</Label>
                  <Input type="date" />
                </div>
              </div>
              <div className="flex justify-end gap-3 pt-4">
                <Button variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
                <Button onClick={handleAddVehicle}>Add Vehicle</Button>
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
                <Bus className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">5</p>
                <p className="text-sm text-muted-foreground">Total Vehicles</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-50 rounded-lg">
                <Bus className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">3</p>
                <p className="text-sm text-muted-foreground">Active</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-amber-50 rounded-lg">
                <Wrench className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">1</p>
                <p className="text-sm text-muted-foreground">In Maintenance</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-50 rounded-lg">
                <Calendar className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">1</p>
                <p className="text-sm text-muted-foreground">Available</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Search and Filter */}
      <Card className="border-0 shadow-sm">
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Fleet Overview</CardTitle>
            <div className="relative w-64">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search vehicles..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9"
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="all">
            <TabsList className="mb-4">
              <TabsTrigger value="all">All Vehicles</TabsTrigger>
              <TabsTrigger value="active">Active</TabsTrigger>
              <TabsTrigger value="maintenance">Maintenance</TabsTrigger>
              <TabsTrigger value="available">Available</TabsTrigger>
            </TabsList>

            <TabsContent value="all">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredVehicles.map((vehicle) => (
                  <Card key={vehicle.id} className="border shadow-sm">
                    <CardContent className="p-5">
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-primary/10 rounded-lg">
                            <Bus className="h-6 w-6 text-primary" />
                          </div>
                          <div>
                            <h3 className="font-semibold">{vehicle.number}</h3>
                            <p className="text-sm text-muted-foreground">{vehicle.make}</p>
                          </div>
                        </div>
                        {getStatusBadge(vehicle.status)}
                      </div>

                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Type:</span>
                          <span>{vehicle.type}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Capacity:</span>
                          <span>{vehicle.capacity} seats</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Route:</span>
                          <span className={vehicle.route === "Not Assigned" ? "text-muted-foreground" : ""}>
                            {vehicle.route === "Not Assigned" ? "Not Assigned" : vehicle.route.split(" - ")[0]}
                          </span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Driver:</span>
                          <span className={vehicle.driver === "Not Assigned" ? "text-muted-foreground" : ""}>
                            {vehicle.driver}
                          </span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Fuel:</span>
                          <span className="flex items-center gap-1">
                            <Fuel className="h-3 w-3" />
                            {vehicle.fuelType} ({vehicle.mileage})
                          </span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Next Service:</span>
                          <span>{vehicle.nextService}</span>
                        </div>
                      </div>

                      <div className="flex gap-2 mt-4 pt-4 border-t">
                        <Button variant="outline" size="sm" className="flex-1">
                          <Eye className="h-4 w-4 mr-1" />
                          View
                        </Button>
                        <Button variant="outline" size="sm" className="flex-1">
                          <Edit className="h-4 w-4 mr-1" />
                          Edit
                        </Button>
                        <Button variant="outline" size="sm">
                          <Settings className="h-4 w-4" />
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="active">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredVehicles.filter(v => v.status === "Active").map((vehicle) => (
                  <Card key={vehicle.id} className="border shadow-sm">
                    <CardContent className="p-5">
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-primary/10 rounded-lg">
                            <Bus className="h-6 w-6 text-primary" />
                          </div>
                          <div>
                            <h3 className="font-semibold">{vehicle.number}</h3>
                            <p className="text-sm text-muted-foreground">{vehicle.make}</p>
                          </div>
                        </div>
                        {getStatusBadge(vehicle.status)}
                      </div>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Route:</span>
                          <span>{vehicle.route.split(" - ")[0]}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Driver:</span>
                          <span>{vehicle.driver}</span>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="maintenance">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredVehicles.filter(v => v.status === "Maintenance").map((vehicle) => (
                  <Card key={vehicle.id} className="border shadow-sm">
                    <CardContent className="p-5">
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-amber-100 rounded-lg">
                            <Wrench className="h-6 w-6 text-amber-600" />
                          </div>
                          <div>
                            <h3 className="font-semibold">{vehicle.number}</h3>
                            <p className="text-sm text-muted-foreground">{vehicle.make}</p>
                          </div>
                        </div>
                        {getStatusBadge(vehicle.status)}
                      </div>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Next Service:</span>
                          <span>{vehicle.nextService}</span>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="available">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredVehicles.filter(v => v.status === "Available").map((vehicle) => (
                  <Card key={vehicle.id} className="border shadow-sm">
                    <CardContent className="p-5">
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-blue-100 rounded-lg">
                            <Bus className="h-6 w-6 text-blue-600" />
                          </div>
                          <div>
                            <h3 className="font-semibold">{vehicle.number}</h3>
                            <p className="text-sm text-muted-foreground">{vehicle.make}</p>
                          </div>
                        </div>
                        {getStatusBadge(vehicle.status)}
                      </div>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span className="text-muted-foreground">Capacity:</span>
                          <span>{vehicle.capacity} seats</span>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </div>
  );
};

export default VehicleManagement;
