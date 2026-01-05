import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { EmployeeAttendance } from "@/types/navigation.types";
import { useToast } from "@/hooks/use-toast";
import { 
  Clock, Search, Download, Plus, CalendarIcon, 
  TrendingUp, Users, CheckCircle, XCircle, AlertCircle,
  ArrowUpDown, ArrowUp, ArrowDown
} from "lucide-react";
import { format } from "date-fns";

export default function AttendanceManagement() {
  const [attendanceRecords, setAttendanceRecords] = useState<EmployeeAttendance[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isMarkDialogOpen, setIsMarkDialogOpen] = useState(false);
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const { toast } = useToast();

  useEffect(() => {
    loadAttendance();
  }, [selectedDate]);

  const loadAttendance = () => {
    // Mock data - replace with actual API call
    const mockData: EmployeeAttendance[] = [
      {
        id: "1",
        employeeId: "EMP001",
        employeeName: "John Doe",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "09:00 AM",
        checkOut: "05:30 PM",
        status: "present",
        workHours: 8.5,
        overtimeHours: 0.5,
        location: "Main Office",
        notes: "Regular attendance",
        createdAt: new Date().toISOString(),
      },
      {
        id: "2",
        employeeId: "EMP002",
        employeeName: "Jane Smith",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "09:15 AM",
        checkOut: "05:00 PM",
        status: "late",
        workHours: 7.75,
        overtimeHours: 0,
        location: "Main Office",
        notes: "Late arrival - traffic",
        createdAt: new Date().toISOString(),
      },
      {
        id: "3",
        employeeId: "EMP003",
        employeeName: "Michael Brown",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "09:00 AM",
        checkOut: "01:00 PM",
        status: "half-day",
        workHours: 4,
        overtimeHours: 0,
        location: "Main Office",
        notes: "Half day - personal work",
        createdAt: new Date().toISOString(),
      },
      {
        id: "4",
        employeeId: "EMP004",
        employeeName: "Emily Davis",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "",
        checkOut: "",
        status: "absent",
        workHours: 0,
        overtimeHours: 0,
        location: "",
        notes: "Unplanned absence",
        createdAt: new Date().toISOString(),
      },
      {
        id: "5",
        employeeId: "EMP005",
        employeeName: "David Wilson",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "",
        checkOut: "",
        status: "on-leave",
        workHours: 0,
        overtimeHours: 0,
        location: "",
        notes: "Approved sick leave",
        createdAt: new Date().toISOString(),
      },
      {
        id: "6",
        employeeId: "EMP006",
        employeeName: "Sarah Johnson",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "08:45 AM",
        checkOut: "06:30 PM",
        status: "present",
        workHours: 9.75,
        overtimeHours: 1.75,
        location: "Main Office",
        notes: "Extra hours for project deadline",
        createdAt: new Date().toISOString(),
      },
      {
        id: "7",
        employeeId: "EMP007",
        employeeName: "Robert Taylor",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "09:00 AM",
        checkOut: "05:00 PM",
        status: "present",
        workHours: 8,
        overtimeHours: 0,
        location: "Main Office",
        createdAt: new Date().toISOString(),
      },
      {
        id: "8",
        employeeId: "EMP008",
        employeeName: "Lisa Anderson",
        date: format(selectedDate, "yyyy-MM-dd"),
        checkIn: "09:30 AM",
        checkOut: "05:00 PM",
        status: "late",
        workHours: 7.5,
        overtimeHours: 0,
        location: "Branch Office",
        notes: "Working from branch office",
        createdAt: new Date().toISOString(),
      },
    ];
    setAttendanceRecords(mockData);
  };

  const handleSort = (column: string) => {
    if (sortColumn === column) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortColumn(column);
      setSortDirection("asc");
    }
  };

  const getSortIcon = (column: string) => {
    if (sortColumn !== column) return <ArrowUpDown className="h-4 w-4 ml-1" />;
    return sortDirection === "asc" ? <ArrowUp className="h-4 w-4 ml-1" /> : <ArrowDown className="h-4 w-4 ml-1" />;
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, { bg: string; icon: React.ReactNode }> = {
      present: { bg: "bg-status-present", icon: <CheckCircle className="h-3 w-3 mr-1" /> },
      absent: { bg: "bg-status-absent", icon: <XCircle className="h-3 w-3 mr-1" /> },
      late: { bg: "bg-amber-500", icon: <AlertCircle className="h-3 w-3 mr-1" /> },
      "half-day": { bg: "bg-blue-500", icon: <Clock className="h-3 w-3 mr-1" /> },
      "on-leave": { bg: "bg-purple-500", icon: <CalendarIcon className="h-3 w-3 mr-1" /> },
    };
    const style = styles[status] || { bg: "bg-muted", icon: null };
    return (
      <Badge className={`${style.bg} flex items-center w-fit`}>
        {style.icon}
        {status}
      </Badge>
    );
  };

  const filteredRecords = attendanceRecords
    .filter(record => {
      const matchesSearch = record.employeeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          record.employeeId.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter === "all" || record.status === statusFilter;
      return matchesSearch && matchesStatus;
    })
    .sort((a, b) => {
      if (!sortColumn) return 0;
      const aValue: any = a[sortColumn as keyof typeof a];
      const bValue: any = b[sortColumn as keyof typeof b];
      if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
      if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });

  const stats = {
    total: attendanceRecords.length,
    present: attendanceRecords.filter(r => r.status === "present").length,
    absent: attendanceRecords.filter(r => r.status === "absent").length,
    late: attendanceRecords.filter(r => r.status === "late").length,
    onLeave: attendanceRecords.filter(r => r.status === "on-leave").length,
    attendanceRate: attendanceRecords.length > 0 
      ? ((attendanceRecords.filter(r => r.status === "present" || r.status === "late" || r.status === "half-day").length / attendanceRecords.length) * 100).toFixed(1)
      : 0,
  };

  const handleBulkMarkAttendance = () => {
    toast({
      title: "Success",
      description: "Attendance marked for selected employees",
    });
    setIsMarkDialogOpen(false);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Attendance Management</h1>
          <p className="text-muted-foreground">Track and manage employee attendance</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Export
          </Button>
          <Dialog open={isMarkDialogOpen} onOpenChange={setIsMarkDialogOpen}>
            <DialogTrigger asChild>
              <Button className="bg-gradient-primary">
                <Plus className="mr-2 h-4 w-4" />
                Mark Attendance
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Bulk Mark Attendance</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label>Date</Label>
                  <Input type="date" defaultValue={format(new Date(), "yyyy-MM-dd")} />
                </div>
                <div className="space-y-2">
                  <Label>Default Status</Label>
                  <Select defaultValue="present">
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="present">Present</SelectItem>
                      <SelectItem value="absent">Absent</SelectItem>
                      <SelectItem value="half-day">Half Day</SelectItem>
                      <SelectItem value="on-leave">On Leave</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <Button onClick={handleBulkMarkAttendance} className="w-full bg-gradient-primary">
                  Mark All
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-6">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Users className="h-4 w-4" />
              Total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <CheckCircle className="h-4 w-4" />
              Present
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-present">{stats.present}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <XCircle className="h-4 w-4" />
              Absent
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-absent">{stats.absent}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <AlertCircle className="h-4 w-4" />
              Late
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-amber-500">{stats.late}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <CalendarIcon className="h-4 w-4" />
              On Leave
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-purple-500">{stats.onLeave}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <TrendingUp className="h-4 w-4" />
              Rate
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.attendanceRate}%</div>
          </CardContent>
        </Card>
      </div>

      {/* Filters and Table */}
      <Card>
        <CardHeader>
          <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              Attendance Records
            </CardTitle>
            <div className="flex flex-wrap gap-2 items-center w-full md:w-auto">
              <Popover>
                <PopoverTrigger asChild>
                  <Button variant="outline" className="w-[200px] justify-start">
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {format(selectedDate, "PPP")}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={selectedDate}
                    onSelect={(date) => date && setSelectedDate(date)}
                  />
                </PopoverContent>
              </Popover>
              
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-[150px]">
                  <SelectValue placeholder="Filter by status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="present">Present</SelectItem>
                  <SelectItem value="absent">Absent</SelectItem>
                  <SelectItem value="late">Late</SelectItem>
                  <SelectItem value="half-day">Half Day</SelectItem>
                  <SelectItem value="on-leave">On Leave</SelectItem>
                </SelectContent>
              </Select>

              <div className="flex items-center gap-2">
                <Search className="h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search employees..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-[200px]"
                />
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("employeeId")}
                  >
                    <div className="flex items-center">
                      Employee ID
                      {getSortIcon("employeeId")}
                    </div>
                  </TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("employeeName")}
                  >
                    <div className="flex items-center">
                      Name
                      {getSortIcon("employeeName")}
                    </div>
                  </TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("checkIn")}
                  >
                    <div className="flex items-center">
                      Check In
                      {getSortIcon("checkIn")}
                    </div>
                  </TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("checkOut")}
                  >
                    <div className="flex items-center">
                      Check Out
                      {getSortIcon("checkOut")}
                    </div>
                  </TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("workHours")}
                  >
                    <div className="flex items-center">
                      Work Hours
                      {getSortIcon("workHours")}
                    </div>
                  </TableHead>
                  <TableHead>Overtime</TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("status")}
                  >
                    <div className="flex items-center">
                      Status
                      {getSortIcon("status")}
                    </div>
                  </TableHead>
                  <TableHead>Location</TableHead>
                  <TableHead>Notes</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredRecords.map((record) => (
                  <TableRow key={record.id}>
                    <TableCell className="font-medium">{record.employeeId}</TableCell>
                    <TableCell>{record.employeeName}</TableCell>
                    <TableCell>{record.checkIn || "-"}</TableCell>
                    <TableCell>{record.checkOut || "-"}</TableCell>
                    <TableCell>{record.workHours}h</TableCell>
                    <TableCell>{record.overtimeHours > 0 ? `${record.overtimeHours}h` : "-"}</TableCell>
                    <TableCell>{getStatusBadge(record.status)}</TableCell>
                    <TableCell>{record.location || "-"}</TableCell>
                    <TableCell className="text-sm text-muted-foreground">{record.notes || "-"}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
