/**
 * Teacher Workload Analysis Dashboard
 * Shows periods per week, classes assigned, and schedule conflicts
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { 
  ArrowLeft, Users, Clock, AlertTriangle, BookOpen, 
  TrendingUp, Calendar, ChevronDown, ChevronUp 
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

interface TeacherWorkloadData {
  id: string;
  name: string;
  department: string;
  avatar: string;
  totalPeriods: number;
  maxPeriods: number;
  classesAssigned: string[];
  subjects: string[];
  conflicts: Conflict[];
  weeklySchedule: DaySchedule[];
}

interface Conflict {
  id: string;
  type: "overlap" | "overload" | "gap";
  description: string;
  severity: "high" | "medium" | "low";
}

interface DaySchedule {
  day: string;
  periods: number;
}

const mockTeachers: TeacherWorkloadData[] = [
  {
    id: "1",
    name: "Priya Singh",
    department: "Languages",
    avatar: "PS",
    totalPeriods: 28,
    maxPeriods: 30,
    classesAssigned: ["Grade 1-A", "Grade 1-B", "Grade 2-A", "Grade 3-A"],
    subjects: ["English", "Hindi"],
    conflicts: [
      { id: "c1", type: "overlap", description: "Double booked: Monday 10:00 AM - Grade 1-A and Grade 2-A", severity: "high" }
    ],
    weeklySchedule: [
      { day: "Mon", periods: 6 },
      { day: "Tue", periods: 5 },
      { day: "Wed", periods: 6 },
      { day: "Thu", periods: 5 },
      { day: "Fri", periods: 4 },
      { day: "Sat", periods: 2 },
    ]
  },
  {
    id: "2",
    name: "Ravi Kumar",
    department: "Mathematics",
    avatar: "RK",
    totalPeriods: 32,
    maxPeriods: 30,
    classesAssigned: ["Grade 1-A", "Grade 1-B", "Grade 2-A", "Grade 2-B", "Grade 3-A"],
    subjects: ["Mathematics"],
    conflicts: [
      { id: "c2", type: "overload", description: "Exceeds maximum periods (32/30)", severity: "high" },
      { id: "c3", type: "gap", description: "3-hour gap between classes on Wednesday", severity: "low" }
    ],
    weeklySchedule: [
      { day: "Mon", periods: 6 },
      { day: "Tue", periods: 6 },
      { day: "Wed", periods: 5 },
      { day: "Thu", periods: 6 },
      { day: "Fri", periods: 5 },
      { day: "Sat", periods: 4 },
    ]
  },
  {
    id: "3",
    name: "Anil Mehta",
    department: "Science",
    avatar: "AM",
    totalPeriods: 24,
    maxPeriods: 30,
    classesAssigned: ["Grade 3-A", "Grade 3-B", "Grade 4-A", "Grade 5-A"],
    subjects: ["Science", "Physics"],
    conflicts: [],
    weeklySchedule: [
      { day: "Mon", periods: 4 },
      { day: "Tue", periods: 5 },
      { day: "Wed", periods: 4 },
      { day: "Thu", periods: 4 },
      { day: "Fri", periods: 5 },
      { day: "Sat", periods: 2 },
    ]
  },
  {
    id: "4",
    name: "Neha Singh",
    department: "Social Studies",
    avatar: "NS",
    totalPeriods: 22,
    maxPeriods: 30,
    classesAssigned: ["Grade 2-A", "Grade 2-B", "Grade 4-A"],
    subjects: ["Social Studies", "History"],
    conflicts: [
      { id: "c4", type: "gap", description: "Long break between morning and afternoon classes on Thursday", severity: "medium" }
    ],
    weeklySchedule: [
      { day: "Mon", periods: 4 },
      { day: "Tue", periods: 4 },
      { day: "Wed", periods: 4 },
      { day: "Thu", periods: 3 },
      { day: "Fri", periods: 4 },
      { day: "Sat", periods: 3 },
    ]
  },
  {
    id: "5",
    name: "Amit Patel",
    department: "Computer Science",
    avatar: "AP",
    totalPeriods: 18,
    maxPeriods: 30,
    classesAssigned: ["Grade 3-A", "Grade 4-A", "Grade 5-A"],
    subjects: ["Computer Science"],
    conflicts: [],
    weeklySchedule: [
      { day: "Mon", periods: 3 },
      { day: "Tue", periods: 3 },
      { day: "Wed", periods: 3 },
      { day: "Thu", periods: 3 },
      { day: "Fri", periods: 3 },
      { day: "Sat", periods: 3 },
    ]
  },
  {
    id: "6",
    name: "Kavita Joshi",
    department: "Arts",
    avatar: "KJ",
    totalPeriods: 20,
    maxPeriods: 30,
    classesAssigned: ["Grade 1-A", "Grade 1-B", "Grade 2-A", "Grade 2-B"],
    subjects: ["Art", "Craft"],
    conflicts: [],
    weeklySchedule: [
      { day: "Mon", periods: 4 },
      { day: "Tue", periods: 3 },
      { day: "Wed", periods: 4 },
      { day: "Thu", periods: 3 },
      { day: "Fri", periods: 4 },
      { day: "Sat", periods: 2 },
    ]
  },
];

export default function TeacherWorkload() {
  const navigate = useNavigate();
  const [selectedDepartment, setSelectedDepartment] = useState("all");
  const [expandedTeacher, setExpandedTeacher] = useState<string | null>(null);

  const departments = ["all", ...new Set(mockTeachers.map(t => t.department))];
  
  const filteredTeachers = selectedDepartment === "all" 
    ? mockTeachers 
    : mockTeachers.filter(t => t.department === selectedDepartment);

  const totalConflicts = mockTeachers.reduce((acc, t) => acc + t.conflicts.length, 0);
  const overloadedTeachers = mockTeachers.filter(t => t.totalPeriods > t.maxPeriods).length;
  const avgWorkload = Math.round(mockTeachers.reduce((acc, t) => acc + t.totalPeriods, 0) / mockTeachers.length);

  const getWorkloadColor = (current: number, max: number) => {
    const percentage = (current / max) * 100;
    if (percentage > 100) return "bg-destructive";
    if (percentage > 85) return "bg-yellow-500";
    return "bg-green-500";
  };

  const getWorkloadStatus = (current: number, max: number) => {
    const percentage = (current / max) * 100;
    if (percentage > 100) return { label: "Overloaded", color: "text-destructive" };
    if (percentage > 85) return { label: "High", color: "text-yellow-500" };
    if (percentage > 60) return { label: "Optimal", color: "text-green-500" };
    return { label: "Low", color: "text-blue-500" };
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate("/classes")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Teacher Workload Analysis</h1>
            <p className="text-muted-foreground mt-1">Monitor teaching load and schedule conflicts</p>
          </div>
        </div>
        <Select value={selectedDepartment} onValueChange={setSelectedDepartment}>
          <SelectTrigger className="w-[180px] bg-background">
            <SelectValue placeholder="Filter by department" />
          </SelectTrigger>
          <SelectContent className="bg-popover">
            {departments.map(dept => (
              <SelectItem key={dept} value={dept}>
                {dept === "all" ? "All Departments" : dept}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Teachers</p>
                <p className="text-2xl font-bold text-foreground">{mockTeachers.length}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-primary/10 flex items-center justify-center">
                <Users className="h-6 w-6 text-primary" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Avg. Periods/Week</p>
                <p className="text-2xl font-bold text-foreground">{avgWorkload}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-blue-500/10 flex items-center justify-center">
                <Clock className="h-6 w-6 text-blue-500" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Overloaded</p>
                <p className="text-2xl font-bold text-destructive">{overloadedTeachers}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-destructive/10 flex items-center justify-center">
                <TrendingUp className="h-6 w-6 text-destructive" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Schedule Conflicts</p>
                <p className="text-2xl font-bold text-yellow-500">{totalConflicts}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-yellow-500/10 flex items-center justify-center">
                <AlertTriangle className="h-6 w-6 text-yellow-500" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Teacher Workload Table */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle className="text-foreground flex items-center gap-2">
            <Calendar className="h-5 w-5" />
            Workload Overview
          </CardTitle>
          <CardDescription>Click on a teacher to view detailed schedule</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="border-border hover:bg-muted/50">
                <TableHead className="text-muted-foreground w-[250px]">Teacher</TableHead>
                <TableHead className="text-muted-foreground">Department</TableHead>
                <TableHead className="text-muted-foreground">Classes</TableHead>
                <TableHead className="text-muted-foreground">Subjects</TableHead>
                <TableHead className="text-muted-foreground w-[200px]">Workload</TableHead>
                <TableHead className="text-muted-foreground">Status</TableHead>
                <TableHead className="text-muted-foreground">Conflicts</TableHead>
                <TableHead className="w-[50px]"></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredTeachers.map((teacher) => {
                const status = getWorkloadStatus(teacher.totalPeriods, teacher.maxPeriods);
                const isExpanded = expandedTeacher === teacher.id;
                
                return (
                  <Collapsible key={teacher.id} open={isExpanded} onOpenChange={() => setExpandedTeacher(isExpanded ? null : teacher.id)}>
                    <TableRow className="border-border hover:bg-muted/50">
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center text-sm font-semibold text-primary">
                            {teacher.avatar}
                          </div>
                          <span className="font-medium text-foreground">{teacher.name}</span>
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="outline">{teacher.department}</Badge>
                      </TableCell>
                      <TableCell className="text-foreground">
                        {teacher.classesAssigned.length} classes
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {teacher.subjects.map(subject => (
                            <Badge key={subject} variant="secondary" className="text-xs">
                              {subject}
                            </Badge>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="space-y-1">
                          <div className="flex justify-between text-xs">
                            <span className="text-muted-foreground">{teacher.totalPeriods}/{teacher.maxPeriods} periods</span>
                            <span className={status.color}>{Math.round((teacher.totalPeriods / teacher.maxPeriods) * 100)}%</span>
                          </div>
                          <Progress 
                            value={Math.min((teacher.totalPeriods / teacher.maxPeriods) * 100, 100)} 
                            className={`h-2 ${getWorkloadColor(teacher.totalPeriods, teacher.maxPeriods)}`}
                          />
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge className={`${
                          status.label === "Overloaded" ? "bg-destructive/10 text-destructive border-destructive/30" :
                          status.label === "High" ? "bg-yellow-500/10 text-yellow-500 border-yellow-500/30" :
                          status.label === "Optimal" ? "bg-green-500/10 text-green-500 border-green-500/30" :
                          "bg-blue-500/10 text-blue-500 border-blue-500/30"
                        }`}>
                          {status.label}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {teacher.conflicts.length > 0 ? (
                          <Badge className="bg-destructive/10 text-destructive border-destructive/30">
                            {teacher.conflicts.length} issues
                          </Badge>
                        ) : (
                          <Badge className="bg-green-500/10 text-green-500 border-green-500/30">
                            None
                          </Badge>
                        )}
                      </TableCell>
                      <TableCell>
                        <CollapsibleTrigger asChild>
                          <Button variant="ghost" size="sm">
                            {isExpanded ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
                          </Button>
                        </CollapsibleTrigger>
                      </TableCell>
                    </TableRow>
                    <CollapsibleContent asChild>
                      <tr>
                        <td colSpan={8} className="p-0">
                          <div className="bg-muted/30 p-4 border-t border-border">
                            <div className="grid grid-cols-2 gap-6">
                              {/* Weekly Schedule */}
                              <div>
                                <h4 className="font-medium text-foreground mb-3 flex items-center gap-2">
                                  <Clock className="h-4 w-4" />
                                  Weekly Schedule
                                </h4>
                                <div className="flex gap-2">
                                  {teacher.weeklySchedule.map(day => (
                                    <div key={day.day} className="flex-1 text-center">
                                      <div className="text-xs text-muted-foreground mb-1">{day.day}</div>
                                      <div className={`h-16 rounded-lg flex items-center justify-center font-semibold ${
                                        day.periods >= 6 ? "bg-destructive/20 text-destructive" :
                                        day.periods >= 5 ? "bg-yellow-500/20 text-yellow-600" :
                                        "bg-green-500/20 text-green-600"
                                      }`}>
                                        {day.periods}
                                      </div>
                                    </div>
                                  ))}
                                </div>
                              </div>
                              
                              {/* Conflicts */}
                              <div>
                                <h4 className="font-medium text-foreground mb-3 flex items-center gap-2">
                                  <AlertTriangle className="h-4 w-4" />
                                  Conflicts & Issues
                                </h4>
                                {teacher.conflicts.length > 0 ? (
                                  <div className="space-y-2">
                                    {teacher.conflicts.map(conflict => (
                                      <div 
                                        key={conflict.id} 
                                        className={`p-3 rounded-lg border ${
                                          conflict.severity === "high" ? "bg-destructive/10 border-destructive/30" :
                                          conflict.severity === "medium" ? "bg-yellow-500/10 border-yellow-500/30" :
                                          "bg-blue-500/10 border-blue-500/30"
                                        }`}
                                      >
                                        <div className="flex items-start gap-2">
                                          <AlertTriangle className={`h-4 w-4 mt-0.5 ${
                                            conflict.severity === "high" ? "text-destructive" :
                                            conflict.severity === "medium" ? "text-yellow-500" :
                                            "text-blue-500"
                                          }`} />
                                          <div>
                                            <Badge variant="outline" className="text-xs mb-1">
                                              {conflict.type}
                                            </Badge>
                                            <p className="text-sm text-foreground">{conflict.description}</p>
                                          </div>
                                        </div>
                                      </div>
                                    ))}
                                  </div>
                                ) : (
                                  <div className="p-4 rounded-lg bg-green-500/10 border border-green-500/30 text-center">
                                    <p className="text-green-600 text-sm">No scheduling conflicts found</p>
                                  </div>
                                )}
                              </div>
                            </div>
                            
                            {/* Classes Assigned */}
                            <div className="mt-4">
                              <h4 className="font-medium text-foreground mb-2 flex items-center gap-2">
                                <BookOpen className="h-4 w-4" />
                                Classes Assigned
                              </h4>
                              <div className="flex flex-wrap gap-2">
                                {teacher.classesAssigned.map(cls => (
                                  <Badge key={cls} variant="secondary">{cls}</Badge>
                                ))}
                              </div>
                            </div>
                          </div>
                        </td>
                      </tr>
                    </CollapsibleContent>
                  </Collapsible>
                );
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
