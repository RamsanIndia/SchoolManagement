import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Bus, Route, Users, MapPin, AlertTriangle, CheckCircle, Clock, Fuel } from "lucide-react";
import { useNavigate } from "react-router-dom";

const TransportDashboard = () => {
  const navigate = useNavigate();

  const stats = [
    { title: "Total Vehicles", value: "24", icon: Bus, color: "text-blue-600", bg: "bg-blue-50" },
    { title: "Active Routes", value: "18", icon: Route, color: "text-green-600", bg: "bg-green-50" },
    { title: "Total Drivers", value: "28", icon: Users, color: "text-purple-600", bg: "bg-purple-50" },
    { title: "Students Using Transport", value: "842", icon: MapPin, color: "text-orange-600", bg: "bg-orange-50" },
  ];

  const activeVehicles = [
    { id: "VH001", number: "KA-01-AB-1234", route: "Route 1 - North Zone", driver: "Ramesh Kumar", status: "On Route", students: 45, lastUpdate: "2 min ago" },
    { id: "VH002", number: "KA-01-CD-5678", route: "Route 2 - South Zone", driver: "Suresh Singh", status: "On Route", students: 38, lastUpdate: "1 min ago" },
    { id: "VH003", number: "KA-01-EF-9012", route: "Route 3 - East Zone", driver: "Prakash Rao", status: "At Stop", students: 42, lastUpdate: "30 sec ago" },
    { id: "VH004", number: "KA-01-GH-3456", route: "Route 4 - West Zone", driver: "Mohan Das", status: "Delayed", students: 35, lastUpdate: "5 min ago" },
  ];

  const alerts = [
    { type: "warning", message: "Vehicle VH004 running 10 minutes behind schedule", time: "5 min ago" },
    { type: "info", message: "Route 5 - Driver change for tomorrow", time: "1 hour ago" },
    { type: "success", message: "All morning routes completed successfully", time: "2 hours ago" },
  ];

  const upcomingMaintenance = [
    { vehicle: "KA-01-AB-1234", type: "Oil Change", date: "Dec 15, 2024" },
    { vehicle: "KA-01-CD-5678", type: "Tire Replacement", date: "Dec 18, 2024" },
    { vehicle: "KA-01-IJ-7890", type: "Annual Service", date: "Dec 20, 2024" },
  ];

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "On Route":
        return <Badge className="bg-green-100 text-green-700 hover:bg-green-100">On Route</Badge>;
      case "At Stop":
        return <Badge className="bg-blue-100 text-blue-700 hover:bg-blue-100">At Stop</Badge>;
      case "Delayed":
        return <Badge className="bg-red-100 text-red-700 hover:bg-red-100">Delayed</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const getAlertIcon = (type: string) => {
    switch (type) {
      case "warning":
        return <AlertTriangle className="h-4 w-4 text-amber-500" />;
      case "success":
        return <CheckCircle className="h-4 w-4 text-green-500" />;
      default:
        return <Clock className="h-4 w-4 text-blue-500" />;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Transport Dashboard</h1>
          <p className="text-muted-foreground mt-1">Monitor and manage school transport operations</p>
        </div>
        <div className="flex gap-3">
          <Button variant="outline" onClick={() => navigate("/transport/tracking")}>
            <MapPin className="h-4 w-4 mr-2" />
            Live Tracking
          </Button>
          <Button onClick={() => navigate("/transport/routes/new")}>
            <Route className="h-4 w-4 mr-2" />
            Create Route
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, index) => (
          <Card key={index} className="border-0 shadow-sm">
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-muted-foreground">{stat.title}</p>
                  <p className="text-3xl font-bold mt-1">{stat.value}</p>
                </div>
                <div className={`p-3 rounded-xl ${stat.bg}`}>
                  <stat.icon className={`h-6 w-6 ${stat.color}`} />
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Active Vehicles */}
        <Card className="lg:col-span-2 border-0 shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg font-semibold">Active Vehicles</CardTitle>
            <Button variant="ghost" size="sm" onClick={() => navigate("/transport/vehicles")}>
              View All
            </Button>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {activeVehicles.map((vehicle) => (
                <div key={vehicle.id} className="flex items-center justify-between p-4 bg-muted/30 rounded-xl">
                  <div className="flex items-center gap-4">
                    <div className="p-2 bg-primary/10 rounded-lg">
                      <Bus className="h-5 w-5 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium">{vehicle.number}</p>
                      <p className="text-sm text-muted-foreground">{vehicle.route}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="flex items-center gap-2">
                      {getStatusBadge(vehicle.status)}
                      <span className="text-sm text-muted-foreground">{vehicle.students} students</span>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">Updated {vehicle.lastUpdate}</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Alerts & Notifications */}
        <Card className="border-0 shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg font-semibold">Alerts & Notifications</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {alerts.map((alert, index) => (
                <div key={index} className="flex gap-3 p-3 bg-muted/30 rounded-lg">
                  {getAlertIcon(alert.type)}
                  <div className="flex-1">
                    <p className="text-sm">{alert.message}</p>
                    <p className="text-xs text-muted-foreground mt-1">{alert.time}</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Quick Actions */}
        <Card className="border-0 shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg font-semibold">Quick Actions</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-3">
              <Button variant="outline" className="h-20 flex-col gap-2" onClick={() => navigate("/transport/routes")}>
                <Route className="h-5 w-5" />
                <span>Manage Routes</span>
              </Button>
              <Button variant="outline" className="h-20 flex-col gap-2" onClick={() => navigate("/transport/vehicles")}>
                <Bus className="h-5 w-5" />
                <span>Manage Vehicles</span>
              </Button>
              <Button variant="outline" className="h-20 flex-col gap-2" onClick={() => navigate("/transport/drivers")}>
                <Users className="h-5 w-5" />
                <span>Manage Drivers</span>
              </Button>
              <Button variant="outline" className="h-20 flex-col gap-2" onClick={() => navigate("/transport/reports")}>
                <Fuel className="h-5 w-5" />
                <span>View Reports</span>
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Upcoming Maintenance */}
        <Card className="border-0 shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg font-semibold">Upcoming Maintenance</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {upcomingMaintenance.map((item, index) => (
                <div key={index} className="flex items-center justify-between p-3 bg-muted/30 rounded-lg">
                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-amber-100 rounded-lg">
                      <AlertTriangle className="h-4 w-4 text-amber-600" />
                    </div>
                    <div>
                      <p className="font-medium text-sm">{item.vehicle}</p>
                      <p className="text-xs text-muted-foreground">{item.type}</p>
                    </div>
                  </div>
                  <Badge variant="outline">{item.date}</Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default TransportDashboard;
