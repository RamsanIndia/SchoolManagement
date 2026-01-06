import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Bus, MapPin, Clock, Users, Navigation, RefreshCw, Phone, AlertTriangle } from "lucide-react";

const VehicleTracking = () => {
  const [selectedVehicle, setSelectedVehicle] = useState("all");

  const vehicles = [
    {
      id: "VH001",
      number: "KA-01-AB-1234",
      route: "Route 1 - North Zone",
      driver: "Ramesh Kumar",
      driverPhone: "9876543210",
      status: "Moving",
      speed: "35 km/h",
      lastStop: "City Center",
      nextStop: "Park Avenue",
      students: 45,
      eta: "7:35 AM",
      location: { lat: 12.9716, lng: 77.5946 },
      lastUpdate: "30 sec ago"
    },
    {
      id: "VH002",
      number: "KA-01-CD-5678",
      route: "Route 2 - South Zone",
      driver: "Suresh Singh",
      driverPhone: "9812345678",
      status: "At Stop",
      speed: "0 km/h",
      lastStop: "South Gate",
      nextStop: "Temple Road",
      students: 38,
      eta: "7:40 AM",
      location: { lat: 12.9516, lng: 77.5746 },
      lastUpdate: "1 min ago"
    },
    {
      id: "VH003",
      number: "KA-01-EF-9012",
      route: "Route 3 - East Zone",
      driver: "Prakash Rao",
      driverPhone: "9901122334",
      status: "Moving",
      speed: "40 km/h",
      lastStop: "East Market",
      nextStop: "School Lane",
      students: 42,
      eta: "7:38 AM",
      location: { lat: 12.9816, lng: 77.6146 },
      lastUpdate: "15 sec ago"
    },
    {
      id: "VH004",
      number: "KA-01-GH-3456",
      route: "Route 4 - West Zone",
      driver: "Mohan Das",
      driverPhone: "9988776655",
      status: "Delayed",
      speed: "20 km/h",
      lastStop: "West Colony",
      nextStop: "Lake View",
      students: 35,
      eta: "7:55 AM",
      location: { lat: 12.9616, lng: 77.5546 },
      lastUpdate: "2 min ago"
    },
  ];

  const filteredVehicles = selectedVehicle === "all" 
    ? vehicles 
    : vehicles.filter(v => v.id === selectedVehicle);

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Moving":
        return "bg-green-500";
      case "At Stop":
        return "bg-blue-500";
      case "Delayed":
        return "bg-red-500";
      default:
        return "bg-gray-500";
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Moving":
        return <Badge className="bg-green-100 text-green-700 hover:bg-green-100">Moving</Badge>;
      case "At Stop":
        return <Badge className="bg-blue-100 text-blue-700 hover:bg-blue-100">At Stop</Badge>;
      case "Delayed":
        return <Badge className="bg-red-100 text-red-700 hover:bg-red-100">Delayed</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Live Vehicle Tracking</h1>
          <p className="text-muted-foreground mt-1">Real-time location of school transport</p>
        </div>
        <div className="flex items-center gap-3">
          <Select value={selectedVehicle} onValueChange={setSelectedVehicle}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Select vehicle" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Vehicles</SelectItem>
              {vehicles.map(v => (
                <SelectItem key={v.id} value={v.id}>{v.number}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button variant="outline">
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Map Placeholder */}
        <Card className="lg:col-span-2 border-0 shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Navigation className="h-5 w-5" />
              Live Map
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="bg-gradient-to-br from-blue-50 to-green-50 rounded-xl h-[500px] flex items-center justify-center relative overflow-hidden">
              {/* Simulated map with vehicle markers */}
              <div className="absolute inset-0 opacity-20">
                <svg className="w-full h-full" viewBox="0 0 100 100">
                  <path d="M10,50 Q30,30 50,50 T90,50" fill="none" stroke="#3B82F6" strokeWidth="0.5" />
                  <path d="M50,10 Q30,30 50,50 T50,90" fill="none" stroke="#3B82F6" strokeWidth="0.5" />
                  <circle cx="50" cy="50" r="30" fill="none" stroke="#3B82F6" strokeWidth="0.3" />
                  <circle cx="50" cy="50" r="20" fill="none" stroke="#3B82F6" strokeWidth="0.3" />
                </svg>
              </div>
              
              {/* Vehicle markers */}
              {filteredVehicles.map((vehicle, index) => (
                <div
                  key={vehicle.id}
                  className="absolute"
                  style={{
                    left: `${20 + (index * 20)}%`,
                    top: `${30 + (index * 15)}%`,
                  }}
                >
                  <div className="relative">
                    <div className={`w-10 h-10 rounded-full ${getStatusColor(vehicle.status)} flex items-center justify-center shadow-lg animate-pulse`}>
                      <Bus className="h-5 w-5 text-white" />
                    </div>
                    <div className="absolute -bottom-8 left-1/2 transform -translate-x-1/2 whitespace-nowrap bg-white px-2 py-1 rounded text-xs font-medium shadow">
                      {vehicle.number}
                    </div>
                  </div>
                </div>
              ))}
              
              {/* School marker */}
              <div className="absolute right-1/4 top-1/4">
                <div className="w-12 h-12 rounded-full bg-primary flex items-center justify-center shadow-lg">
                  <MapPin className="h-6 w-6 text-white" />
                </div>
                <div className="absolute -bottom-8 left-1/2 transform -translate-x-1/2 whitespace-nowrap bg-white px-2 py-1 rounded text-xs font-medium shadow">
                  School
                </div>
              </div>

              <div className="absolute bottom-4 left-4 bg-white/90 backdrop-blur-sm rounded-lg p-3 shadow">
                <p className="text-sm font-medium mb-2">Legend</p>
                <div className="space-y-1 text-xs">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-green-500"></div>
                    <span>Moving</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-blue-500"></div>
                    <span>At Stop</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-red-500"></div>
                    <span>Delayed</span>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Vehicle List */}
        <Card className="border-0 shadow-sm">
          <CardHeader>
            <CardTitle>Active Vehicles</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {filteredVehicles.map((vehicle) => (
              <div key={vehicle.id} className="p-4 bg-muted/30 rounded-xl space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className={`w-10 h-10 rounded-full ${getStatusColor(vehicle.status)} flex items-center justify-center`}>
                      <Bus className="h-5 w-5 text-white" />
                    </div>
                    <div>
                      <p className="font-semibold">{vehicle.number}</p>
                      <p className="text-xs text-muted-foreground">{vehicle.route}</p>
                    </div>
                  </div>
                  {getStatusBadge(vehicle.status)}
                </div>

                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <p className="text-muted-foreground text-xs">Speed</p>
                    <p className="font-medium">{vehicle.speed}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground text-xs">ETA to School</p>
                    <p className="font-medium">{vehicle.eta}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground text-xs">Last Stop</p>
                    <p className="font-medium">{vehicle.lastStop}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground text-xs">Next Stop</p>
                    <p className="font-medium">{vehicle.nextStop}</p>
                  </div>
                </div>

                <div className="flex items-center justify-between pt-2 border-t">
                  <div className="flex items-center gap-2 text-sm">
                    <Users className="h-4 w-4 text-muted-foreground" />
                    <span>{vehicle.students} students</span>
                  </div>
                  <div className="flex items-center gap-2 text-xs text-muted-foreground">
                    <Clock className="h-3 w-3" />
                    <span>Updated {vehicle.lastUpdate}</span>
                  </div>
                </div>

                <div className="flex items-center justify-between pt-2">
                  <div className="text-sm">
                    <p className="text-muted-foreground text-xs">Driver</p>
                    <p className="font-medium">{vehicle.driver}</p>
                  </div>
                  <Button variant="outline" size="sm">
                    <Phone className="h-4 w-4 mr-1" />
                    Call
                  </Button>
                </div>

                {vehicle.status === "Delayed" && (
                  <div className="flex items-center gap-2 p-2 bg-red-50 rounded-lg text-sm text-red-700">
                    <AlertTriangle className="h-4 w-4" />
                    <span>Running 10 minutes behind schedule</span>
                  </div>
                )}
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-50 rounded-lg">
                <Bus className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">2</p>
                <p className="text-sm text-muted-foreground">Vehicles Moving</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-50 rounded-lg">
                <MapPin className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">1</p>
                <p className="text-sm text-muted-foreground">At Stop</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-red-50 rounded-lg">
                <AlertTriangle className="h-5 w-5 text-red-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">1</p>
                <p className="text-sm text-muted-foreground">Delayed</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-50 rounded-lg">
                <Users className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">160</p>
                <p className="text-sm text-muted-foreground">Students In Transit</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default VehicleTracking;
