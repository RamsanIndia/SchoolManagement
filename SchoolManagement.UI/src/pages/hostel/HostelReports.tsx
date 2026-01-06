import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  BarChart3, 
  Download, 
  Building2,
  Users,
  Utensils,
  CreditCard,
  TrendingUp,
  TrendingDown,
  Calendar,
  FileText
} from "lucide-react";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, LineChart, Line } from 'recharts';

const HostelReports = () => {
  const occupancyData = [
    { month: "Aug", occupancy: 82 },
    { month: "Sep", occupancy: 88 },
    { month: "Oct", occupancy: 92 },
    { month: "Nov", occupancy: 95 },
    { month: "Dec", occupancy: 78 },
    { month: "Jan", occupancy: 90 },
  ];

  const blockOccupancy = [
    { name: "Block A", value: 87, color: "#3B82F6" },
    { name: "Block B", value: 92, color: "#10B981" },
    { name: "Block C", value: 90, color: "#F59E0B" },
    { name: "Block D", value: 96, color: "#8B5CF6" },
  ];

  const feeCollection = [
    { month: "Aug", collected: 450000, pending: 50000 },
    { month: "Sep", collected: 480000, pending: 20000 },
    { month: "Oct", collected: 490000, pending: 10000 },
    { month: "Nov", collected: 475000, pending: 25000 },
    { month: "Dec", collected: 420000, pending: 80000 },
    { month: "Jan", collected: 460000, pending: 40000 },
  ];

  const mealStats = [
    { meal: "Breakfast", avgAttendance: 340, rating: 4.2 },
    { meal: "Lunch", avgAttendance: 380, rating: 4.0 },
    { meal: "Snacks", avgAttendance: 290, rating: 4.5 },
    { meal: "Dinner", avgAttendance: 365, rating: 4.1 },
  ];

  const maintenanceLog = [
    { id: 1, room: "A-105", issue: "AC Repair", status: "Completed", date: "2024-01-14", cost: 2500 },
    { id: 2, room: "B-203", issue: "Plumbing", status: "In Progress", date: "2024-01-15", cost: 1800 },
    { id: 3, room: "C-112", issue: "Electrical", status: "Pending", date: "2024-01-15", cost: 1200 },
    { id: 4, room: "D-101", issue: "Furniture", status: "Completed", date: "2024-01-13", cost: 3500 },
  ];

  const feeDefaulters = [
    { id: 1, student: "Rahul Verma", room: "A-102", pending: 15000, months: 2 },
    { id: 2, student: "Sneha Gupta", room: "C-105", pending: 7500, months: 1 },
    { id: 3, student: "Amit Kumar", room: "B-201", pending: 22500, months: 3 },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Hostel Reports</h1>
          <p className="text-muted-foreground">Analytics and reports for hostel operations</p>
        </div>
        <div className="flex gap-2">
          <Select defaultValue="month">
            <SelectTrigger className="w-40">
              <Calendar className="h-4 w-4 mr-2" />
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="week">This Week</SelectItem>
              <SelectItem value="month">This Month</SelectItem>
              <SelectItem value="quarter">This Quarter</SelectItem>
              <SelectItem value="year">This Year</SelectItem>
            </SelectContent>
          </Select>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
        </div>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Occupancy Rate</p>
                <p className="text-3xl font-bold mt-1">91%</p>
                <p className="text-sm text-green-600 flex items-center mt-1">
                  <TrendingUp className="h-4 w-4 mr-1" />
                  +3% from last month
                </p>
              </div>
              <Building2 className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Students</p>
                <p className="text-3xl font-bold mt-1">385</p>
                <p className="text-sm text-green-600 flex items-center mt-1">
                  <TrendingUp className="h-4 w-4 mr-1" />
                  +12 new admissions
                </p>
              </div>
              <Users className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Fee Collection</p>
                <p className="text-3xl font-bold mt-1">₹4.6L</p>
                <p className="text-sm text-amber-600 flex items-center mt-1">
                  <TrendingDown className="h-4 w-4 mr-1" />
                  ₹40K pending
                </p>
              </div>
              <CreditCard className="h-8 w-8 text-purple-500" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Mess Rating</p>
                <p className="text-3xl font-bold mt-1">4.2/5</p>
                <p className="text-sm text-green-600 flex items-center mt-1">
                  <TrendingUp className="h-4 w-4 mr-1" />
                  +0.3 improvement
                </p>
              </div>
              <Utensils className="h-8 w-8 text-amber-500" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="occupancy" className="space-y-4">
        <TabsList>
          <TabsTrigger value="occupancy">
            <Building2 className="h-4 w-4 mr-2" />
            Occupancy
          </TabsTrigger>
          <TabsTrigger value="fees">
            <CreditCard className="h-4 w-4 mr-2" />
            Fees
          </TabsTrigger>
          <TabsTrigger value="mess">
            <Utensils className="h-4 w-4 mr-2" />
            Mess
          </TabsTrigger>
          <TabsTrigger value="maintenance">
            <FileText className="h-4 w-4 mr-2" />
            Maintenance
          </TabsTrigger>
        </TabsList>

        <TabsContent value="occupancy">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Monthly Occupancy Trend</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-80">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={occupancyData}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis domain={[0, 100]} />
                      <Tooltip formatter={(value) => [`${value}%`, "Occupancy"]} />
                      <Line 
                        type="monotone" 
                        dataKey="occupancy" 
                        stroke="#3B82F6" 
                        strokeWidth={2}
                        dot={{ fill: "#3B82F6" }}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Block-wise Occupancy</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-80">
                  <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                      <Pie
                        data={blockOccupancy}
                        cx="50%"
                        cy="50%"
                        innerRadius={60}
                        outerRadius={100}
                        paddingAngle={5}
                        dataKey="value"
                        label={({ name, value }) => `${name}: ${value}%`}
                      >
                        {blockOccupancy.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={entry.color} />
                        ))}
                      </Pie>
                      <Tooltip formatter={(value) => [`${value}%`, "Occupancy"]} />
                    </PieChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="fees">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Fee Collection Trend</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-80">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={feeCollection}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="month" />
                      <YAxis />
                      <Tooltip formatter={(value) => [`₹${(value as number / 1000).toFixed(0)}K`, ""]} />
                      <Bar dataKey="collected" fill="#10B981" name="Collected" />
                      <Bar dataKey="pending" fill="#F59E0B" name="Pending" />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Fee Defaulters</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Student</TableHead>
                      <TableHead>Room</TableHead>
                      <TableHead>Pending</TableHead>
                      <TableHead>Months</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {feeDefaulters.map((defaulter) => (
                      <TableRow key={defaulter.id}>
                        <TableCell className="font-medium">{defaulter.student}</TableCell>
                        <TableCell>{defaulter.room}</TableCell>
                        <TableCell className="text-red-600">₹{defaulter.pending.toLocaleString()}</TableCell>
                        <TableCell>
                          <Badge variant={defaulter.months >= 3 ? "destructive" : "outline"}>
                            {defaulter.months} month{defaulter.months > 1 ? 's' : ''}
                          </Badge>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="mess">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Meal Attendance Statistics</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-80">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={mealStats} layout="vertical">
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis type="number" />
                      <YAxis dataKey="meal" type="category" width={80} />
                      <Tooltip />
                      <Bar dataKey="avgAttendance" fill="#3B82F6" name="Avg. Attendance" />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Meal Ratings</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {mealStats.map((meal, index) => (
                    <div key={index} className="flex items-center justify-between p-4 rounded-lg bg-muted/50">
                      <div>
                        <p className="font-medium">{meal.meal}</p>
                        <p className="text-sm text-muted-foreground">
                          Avg. {meal.avgAttendance} students
                        </p>
                      </div>
                      <div className="text-right">
                        <p className={`text-2xl font-bold ${
                          meal.rating >= 4.3 ? 'text-green-600' :
                          meal.rating >= 4 ? 'text-blue-600' :
                          'text-amber-600'
                        }`}>
                          {meal.rating}
                        </p>
                        <p className="text-sm text-muted-foreground">out of 5</p>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="maintenance">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Maintenance Log</CardTitle>
                <Button variant="outline" size="sm">
                  <Download className="h-4 w-4 mr-2" />
                  Export Report
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Room</TableHead>
                    <TableHead>Issue</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Cost</TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {maintenanceLog.map((log) => (
                    <TableRow key={log.id}>
                      <TableCell className="font-medium">{log.room}</TableCell>
                      <TableCell>{log.issue}</TableCell>
                      <TableCell>{log.date}</TableCell>
                      <TableCell>₹{log.cost.toLocaleString()}</TableCell>
                      <TableCell>
                        <Badge 
                          variant={
                            log.status === "Completed" ? "default" :
                            log.status === "In Progress" ? "secondary" :
                            "outline"
                          }
                          className={log.status === "Completed" ? "bg-green-500" : ""}
                        >
                          {log.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              <div className="mt-4 p-4 rounded-lg bg-muted/50">
                <div className="flex items-center justify-between">
                  <span className="font-medium">Total Maintenance Cost (This Month)</span>
                  <span className="text-xl font-bold">₹9,000</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default HostelReports;
