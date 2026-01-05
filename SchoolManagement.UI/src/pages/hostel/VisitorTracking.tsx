import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  UserCheck, 
  Plus, 
  Search,
  Clock,
  Phone,
  User,
  CheckCircle,
  XCircle,
  LogIn,
  LogOut,
  Filter,
  Calendar
} from "lucide-react";

const VisitorTracking = () => {
  const [filterStatus, setFilterStatus] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");

  const visitors = [
    { 
      id: 1, 
      name: "Rajesh Sharma", 
      relation: "Father", 
      student: "Rohan Sharma", 
      room: "A-101",
      phone: "9876543210",
      purpose: "Monthly Visit",
      checkIn: "10:30 AM",
      checkOut: null,
      status: "inside",
      date: "2024-01-15"
    },
    { 
      id: 2, 
      name: "Anita Verma", 
      relation: "Mother", 
      student: "Aisha Verma", 
      room: "C-102",
      phone: "9812345678",
      purpose: "Medical Emergency",
      checkIn: "09:15 AM",
      checkOut: "11:30 AM",
      status: "left",
      date: "2024-01-15"
    },
    { 
      id: 3, 
      name: "Imran Khan", 
      relation: "Father", 
      student: "Kabir Khan", 
      room: "A-105",
      phone: "9901122334",
      purpose: "Document Submission",
      checkIn: null,
      checkOut: null,
      status: "pending",
      date: "2024-01-15"
    },
    { 
      id: 4, 
      name: "Sunita Patel", 
      relation: "Mother", 
      student: "Priya Patel", 
      room: "C-103",
      phone: "9823456789",
      purpose: "Birthday Celebration",
      checkIn: "02:00 PM",
      checkOut: null,
      status: "inside",
      date: "2024-01-15"
    },
    { 
      id: 5, 
      name: "Ramesh Gupta", 
      relation: "Uncle", 
      student: "Amit Gupta", 
      room: "B-201",
      phone: "9734567890",
      purpose: "Relative Visit",
      checkIn: null,
      checkOut: null,
      status: "rejected",
      date: "2024-01-15"
    },
  ];

  const visitorHistory = [
    { id: 1, name: "Suresh Kumar", student: "Vikram Kumar", date: "2024-01-14", duration: "2h 30m" },
    { id: 2, name: "Meena Singh", student: "Raj Singh", date: "2024-01-14", duration: "1h 45m" },
    { id: 3, name: "Pankaj Joshi", student: "Arjun Joshi", date: "2024-01-13", duration: "3h 15m" },
    { id: 4, name: "Rekha Sharma", student: "Neha Sharma", date: "2024-01-13", duration: "2h 00m" },
  ];

  const stats = [
    { title: "Today's Visitors", value: "12", icon: UserCheck, color: "text-blue-500" },
    { title: "Currently Inside", value: "5", icon: LogIn, color: "text-green-500" },
    { title: "Pending Approval", value: "2", icon: Clock, color: "text-amber-500" },
    { title: "Left Today", value: "7", icon: LogOut, color: "text-purple-500" },
  ];

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "inside":
        return <Badge className="bg-green-500 hover:bg-green-600">Inside</Badge>;
      case "left":
        return <Badge variant="secondary">Left</Badge>;
      case "pending":
        return <Badge variant="outline" className="text-amber-600 border-amber-300">Pending</Badge>;
      case "rejected":
        return <Badge variant="destructive">Rejected</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  const filteredVisitors = visitors.filter(visitor => 
    (filterStatus === "all" || visitor.status === filterStatus) &&
    (visitor.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
     visitor.student.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Visitor Tracking</h1>
          <p className="text-muted-foreground">Manage and track hostel visitors</p>
        </div>
        <Dialog>
          <DialogTrigger asChild>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Register Visitor
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-lg">
            <DialogHeader>
              <DialogTitle>Register New Visitor</DialogTitle>
            </DialogHeader>
            <div className="space-y-4 pt-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Visitor Name</Label>
                  <Input placeholder="Full name" />
                </div>
                <div className="space-y-2">
                  <Label>Phone Number</Label>
                  <Input placeholder="Mobile number" />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Relation</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select relation" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="father">Father</SelectItem>
                      <SelectItem value="mother">Mother</SelectItem>
                      <SelectItem value="guardian">Guardian</SelectItem>
                      <SelectItem value="sibling">Sibling</SelectItem>
                      <SelectItem value="relative">Relative</SelectItem>
                      <SelectItem value="other">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>ID Type</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select ID" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="aadhar">Aadhar Card</SelectItem>
                      <SelectItem value="pan">PAN Card</SelectItem>
                      <SelectItem value="driving">Driving License</SelectItem>
                      <SelectItem value="passport">Passport</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="space-y-2">
                <Label>ID Number</Label>
                <Input placeholder="Enter ID number" />
              </div>
              <div className="space-y-2">
                <Label>Student to Visit</Label>
                <Select>
                  <SelectTrigger>
                    <SelectValue placeholder="Select student" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="s1">Rohan Sharma - A-101</SelectItem>
                    <SelectItem value="s2">Aisha Verma - C-102</SelectItem>
                    <SelectItem value="s3">Kabir Khan - A-105</SelectItem>
                    <SelectItem value="s4">Priya Patel - C-103</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Purpose of Visit</Label>
                <Textarea placeholder="Reason for visiting..." rows={2} />
              </div>
              <Button className="w-full">Register Visitor</Button>
            </div>
          </DialogContent>
        </Dialog>
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
                <stat.icon className={`h-8 w-8 ${stat.color}`} />
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Tabs defaultValue="today" className="space-y-4">
        <TabsList>
          <TabsTrigger value="today">
            <Calendar className="h-4 w-4 mr-2" />
            Today's Visitors
          </TabsTrigger>
          <TabsTrigger value="pending">
            <Clock className="h-4 w-4 mr-2" />
            Pending Approval
          </TabsTrigger>
          <TabsTrigger value="history">
            <UserCheck className="h-4 w-4 mr-2" />
            History
          </TabsTrigger>
        </TabsList>

        <TabsContent value="today">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Visitor Log - Today</CardTitle>
                <div className="flex gap-2">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input 
                      placeholder="Search visitors..." 
                      className="pl-9 w-64"
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                    />
                  </div>
                  <Select value={filterStatus} onValueChange={setFilterStatus}>
                    <SelectTrigger className="w-40">
                      <Filter className="h-4 w-4 mr-2" />
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Status</SelectItem>
                      <SelectItem value="inside">Inside</SelectItem>
                      <SelectItem value="left">Left</SelectItem>
                      <SelectItem value="pending">Pending</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Visitor</TableHead>
                    <TableHead>Student</TableHead>
                    <TableHead>Room</TableHead>
                    <TableHead>Purpose</TableHead>
                    <TableHead>Check In</TableHead>
                    <TableHead>Check Out</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredVisitors.map((visitor) => (
                    <TableRow key={visitor.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{visitor.name}</p>
                          <p className="text-sm text-muted-foreground">{visitor.relation}</p>
                        </div>
                      </TableCell>
                      <TableCell>{visitor.student}</TableCell>
                      <TableCell>{visitor.room}</TableCell>
                      <TableCell className="text-muted-foreground">{visitor.purpose}</TableCell>
                      <TableCell>{visitor.checkIn || "-"}</TableCell>
                      <TableCell>{visitor.checkOut || "-"}</TableCell>
                      <TableCell>{getStatusBadge(visitor.status)}</TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          {visitor.status === "pending" && (
                            <>
                              <Button variant="ghost" size="icon" className="text-green-600">
                                <CheckCircle className="h-4 w-4" />
                              </Button>
                              <Button variant="ghost" size="icon" className="text-destructive">
                                <XCircle className="h-4 w-4" />
                              </Button>
                            </>
                          )}
                          {visitor.status === "inside" && (
                            <Button variant="outline" size="sm">
                              <LogOut className="h-4 w-4 mr-1" />
                              Check Out
                            </Button>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="pending">
          <Card>
            <CardHeader>
              <CardTitle>Pending Visitor Approvals</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {visitors.filter(v => v.status === "pending").map((visitor) => (
                  <div key={visitor.id} className="flex items-center justify-between p-4 rounded-lg border bg-muted/30">
                    <div className="flex items-center gap-4">
                      <div className="h-12 w-12 rounded-full bg-primary/10 flex items-center justify-center">
                        <User className="h-6 w-6 text-primary" />
                      </div>
                      <div>
                        <p className="font-medium">{visitor.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {visitor.relation} of {visitor.student} ({visitor.room})
                        </p>
                        <div className="flex items-center gap-2 mt-1 text-sm text-muted-foreground">
                          <Phone className="h-3 w-3" />
                          {visitor.phone}
                        </div>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">{visitor.purpose}</p>
                      <div className="flex gap-2 mt-2">
                        <Button size="sm" className="bg-green-600 hover:bg-green-700">
                          <CheckCircle className="h-4 w-4 mr-1" />
                          Approve
                        </Button>
                        <Button size="sm" variant="destructive">
                          <XCircle className="h-4 w-4 mr-1" />
                          Reject
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
                {visitors.filter(v => v.status === "pending").length === 0 && (
                  <p className="text-center text-muted-foreground py-8">No pending approvals</p>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="history">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Visitor History</CardTitle>
                <div className="flex gap-2">
                  <Input type="date" className="w-40" />
                  <Input type="date" className="w-40" />
                  <Button variant="outline">
                    <Search className="h-4 w-4 mr-2" />
                    Search
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Visitor Name</TableHead>
                    <TableHead>Student Visited</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Duration</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {visitorHistory.map((visit) => (
                    <TableRow key={visit.id}>
                      <TableCell className="font-medium">{visit.name}</TableCell>
                      <TableCell>{visit.student}</TableCell>
                      <TableCell>{visit.date}</TableCell>
                      <TableCell>{visit.duration}</TableCell>
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

export default VisitorTracking;
