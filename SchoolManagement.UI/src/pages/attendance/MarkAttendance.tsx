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
import { Check, X, Clock, Calendar as CalendarIcon, Users } from "lucide-react";
import { format } from "date-fns";
import { cn } from "@/lib/utils";
import { useToast } from "@/hooks/use-toast";

interface Student {
  rollNo: number;
  name: string;
  status: "present" | "absent" | "late";
}

const mockStudents: Student[] = [
  { rollNo: 1, name: "Amit Kumar", status: "present" },
  { rollNo: 2, name: "Sana Sharma", status: "late" },
  { rollNo: 3, name: "Vikram Singh", status: "absent" },
  { rollNo: 4, name: "Kavya Jain", status: "present" },
  { rollNo: 5, name: "Rahul Verma", status: "present" },
  { rollNo: 6, name: "Priya Patel", status: "late" },
  { rollNo: 7, name: "Arjun Reddy", status: "present" },
  { rollNo: 8, name: "Neha Gupta", status: "present" },
  { rollNo: 9, name: "Rohan Sharma", status: "absent" },
  { rollNo: 10, name: "Ananya Singh", status: "present" },
];

export default function MarkAttendance() {
  const [selectedClass, setSelectedClass] = useState("10");
  const [selectedSection, setSelectedSection] = useState("A");
  const [selectedPeriod, setSelectedPeriod] = useState("1");
  const [date, setDate] = useState<Date>(new Date());
  const [students, setStudents] = useState<Student[]>(mockStudents);
  const { toast } = useToast();

  const updateStatus = (rollNo: number, status: "present" | "absent" | "late") => {
    setStudents(prev =>
      prev.map(student =>
        student.rollNo === rollNo ? { ...student, status } : student
      )
    );
  };

  const markAllStatus = (status: "present" | "absent" | "late") => {
    setStudents(prev => prev.map(student => ({ ...student, status })));
    toast({
      title: "Bulk Update",
      description: `All students marked as ${status}`,
    });
  };

  const getStatusBadge = (status: string) => {
    const styles = {
      present: "bg-status-present text-white",
      absent: "bg-status-absent text-white",
      late: "bg-status-late text-white",
    };
    return <Badge className={styles[status as keyof typeof styles]}>{status.toUpperCase()}</Badge>;
  };

  const stats = {
    present: students.filter(s => s.status === "present").length,
    absent: students.filter(s => s.status === "absent").length,
    late: students.filter(s => s.status === "late").length,
  };

  const handleSave = () => {
    toast({
      title: "Attendance Saved",
      description: `Attendance for Class ${selectedClass}-${selectedSection} has been saved successfully.`,
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Mark Attendance</h1>
          <p className="text-muted-foreground">Mark attendance for students by class and section</p>
        </div>
        <Button onClick={handleSave} className="bg-gradient-primary">
          Save Attendance
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Users className="h-4 w-4" />
              Total Students
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{students.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Present</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-present">{stats.present}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Absent</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-absent">{stats.absent}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Late</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-late">{stats.late}</div>
          </CardContent>
        </Card>
      </div>

      {/* Controls */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid gap-4 md:grid-cols-5">
            <div className="space-y-2">
              <label className="text-sm font-medium">Class</label>
              <Select value={selectedClass} onValueChange={setSelectedClass}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="1">Class 1</SelectItem>
                  <SelectItem value="5">Class 5</SelectItem>
                  <SelectItem value="8">Class 8</SelectItem>
                  <SelectItem value="10">Class 10</SelectItem>
                  <SelectItem value="12">Class 12</SelectItem>
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
                  <SelectItem value="A">Section A</SelectItem>
                  <SelectItem value="B">Section B</SelectItem>
                  <SelectItem value="C">Section C</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Period</label>
              <Select value={selectedPeriod} onValueChange={setSelectedPeriod}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="1">Period 1</SelectItem>
                  <SelectItem value="2">Period 2</SelectItem>
                  <SelectItem value="3">Period 3</SelectItem>
                  <SelectItem value="4">Period 4</SelectItem>
                  <SelectItem value="5">Period 5</SelectItem>
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
              <label className="text-sm font-medium">Bulk Actions</label>
              <div className="flex gap-1">
                <Button size="sm" onClick={() => markAllStatus("present")} className="bg-status-present hover:bg-status-present/90">
                  All Present
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Student List */}
      <Card>
        <CardHeader>
          <CardTitle>Student List - Class {selectedClass}-{selectedSection}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {students.map((student) => (
              <div
                key={student.rollNo}
                className="flex items-center justify-between p-4 border rounded-lg hover:bg-muted/30 transition-colors"
              >
                <div className="flex items-center gap-4">
                  <div className="w-12 h-12 rounded-full bg-gradient-primary flex items-center justify-center text-white font-semibold">
                    {student.rollNo}
                  </div>
                  <div>
                    <p className="font-medium">{student.name}</p>
                    <p className="text-sm text-muted-foreground">Roll No: {student.rollNo}</p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  {getStatusBadge(student.status)}
                  <div className="flex gap-1">
                    <Button
                      size="sm"
                      variant={student.status === "present" ? "default" : "outline"}
                      onClick={() => updateStatus(student.rollNo, "present")}
                      className={cn(
                        "h-9 w-9 p-0",
                        student.status === "present" && "bg-status-present hover:bg-status-present/90"
                      )}
                    >
                      <Check className="h-4 w-4" />
                    </Button>
                    <Button
                      size="sm"
                      variant={student.status === "late" ? "default" : "outline"}
                      onClick={() => updateStatus(student.rollNo, "late")}
                      className={cn(
                        "h-9 w-9 p-0",
                        student.status === "late" && "bg-status-late hover:bg-status-late/90"
                      )}
                    >
                      <Clock className="h-4 w-4" />
                    </Button>
                    <Button
                      size="sm"
                      variant={student.status === "absent" ? "default" : "outline"}
                      onClick={() => updateStatus(student.rollNo, "absent")}
                      className={cn(
                        "h-9 w-9 p-0",
                        student.status === "absent" && "bg-status-absent hover:bg-status-absent/90"
                      )}
                    >
                      <X className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
