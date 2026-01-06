import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { 
  Building2, 
  Users, 
  Utensils, 
  UserCheck, 
  AlertTriangle,
  BedDouble,
  DoorOpen,
  Wrench
} from "lucide-react";

const HostelDashboard = () => {
  const stats = [
    { title: "Total Rooms", value: "120", icon: Building2, color: "text-blue-500", bgColor: "bg-blue-50" },
    { title: "Occupied Beds", value: "385", icon: BedDouble, color: "text-green-500", bgColor: "bg-green-50" },
    { title: "Vacant Beds", value: "95", icon: DoorOpen, color: "text-amber-500", bgColor: "bg-amber-50" },
    { title: "Today's Visitors", value: "12", icon: UserCheck, color: "text-purple-500", bgColor: "bg-purple-50" },
  ];

  const blocks = [
    { name: "Block A - Boys", total: 60, occupied: 52, maintenance: 2 },
    { name: "Block B - Boys", total: 60, occupied: 55, maintenance: 1 },
    { name: "Block C - Girls", total: 50, occupied: 45, maintenance: 0 },
    { name: "Block D - Girls", total: 50, occupied: 48, maintenance: 1 },
  ];

  const recentActivities = [
    { type: "check-in", student: "Rohan Sharma", room: "A-105", time: "10:30 AM" },
    { type: "visitor", visitor: "Mr. Rajesh Sharma", student: "Rohan Sharma", time: "11:00 AM" },
    { type: "maintenance", room: "B-203", issue: "AC Repair", time: "09:15 AM" },
    { type: "check-out", student: "Priya Patel", room: "C-112", time: "08:00 AM" },
  ];

  const todayMeals = [
    { meal: "Breakfast", time: "7:30 - 9:00 AM", menu: "Poha, Bread Toast, Milk, Fruits", status: "completed" },
    { meal: "Lunch", time: "12:30 - 2:00 PM", menu: "Rice, Dal, Paneer Curry, Salad, Roti", status: "ongoing" },
    { meal: "Snacks", time: "5:00 - 6:00 PM", menu: "Samosa, Tea, Biscuits", status: "upcoming" },
    { meal: "Dinner", time: "7:30 - 9:00 PM", menu: "Rice, Rajma, Mixed Veg, Chapati", status: "upcoming" },
  ];

  const alerts = [
    { type: "warning", message: "3 rooms pending maintenance inspection", time: "2 hours ago" },
    { type: "info", message: "Mess menu updated for next week", time: "5 hours ago" },
    { type: "alert", message: "2 visitors waiting for approval", time: "30 mins ago" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Hostel Dashboard</h1>
          <p className="text-muted-foreground">Overview of hostel operations and status</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Wrench className="h-4 w-4 mr-2" />
            Maintenance
          </Button>
          <Button>
            <Users className="h-4 w-4 mr-2" />
            New Admission
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, index) => (
          <Card key={index}>
            <CardContent className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-muted-foreground">{stat.title}</p>
                  <p className="text-3xl font-bold mt-1">{stat.value}</p>
                </div>
                <div className={`p-3 rounded-full ${stat.bgColor}`}>
                  <stat.icon className={`h-6 w-6 ${stat.color}`} />
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Block Occupancy */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Block-wise Occupancy
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {blocks.map((block, index) => (
              <div key={index} className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{block.name}</span>
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <span>{block.occupied}/{block.total} beds</span>
                    {block.maintenance > 0 && (
                      <Badge variant="outline" className="text-amber-600 border-amber-300">
                        <Wrench className="h-3 w-3 mr-1" />
                        {block.maintenance} in maintenance
                      </Badge>
                    )}
                  </div>
                </div>
                <Progress value={(block.occupied / block.total) * 100} className="h-2" />
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Alerts */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5" />
              Alerts & Notifications
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {alerts.map((alert, index) => (
              <div 
                key={index} 
                className={`p-3 rounded-lg border ${
                  alert.type === 'alert' ? 'bg-red-50 border-red-200' :
                  alert.type === 'warning' ? 'bg-amber-50 border-amber-200' :
                  'bg-blue-50 border-blue-200'
                }`}
              >
                <p className="text-sm font-medium">{alert.message}</p>
                <p className="text-xs text-muted-foreground mt-1">{alert.time}</p>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Today's Meals */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Utensils className="h-5 w-5" />
              Today's Mess Schedule
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {todayMeals.map((meal, index) => (
              <div key={index} className="flex items-start justify-between p-3 rounded-lg bg-muted/50">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{meal.meal}</span>
                    <Badge 
                      variant={meal.status === 'completed' ? 'secondary' : 
                               meal.status === 'ongoing' ? 'default' : 'outline'}
                    >
                      {meal.status}
                    </Badge>
                  </div>
                  <p className="text-sm text-muted-foreground">{meal.time}</p>
                  <p className="text-sm mt-1">{meal.menu}</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Recent Activities */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              Recent Activities
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {recentActivities.map((activity, index) => (
              <div key={index} className="flex items-center gap-3 p-3 rounded-lg bg-muted/50">
                <div className={`p-2 rounded-full ${
                  activity.type === 'check-in' ? 'bg-green-100' :
                  activity.type === 'check-out' ? 'bg-blue-100' :
                  activity.type === 'visitor' ? 'bg-purple-100' :
                  'bg-amber-100'
                }`}>
                  {activity.type === 'check-in' && <DoorOpen className="h-4 w-4 text-green-600" />}
                  {activity.type === 'check-out' && <DoorOpen className="h-4 w-4 text-blue-600" />}
                  {activity.type === 'visitor' && <UserCheck className="h-4 w-4 text-purple-600" />}
                  {activity.type === 'maintenance' && <Wrench className="h-4 w-4 text-amber-600" />}
                </div>
                <div className="flex-1">
                  {activity.type === 'check-in' && (
                    <p className="text-sm"><span className="font-medium">{activity.student}</span> checked into Room {activity.room}</p>
                  )}
                  {activity.type === 'check-out' && (
                    <p className="text-sm"><span className="font-medium">{activity.student}</span> checked out from Room {activity.room}</p>
                  )}
                  {activity.type === 'visitor' && (
                    <p className="text-sm"><span className="font-medium">{activity.visitor}</span> visited {activity.student}</p>
                  )}
                  {activity.type === 'maintenance' && (
                    <p className="text-sm">Room {activity.room}: <span className="font-medium">{activity.issue}</span></p>
                  )}
                  <p className="text-xs text-muted-foreground">{activity.time}</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default HostelDashboard;
