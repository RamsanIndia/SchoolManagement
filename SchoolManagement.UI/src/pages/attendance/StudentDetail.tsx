import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Input } from "@/components/ui/input";
import { Search, User, Calendar } from "lucide-react";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

const mockStudentData = {
  studentName: "Ananya Gupta",
  class: "7",
  section: "A",
  rollNo: 12,
  totalDays: 220,
  present: 200,
  absent: 10,
  late: 10,
  percentage: 90.9,
  monthlyAttendance: [
    { month: "Apr", present: 20, absent: 1, late: 2 },
    { month: "May", present: 18, absent: 2, late: 1 },
    { month: "Jun", present: 22, absent: 0, late: 1 },
    { month: "Jul", present: 19, absent: 2, late: 2 },
    { month: "Aug", present: 21, absent: 1, late: 1 },
    { month: "Sep", present: 20, absent: 2, late: 1 },
    { month: "Oct", present: 22, absent: 0, late: 0 },
    { month: "Nov", present: 19, absent: 1, late: 1 },
    { month: "Dec", present: 20, absent: 1, late: 1 },
    { month: "Jan", present: 19, absent: 0, late: 0 },
  ],
  dailyEntries: [
    { date: "2025-01-15", status: "present" },
    { date: "2025-01-14", status: "present" },
    { date: "2025-01-13", status: "late" },
    { date: "2025-01-12", status: "present" },
    { date: "2025-01-11", status: "absent" },
    { date: "2025-01-10", status: "present" },
    { date: "2025-01-09", status: "present" },
    { date: "2025-01-08", status: "late" },
    { date: "2025-01-07", status: "present" },
    { date: "2025-01-06", status: "present" },
  ],
};

export default function StudentDetail() {
  const [searchTerm, setSearchTerm] = useState("");

  const getStatusBadge = (status: string) => {
    const styles = {
      present: "bg-status-present text-white",
      absent: "bg-status-absent text-white",
      late: "bg-status-late text-white",
    };
    return <Badge className={styles[status as keyof typeof styles]}>{status.toUpperCase()}</Badge>;
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Student Attendance Details</h1>
        <p className="text-muted-foreground">Detailed view of individual student attendance records</p>
      </div>

      {/* Search */}
      <Card>
        <CardContent className="pt-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search student by name or roll number..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* Student Card */}
      <Card className="border-2">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarFallback className="bg-gradient-primary text-white text-2xl">
                {mockStudentData.studentName.split(' ').map(n => n[0]).join('')}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h2 className="text-2xl font-bold mb-1">{mockStudentData.studentName}</h2>
              <div className="flex gap-4 text-sm text-muted-foreground">
                <div className="flex items-center gap-1">
                  <User className="h-4 w-4" />
                  <span>Class {mockStudentData.class}-{mockStudentData.section}</span>
                </div>
                <div className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  <span>Roll No: {mockStudentData.rollNo}</span>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Days</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{mockStudentData.totalDays}</div>
            <p className="text-xs text-muted-foreground mt-1">School days</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Present Days</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-present">{mockStudentData.present}</div>
            <p className="text-xs text-muted-foreground mt-1">Days attended</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Absent Days</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-absent">{mockStudentData.absent}</div>
            <p className="text-xs text-muted-foreground mt-1">Days missed</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Late Arrivals</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-late">{mockStudentData.late}</div>
            <p className="text-xs text-muted-foreground mt-1">Times late</p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-primary">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-white/80">Overall %</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-white">{mockStudentData.percentage}%</div>
            <p className="text-xs text-white/70 mt-1">Attendance rate</p>
          </CardContent>
        </Card>
      </div>

      {/* Monthly Chart */}
      <Card>
        <CardHeader>
          <CardTitle>Monthly Attendance Trend</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={mockStudentData.monthlyAttendance}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis dataKey="month" className="text-xs" />
              <YAxis className="text-xs" />
              <Tooltip
                contentStyle={{
                  backgroundColor: 'hsl(var(--card))',
                  border: '1px solid hsl(var(--border))',
                  borderRadius: '8px'
                }}
              />
              <Bar dataKey="present" fill="hsl(142, 76%, 36%)" name="Present" radius={[4, 4, 0, 0]} />
              <Bar dataKey="absent" fill="hsl(0, 84%, 60%)" name="Absent" radius={[4, 4, 0, 0]} />
              <Bar dataKey="late" fill="hsl(25, 95%, 53%)" name="Late" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Recent Attendance */}
      <Card>
        <CardHeader>
          <CardTitle>Recent Attendance Records</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Date</TableHead>
                <TableHead>Day</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {mockStudentData.dailyEntries.map((entry, index) => {
                const date = new Date(entry.date);
                return (
                  <TableRow key={index}>
                    <TableCell className="font-medium">
                      {date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {date.toLocaleDateString('en-US', { weekday: 'long' })}
                    </TableCell>
                    <TableCell>{getStatusBadge(entry.status)}</TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
