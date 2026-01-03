import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Calendar as CalendarIcon, Download, FileText } from "lucide-react";
import { format } from "date-fns";
import { cn } from "@/lib/utils";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useToast } from "@/hooks/use-toast";

const mockDailyReport = [
  { date: "2025-01-15", class: "10", section: "A", present: 42, absent: 5, late: 3, total: 50 },
  { date: "2025-01-15", class: "9", section: "A", present: 35, absent: 6, late: 2, total: 43 },
  { date: "2025-01-15", class: "8", section: "B", present: 38, absent: 4, late: 1, total: 43 },
  { date: "2025-01-15", class: "7", section: "A", present: 40, absent: 3, late: 2, total: 45 },
];

const mockMonthlyReport = [
  { class: "10-A", totalDays: 20, avgPresent: 42, avgAbsent: 5, avgLate: 3, attendance: 84.0 },
  { class: "9-A", totalDays: 20, avgPresent: 36, avgAbsent: 5, avgLate: 2, attendance: 83.7 },
  { class: "8-B", totalDays: 20, avgPresent: 39, avgAbsent: 3, avgLate: 1, attendance: 90.7 },
  { class: "7-A", totalDays: 20, avgPresent: 41, avgAbsent: 3, avgLate: 1, attendance: 91.1 },
];

const mockIndividualReport = [
  { rollNo: 1, name: "Amit Kumar", class: "10-A", present: 18, absent: 1, late: 1, percentage: 90.0 },
  { rollNo: 2, name: "Sana Sharma", class: "10-A", present: 19, absent: 0, late: 1, percentage: 95.0 },
  { rollNo: 3, name: "Vikram Singh", class: "10-A", present: 17, absent: 2, late: 1, percentage: 85.0 },
  { rollNo: 4, name: "Kavya Jain", class: "10-A", present: 20, absent: 0, late: 0, percentage: 100.0 },
];

