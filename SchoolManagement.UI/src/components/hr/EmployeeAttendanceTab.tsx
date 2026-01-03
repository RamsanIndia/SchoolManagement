import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { EmployeeAttendance, AttendanceSummary } from "@/types/navigation.types";
import { Calendar, Clock, TrendingUp } from "lucide-react";

interface Props {
  employeeId: string;
}

export default function EmployeeAttendanceTab({ employeeId }: Props) {
  const [attendance, setAttendance] = useState<EmployeeAttendance[]>([]);
  const [summary, setSummary] = useState<AttendanceSummary | null>(null);

  useEffect(() => {
    // Mock data - replace with actual API call
    const mockAttendance: EmployeeAttendance[] = [
      {
        id: "1",
        employeeId,
        employeeName: "John Doe",
        date: "2024-01-15",
        checkIn: "09:00 AM",
        checkOut: "05:30 PM",
        status: "present",
        workHours: 8.5,
        overtimeHours: 0.5,
        location: "Main Office",
        createdAt: "2024-01-15T09:00:00Z",
      },
      {
        id: "2",
        employeeId,
        employeeName: "John Doe",
        date: "2024-01-14",
        checkIn: "09:15 AM",
        checkOut: "05:00 PM",
        status: "late",
        workHours: 7.75,
        overtimeHours: 0,
        location: "Main Office",
        createdAt: "2024-01-14T09:15:00Z",
      },
    ];

    const mockSummary: AttendanceSummary = {
      employeeId,
      month: "January",
      year: 2024,
      totalWorkingDays: 22,
      presentDays: 20,
      absentDays: 1,
      halfDays: 0,
      lateDays: 1,
      leaveDays: 0,
      totalWorkHours: 168,
      overtimeHours: 4,
      attendancePercentage: 95.5,
    };

    setAttendance(mockAttendance);
    setSummary(mockSummary);
  }, [employeeId]);

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      present: "bg-status-present",
      absent: "bg-status-absent",
      late: "bg-amber-500",
      "half-day": "bg-blue-500",
      "on-leave": "bg-purple-500",
    };
    return <Badge className={colors[status] || "bg-muted"}>{status}</Badge>;
  };

  return (
    <div className="space-y-4">
      {/* Summary Cards */}
      {summary && (
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Present Days
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summary.presentDays}</div>
              <p className="text-xs text-muted-foreground">
                of {summary.totalWorkingDays} working days
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <TrendingUp className="h-4 w-4" />
                Attendance Rate
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summary.attendancePercentage}%</div>
              <p className="text-xs text-muted-foreground">
                {summary.month} {summary.year}
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Clock className="h-4 w-4" />
                Work Hours
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summary.totalWorkHours}h</div>
              <p className="text-xs text-muted-foreground">
                +{summary.overtimeHours}h overtime
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Absences
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-status-absent">{summary.absentDays}</div>
              <p className="text-xs text-muted-foreground">
                {summary.lateDays} late arrivals
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Attendance Records Table */}
      <Card>
        <CardHeader>
          <CardTitle>Attendance Records</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Date</TableHead>
                <TableHead>Check In</TableHead>
                <TableHead>Check Out</TableHead>
                <TableHead>Work Hours</TableHead>
                <TableHead>Overtime</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Location</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {attendance.map((record) => (
                <TableRow key={record.id}>
                  <TableCell>{new Date(record.date).toLocaleDateString()}</TableCell>
                  <TableCell>{record.checkIn}</TableCell>
                  <TableCell>{record.checkOut || "-"}</TableCell>
                  <TableCell>{record.workHours}h</TableCell>
                  <TableCell>{record.overtimeHours}h</TableCell>
                  <TableCell>{getStatusBadge(record.status)}</TableCell>
                  <TableCell>{record.location || "-"}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
