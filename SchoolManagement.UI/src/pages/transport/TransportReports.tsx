import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, PieChart, Pie, Cell } from "recharts";
import { Download, Calendar, Fuel, Bus, Route, Clock, TrendingUp, FileText } from "lucide-react";

const TransportReports = () => {
  const [selectedMonth, setSelectedMonth] = useState("december");
  const [selectedVehicle, setSelectedVehicle] = useState("all");

  const fuelData = [
    { month: "Jul", consumption: 1200, cost: 96000 },
    { month: "Aug", consumption: 1150, cost: 92000 },
    { month: "Sep", consumption: 1180, cost: 94400 },
    { month: "Oct", consumption: 1220, cost: 97600 },
    { month: "Nov", consumption: 1100, cost: 88000 },
    { month: "Dec", consumption: 1050, cost: 84000 },
  ];

  const routeEfficiency = [
    { route: "Route 1", onTime: 95, delayed: 5 },
    { route: "Route 2", onTime: 92, delayed: 8 },
    { route: "Route 3", onTime: 88, delayed: 12 },
    { route: "Route 4", onTime: 90, delayed: 10 },
    { route: "Route 5", onTime: 97, delayed: 3 },
  ];

  const vehicleUtilization = [
    { name: "In Use", value: 75 },
    { name: "Available", value: 15 },
    { name: "Maintenance", value: 10 },
  ];

  const COLORS = ["#22c55e", "#3b82f6", "#f59e0b"];

  const tripHistory = [
    { date: "Dec 10, 2024", vehicle: "KA-01-AB-1234", route: "Route 1", driver: "Ramesh Kumar", students: 45, status: "Completed", departure: "7:00 AM", arrival: "7:42 AM" },
    { date: "Dec 10, 2024", vehicle: "KA-01-CD-5678", route: "Route 2", driver: "Suresh Singh", students: 38, status: "Completed", departure: "7:00 AM", arrival: "7:38 AM" },
    { date: "Dec 10, 2024", vehicle: "KA-01-EF-9012", route: "Route 3", driver: "Prakash Rao", students: 42, status: "Completed", departure: "6:45 AM", arrival: "7:35 AM" },
    { date: "Dec 10, 2024", vehicle: "KA-01-GH-3456", route: "Route 4", driver: "Mohan Das", students: 35, status: "Delayed", departure: "7:15 AM", arrival: "8:00 AM" },
    { date: "Dec 09, 2024", vehicle: "KA-01-AB-1234", route: "Route 1", driver: "Ramesh Kumar", students: 44, status: "Completed", departure: "7:00 AM", arrival: "7:40 AM" },
  ];

  const maintenanceLogs = [
    { date: "Dec 08, 2024", vehicle: "KA-01-GH-3456", type: "Scheduled", description: "Oil change and brake inspection", cost: 4500, status: "Completed" },
    { date: "Dec 05, 2024", vehicle: "KA-01-AB-1234", type: "Repair", description: "AC compressor replacement", cost: 12000, status: "Completed" },
    { date: "Dec 01, 2024", vehicle: "KA-01-CD-5678", type: "Scheduled", description: "Tire rotation and alignment", cost: 3500, status: "Completed" },
    { date: "Nov 28, 2024", vehicle: "KA-01-EF-9012", type: "Scheduled", description: "Regular service", cost: 5000, status: "Completed" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Transport Reports</h1>
          <p className="text-muted-foreground mt-1">Analytics and insights for transport operations</p>
        </div>
        <div className="flex items-center gap-3">
          <Select value={selectedMonth} onValueChange={setSelectedMonth}>
            <SelectTrigger className="w-40">
              <SelectValue placeholder="Select month" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="december">December 2024</SelectItem>
              <SelectItem value="november">November 2024</SelectItem>
              <SelectItem value="october">October 2024</SelectItem>
            </SelectContent>
          </Select>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
        </div>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-50 rounded-lg">
                <Bus className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">248</p>
                <p className="text-sm text-muted-foreground">Total Trips</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-50 rounded-lg">
                <TrendingUp className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">92%</p>
                <p className="text-sm text-muted-foreground">On-Time Rate</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-amber-50 rounded-lg">
                <Fuel className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">₹84,000</p>
                <p className="text-sm text-muted-foreground">Fuel Cost</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-sm">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-50 rounded-lg">
                <Route className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">1,860 km</p>
                <p className="text-sm text-muted-foreground">Total Distance</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="trips">Trip History</TabsTrigger>
          <TabsTrigger value="fuel">Fuel Reports</TabsTrigger>
          <TabsTrigger value="maintenance">Maintenance</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Fuel Consumption Chart */}
            <Card className="border-0 shadow-sm">
              <CardHeader>
                <CardTitle className="text-lg">Fuel Consumption Trend</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={fuelData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                    <XAxis dataKey="month" stroke="#888" fontSize={12} />
                    <YAxis stroke="#888" fontSize={12} />
                    <Tooltip />
                    <Line type="monotone" dataKey="consumption" stroke="#3b82f6" strokeWidth={2} dot={{ fill: "#3b82f6" }} />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            {/* Route Efficiency Chart */}
            <Card className="border-0 shadow-sm">
              <CardHeader>
                <CardTitle className="text-lg">Route Efficiency</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={routeEfficiency}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                    <XAxis dataKey="route" stroke="#888" fontSize={12} />
                    <YAxis stroke="#888" fontSize={12} />
                    <Tooltip />
                    <Bar dataKey="onTime" fill="#22c55e" name="On Time %" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="delayed" fill="#ef4444" name="Delayed %" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Vehicle Utilization */}
            <Card className="border-0 shadow-sm">
              <CardHeader>
                <CardTitle className="text-lg">Vehicle Utilization</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={250}>
                  <PieChart>
                    <Pie
                      data={vehicleUtilization}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={80}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {vehicleUtilization.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
                <div className="flex justify-center gap-4 mt-4">
                  {vehicleUtilization.map((item, index) => (
                    <div key={item.name} className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full" style={{ backgroundColor: COLORS[index] }}></div>
                      <span className="text-sm">{item.name} ({item.value}%)</span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Quick Stats */}
            <Card className="border-0 shadow-sm lg:col-span-2">
              <CardHeader>
                <CardTitle className="text-lg">Monthly Summary</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 gap-4">
                  <div className="p-4 bg-muted/30 rounded-xl">
                    <div className="flex items-center gap-2 text-muted-foreground mb-2">
                      <Calendar className="h-4 w-4" />
                      <span className="text-sm">Operating Days</span>
                    </div>
                    <p className="text-2xl font-bold">22</p>
                  </div>
                  <div className="p-4 bg-muted/30 rounded-xl">
                    <div className="flex items-center gap-2 text-muted-foreground mb-2">
                      <Clock className="h-4 w-4" />
                      <span className="text-sm">Avg. Trip Duration</span>
                    </div>
                    <p className="text-2xl font-bold">42 min</p>
                  </div>
                  <div className="p-4 bg-muted/30 rounded-xl">
                    <div className="flex items-center gap-2 text-muted-foreground mb-2">
                      <Bus className="h-4 w-4" />
                      <span className="text-sm">Students Transported</span>
                    </div>
                    <p className="text-2xl font-bold">7,040</p>
                  </div>
                  <div className="p-4 bg-muted/30 rounded-xl">
                    <div className="flex items-center gap-2 text-muted-foreground mb-2">
                      <FileText className="h-4 w-4" />
                      <span className="text-sm">Incidents Reported</span>
                    </div>
                    <p className="text-2xl font-bold">2</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="trips">
          <Card className="border-0 shadow-sm">
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Trip History</CardTitle>
                <Select value={selectedVehicle} onValueChange={setSelectedVehicle}>
                  <SelectTrigger className="w-48">
                    <SelectValue placeholder="Select vehicle" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Vehicles</SelectItem>
                    <SelectItem value="vh001">KA-01-AB-1234</SelectItem>
                    <SelectItem value="vh002">KA-01-CD-5678</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Vehicle</TableHead>
                    <TableHead>Route</TableHead>
                    <TableHead>Driver</TableHead>
                    <TableHead>Students</TableHead>
                    <TableHead>Departure</TableHead>
                    <TableHead>Arrival</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {tripHistory.map((trip, index) => (
                    <TableRow key={index}>
                      <TableCell>{trip.date}</TableCell>
                      <TableCell className="font-medium">{trip.vehicle}</TableCell>
                      <TableCell>{trip.route}</TableCell>
                      <TableCell>{trip.driver}</TableCell>
                      <TableCell>{trip.students}</TableCell>
                      <TableCell>{trip.departure}</TableCell>
                      <TableCell>{trip.arrival}</TableCell>
                      <TableCell>
                        <Badge variant={trip.status === "Completed" ? "default" : "destructive"}>
                          {trip.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="fuel">
          <Card className="border-0 shadow-sm">
            <CardHeader>
              <CardTitle>Fuel Consumption Report</CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={400}>
                <BarChart data={fuelData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="month" stroke="#888" fontSize={12} />
                  <YAxis yAxisId="left" stroke="#888" fontSize={12} />
                  <YAxis yAxisId="right" orientation="right" stroke="#888" fontSize={12} />
                  <Tooltip />
                  <Bar yAxisId="left" dataKey="consumption" fill="#3b82f6" name="Consumption (L)" radius={[4, 4, 0, 0]} />
                  <Bar yAxisId="right" dataKey="cost" fill="#22c55e" name="Cost (₹)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="maintenance">
          <Card className="border-0 shadow-sm">
            <CardHeader>
              <CardTitle>Maintenance Logs</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Vehicle</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead>Cost</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {maintenanceLogs.map((log, index) => (
                    <TableRow key={index}>
                      <TableCell>{log.date}</TableCell>
                      <TableCell className="font-medium">{log.vehicle}</TableCell>
                      <TableCell>
                        <Badge variant={log.type === "Scheduled" ? "outline" : "secondary"}>
                          {log.type}
                        </Badge>
                      </TableCell>
                      <TableCell>{log.description}</TableCell>
                      <TableCell>₹{log.cost.toLocaleString()}</TableCell>
                      <TableCell>
                        <Badge className="bg-green-100 text-green-700 hover:bg-green-100">
                          {log.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default TransportReports;