export default function AttendanceReports() {
  const [reportType, setReportType] = useState("daily");
  const [selectedClass, setSelectedClass] = useState("all");
  const [selectedSection, setSelectedSection] = useState("all");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [date, setDate] = useState<Date>(new Date());
  const { toast } = useToast();

  const handleExport = () => {
    toast({
      title: "Export Initiated",
      description: "Your report is being generated and will download shortly.",
    });
  };

  const getStatusBadge = (status: string) => {
    const styles = {
      present: "bg-status-present text-white",
      absent: "bg-status-absent text-white",
      late: "bg-status-late text-white",
      all: "bg-muted text-foreground",
    };
    return <Badge className={styles[status as keyof typeof styles]}>{status.toUpperCase()}</Badge>;
  };

  const renderDailyReport = () => (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Date</TableHead>
          <TableHead>Class</TableHead>
          <TableHead>Section</TableHead>
          <TableHead className="text-right">Total</TableHead>
          <TableHead className="text-right text-status-present">Present</TableHead>
          <TableHead className="text-right text-status-absent">Absent</TableHead>
          <TableHead className="text-right text-status-late">Late</TableHead>
          <TableHead className="text-right">Attendance %</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {mockDailyReport.map((row, index) => {
          const percentage = (((row.present + row.late) / row.total) * 100).toFixed(1);
          return (
            <TableRow key={index}>
              <TableCell className="font-medium">{row.date}</TableCell>
              <TableCell>Class {row.class}</TableCell>
              <TableCell>Section {row.section}</TableCell>
              <TableCell className="text-right font-semibold">{row.total}</TableCell>
              <TableCell className="text-right text-status-present font-semibold">{row.present}</TableCell>
              <TableCell className="text-right text-status-absent font-semibold">{row.absent}</TableCell>
              <TableCell className="text-right text-status-late font-semibold">{row.late}</TableCell>
              <TableCell className="text-right font-bold">{percentage}%</TableCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );

  const renderMonthlyReport = () => (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Class</TableHead>
          <TableHead className="text-right">Total Days</TableHead>
          <TableHead className="text-right text-status-present">Avg Present</TableHead>
          <TableHead className="text-right text-status-absent">Avg Absent</TableHead>
          <TableHead className="text-right text-status-late">Avg Late</TableHead>
          <TableHead className="text-right">Attendance %</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {mockMonthlyReport.map((row, index) => (
          <TableRow key={index}>
            <TableCell className="font-medium">{row.class}</TableCell>
            <TableCell className="text-right">{row.totalDays}</TableCell>
            <TableCell className="text-right text-status-present font-semibold">{row.avgPresent}</TableCell>
            <TableCell className="text-right text-status-absent font-semibold">{row.avgAbsent}</TableCell>
            <TableCell className="text-right text-status-late font-semibold">{row.avgLate}</TableCell>
            <TableCell className="text-right font-bold">{row.attendance}%</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );

  const renderIndividualReport = () => (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Roll No</TableHead>
          <TableHead>Student Name</TableHead>
          <TableHead>Class</TableHead>
          <TableHead className="text-right text-status-present">Present</TableHead>
          <TableHead className="text-right text-status-absent">Absent</TableHead>
          <TableHead className="text-right text-status-late">Late</TableHead>
          <TableHead className="text-right">Attendance %</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {mockIndividualReport.map((row) => (
          <TableRow key={row.rollNo}>
            <TableCell className="font-medium">{row.rollNo}</TableCell>
            <TableCell>{row.name}</TableCell>
            <TableCell>{row.class}</TableCell>
            <TableCell className="text-right text-status-present font-semibold">{row.present}</TableCell>
            <TableCell className="text-right text-status-absent font-semibold">{row.absent}</TableCell>
            <TableCell className="text-right text-status-late font-semibold">{row.late}</TableCell>
            <TableCell className="text-right font-bold">{row.percentage}%</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Attendance Reports</h1>
          <p className="text-muted-foreground">Generate and view various attendance reports</p>
        </div>
        <Button onClick={handleExport} className="bg-gradient-primary">
          <Download className="mr-2 h-4 w-4" />
          Export Report
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Report Filters
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-5">
            <div className="space-y-2">
              <label className="text-sm font-medium">Report Type</label>
              <Select value={reportType} onValueChange={setReportType}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="daily">Daily Report</SelectItem>
                  <SelectItem value="monthly">Monthly Report</SelectItem>
                  <SelectItem value="classwise">Class-wise Report</SelectItem>
                  <SelectItem value="individual">Individual Report</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Date</label>
              <Popover>
                <PopoverTrigger asChild>
                  <Button variant="outline" className={cn("w-full justify-start text-left font-normal")}>
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {format(date, "PPP")}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar mode="single" selected={date} onSelect={(d) => d && setDate(d)} initialFocus />
                </PopoverContent>
              </Popover>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Class</label>
              <Select value={selectedClass} onValueChange={setSelectedClass}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Classes</SelectItem>
                  <SelectItem value="1">Class 1</SelectItem>
                  <SelectItem value="5">Class 5</SelectItem>
                  <SelectItem value="8">Class 8</SelectItem>
                  <SelectItem value="10">Class 10</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Section</label>
              <Select value={selectedSection} onValueChange={setSelectedSection}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Sections</SelectItem>
                  <SelectItem value="A">Section A</SelectItem>
                  <SelectItem value="B">Section B</SelectItem>
                  <SelectItem value="C">Section C</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Status Filter</label>
              <Select value={selectedStatus} onValueChange={setSelectedStatus}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="present">Present Only</SelectItem>
                  <SelectItem value="absent">Absent Only</SelectItem>
                  <SelectItem value="late">Late Only</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Report Display */}
      <Card>
        <CardHeader>
          <CardTitle>
            {reportType === "daily" && "Daily Attendance Report"}
            {reportType === "monthly" && "Monthly Attendance Report"}
            {reportType === "classwise" && "Class-wise Attendance Report"}
            {reportType === "individual" && "Individual Student Report"}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {reportType === "daily" && renderDailyReport()}
          {reportType === "monthly" && renderMonthlyReport()}
          {reportType === "classwise" && renderMonthlyReport()}
          {reportType === "individual" && renderIndividualReport()}
        </CardContent>
      </Card>
    </div>
  );
}
