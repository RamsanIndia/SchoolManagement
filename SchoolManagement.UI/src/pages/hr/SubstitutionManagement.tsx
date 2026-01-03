import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
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
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Search,
  Plus,
  UserX,
  UserCheck,
  Calendar,
  Clock,
  AlertTriangle,
  CheckCircle,
  RefreshCw,
} from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface AbsenceRecord {
  id: string;
  teacherName: string;
  teacherId: string;
  date: string;
  periods: string[];
  reason: string;
  status: "pending" | "covered" | "uncovered";
  substitute?: string;
  substituteId?: string;
  classes: string[];
  subjects: string[];
}

interface AvailableTeacher {
  id: string;
  name: string;
  department: string;
  subjects: string[];
  freePeriods: string[];
  currentLoad: number;
  maxLoad: number;
}

const mockAbsences: AbsenceRecord[] = [
  {
    id: "1",
    teacherName: "Rajesh Kumar",
    teacherId: "T001",
    date: "2024-01-15",
    periods: ["1", "2", "3"],
    reason: "Medical Leave",
    status: "covered",
    substitute: "Priya Sharma",
    substituteId: "T005",
    classes: ["Class 10-A", "Class 10-B", "Class 9-A"],
    subjects: ["Mathematics"],
  },
  {
    id: "2",
    teacherName: "Anita Verma",
    teacherId: "T002",
    date: "2024-01-15",
    periods: ["4", "5"],
    reason: "Personal Emergency",
    status: "pending",
    classes: ["Class 8-A", "Class 8-B"],
    subjects: ["English"],
  },
  {
    id: "3",
    teacherName: "Suresh Patel",
    teacherId: "T003",
    date: "2024-01-16",
    periods: ["1", "2", "6", "7"],
    reason: "Conference Attendance",
    status: "uncovered",
    classes: ["Class 11-A", "Class 11-B", "Class 12-A", "Class 12-B"],
    subjects: ["Physics"],
  },
  {
    id: "4",
    teacherName: "Meera Singh",
    teacherId: "T004",
    date: "2024-01-16",
    periods: ["3", "4"],
    reason: "Family Function",
    status: "covered",
    substitute: "Amit Joshi",
    substituteId: "T006",
    classes: ["Class 7-A", "Class 7-B"],
    subjects: ["Hindi"],
  },
];

const mockAvailableTeachers: AvailableTeacher[] = [
  {
    id: "T005",
    name: "Priya Sharma",
    department: "Mathematics",
    subjects: ["Mathematics", "Statistics"],
    freePeriods: ["1", "2", "3", "7"],
    currentLoad: 24,
    maxLoad: 30,
  },
  {
    id: "T006",
    name: "Amit Joshi",
    department: "Languages",
    subjects: ["Hindi", "Sanskrit"],
    freePeriods: ["3", "4", "5", "8"],
    currentLoad: 22,
    maxLoad: 30,
  },
  {
    id: "T007",
    name: "Kavita Reddy",
    department: "Science",
    subjects: ["Physics", "Chemistry"],
    freePeriods: ["1", "2", "6", "7"],
    currentLoad: 26,
    maxLoad: 30,
  },
  {
    id: "T008",
    name: "Vikram Singh",
    department: "Languages",
    subjects: ["English", "French"],
    freePeriods: ["4", "5", "6"],
    currentLoad: 20,
    maxLoad: 30,
  },
];

export default function SubstitutionManagement() {
  const [absences, setAbsences] = useState<AbsenceRecord[]>(mockAbsences);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [dateFilter, setDateFilter] = useState<string>("");
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [selectedAbsence, setSelectedAbsence] = useState<AbsenceRecord | null>(null);
  const [isAssignDialogOpen, setIsAssignDialogOpen] = useState(false);

  const filteredAbsences = absences.filter((absence) => {
    const matchesSearch =
      absence.teacherName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      absence.classes.some((c) => c.toLowerCase().includes(searchTerm.toLowerCase()));
    const matchesStatus = statusFilter === "all" || absence.status === statusFilter;
    const matchesDate = !dateFilter || absence.date === dateFilter;
    return matchesSearch && matchesStatus && matchesDate;
  });

  const stats = {
    totalAbsences: absences.length,
    covered: absences.filter((a) => a.status === "covered").length,
    pending: absences.filter((a) => a.status === "pending").length,
    uncovered: absences.filter((a) => a.status === "uncovered").length,
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "covered":
        return <Badge className="bg-green-500/20 text-green-400">Covered</Badge>;
      case "pending":
        return <Badge className="bg-yellow-500/20 text-yellow-400">Pending</Badge>;
      case "uncovered":
        return <Badge className="bg-red-500/20 text-red-400">Uncovered</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const findAvailableSubstitutes = (absence: AbsenceRecord) => {
    return mockAvailableTeachers.filter((teacher) => {
      const hasMatchingSubject = teacher.subjects.some((s) =>
        absence.subjects.includes(s)
      );
      const hasFreePeriods = absence.periods.some((p) =>
        teacher.freePeriods.includes(p)
      );
      const hasCapacity = teacher.currentLoad < teacher.maxLoad;
      return hasMatchingSubject && hasFreePeriods && hasCapacity;
    });
  };

  const handleAssignSubstitute = (teacherId: string) => {
    if (!selectedAbsence) return;

    const teacher = mockAvailableTeachers.find((t) => t.id === teacherId);
    if (!teacher) return;

    setAbsences((prev) =>
      prev.map((a) =>
        a.id === selectedAbsence.id
          ? { ...a, status: "covered" as const, substitute: teacher.name, substituteId: teacher.id }
          : a
      )
    );

    toast({
      title: "Substitute Assigned",
      description: `${teacher.name} has been assigned as substitute for ${selectedAbsence.teacherName}`,
    });

    setIsAssignDialogOpen(false);
    setSelectedAbsence(null);
  };

  const handleAutoAssign = () => {
    let assignedCount = 0;
    const updatedAbsences = absences.map((absence) => {
      if (absence.status !== "pending" && absence.status !== "uncovered") {
        return absence;
      }

      const availableTeachers = findAvailableSubstitutes(absence);
      if (availableTeachers.length > 0) {
        const bestMatch = availableTeachers.sort(
          (a, b) => a.currentLoad - b.currentLoad
        )[0];
        assignedCount++;
        return {
          ...absence,
          status: "covered" as const,
          substitute: bestMatch.name,
          substituteId: bestMatch.id,
        };
      }
      return absence;
    });

    setAbsences(updatedAbsences);
    toast({
      title: "Auto-Assignment Complete",
      description: `Successfully assigned ${assignedCount} substitutes`,
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Substitution Management</h1>
          <p className="text-muted-foreground">
            Handle teacher absences and find available replacements
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleAutoAssign}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Auto-Assign
          </Button>
          <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Report Absence
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-md">
              <DialogHeader>
                <DialogTitle>Report Teacher Absence</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div>
                  <Label>Teacher</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select teacher" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="T001">Rajesh Kumar</SelectItem>
                      <SelectItem value="T002">Anita Verma</SelectItem>
                      <SelectItem value="T003">Suresh Patel</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <Label>Date</Label>
                  <Input type="date" />
                </div>
                <div>
                  <Label>Periods</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select periods" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1-2">Period 1-2</SelectItem>
                      <SelectItem value="3-4">Period 3-4</SelectItem>
                      <SelectItem value="5-6">Period 5-6</SelectItem>
                      <SelectItem value="all">All Day</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <Label>Reason</Label>
                  <Textarea placeholder="Enter reason for absence" />
                </div>
                <div className="flex justify-end gap-2">
                  <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                    Cancel
                  </Button>
                  <Button onClick={() => setIsAddDialogOpen(false)}>
                    Submit
                  </Button>
                </div>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Absences</p>
                <p className="text-2xl font-bold">{stats.totalAbsences}</p>
              </div>
              <UserX className="h-8 w-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Covered</p>
                <p className="text-2xl font-bold text-green-400">{stats.covered}</p>
              </div>
              <CheckCircle className="h-8 w-8 text-green-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Pending</p>
                <p className="text-2xl font-bold text-yellow-400">{stats.pending}</p>
              </div>
              <Clock className="h-8 w-8 text-yellow-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Uncovered</p>
                <p className="text-2xl font-bold text-red-400">{stats.uncovered}</p>
              </div>
              <AlertTriangle className="h-8 w-8 text-red-400" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="bg-card border-border">
        <CardContent className="pt-6">
          <div className="flex flex-wrap gap-4">
            <div className="relative flex-1 min-w-[200px]">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by teacher or class..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="covered">Covered</SelectItem>
                <SelectItem value="pending">Pending</SelectItem>
                <SelectItem value="uncovered">Uncovered</SelectItem>
              </SelectContent>
            </Select>
            <Input
              type="date"
              value={dateFilter}
              onChange={(e) => setDateFilter(e.target.value)}
              className="w-[180px]"
            />
          </div>
        </CardContent>
      </Card>

      {/* Absence Records Table */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle>Absence Records</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Teacher</TableHead>
                <TableHead>Date</TableHead>
                <TableHead>Periods</TableHead>
                <TableHead>Classes</TableHead>
                <TableHead>Reason</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Substitute</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredAbsences.map((absence) => (
                <TableRow key={absence.id}>
                  <TableCell className="font-medium">{absence.teacherName}</TableCell>
                  <TableCell>{absence.date}</TableCell>
                  <TableCell>{absence.periods.join(", ")}</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {absence.classes.slice(0, 2).map((c) => (
                        <Badge key={c} variant="outline" className="text-xs">
                          {c}
                        </Badge>
                      ))}
                      {absence.classes.length > 2 && (
                        <Badge variant="outline" className="text-xs">
                          +{absence.classes.length - 2}
                        </Badge>
                      )}
                    </div>
                  </TableCell>
                  <TableCell>{absence.reason}</TableCell>
                  <TableCell>{getStatusBadge(absence.status)}</TableCell>
                  <TableCell>
                    {absence.substitute || (
                      <span className="text-muted-foreground">Not assigned</span>
                    )}
                  </TableCell>
                  <TableCell>
                    {absence.status !== "covered" && (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => {
                          setSelectedAbsence(absence);
                          setIsAssignDialogOpen(true);
                        }}
                      >
                        <UserCheck className="h-4 w-4 mr-1" />
                        Assign
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Assign Substitute Dialog */}
      <Dialog open={isAssignDialogOpen} onOpenChange={setIsAssignDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Assign Substitute Teacher</DialogTitle>
          </DialogHeader>
          {selectedAbsence && (
            <div className="space-y-4">
              <div className="p-4 bg-muted/50 rounded-lg">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">Absent Teacher:</span>
                    <span className="ml-2 font-medium">{selectedAbsence.teacherName}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Date:</span>
                    <span className="ml-2 font-medium">{selectedAbsence.date}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Periods:</span>
                    <span className="ml-2 font-medium">{selectedAbsence.periods.join(", ")}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Subject:</span>
                    <span className="ml-2 font-medium">{selectedAbsence.subjects.join(", ")}</span>
                  </div>
                </div>
              </div>

              <div>
                <h4 className="font-medium mb-3">Available Substitutes</h4>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Teacher</TableHead>
                      <TableHead>Department</TableHead>
                      <TableHead>Subjects</TableHead>
                      <TableHead>Free Periods</TableHead>
                      <TableHead>Workload</TableHead>
                      <TableHead>Action</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {findAvailableSubstitutes(selectedAbsence).map((teacher) => (
                      <TableRow key={teacher.id}>
                        <TableCell className="font-medium">{teacher.name}</TableCell>
                        <TableCell>{teacher.department}</TableCell>
                        <TableCell>{teacher.subjects.join(", ")}</TableCell>
                        <TableCell>{teacher.freePeriods.join(", ")}</TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <div className="w-16 bg-muted rounded-full h-2">
                              <div
                                className="bg-primary h-2 rounded-full"
                                style={{
                                  width: `${(teacher.currentLoad / teacher.maxLoad) * 100}%`,
                                }}
                              />
                            </div>
                            <span className="text-xs">
                              {teacher.currentLoad}/{teacher.maxLoad}
                            </span>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Button
                            size="sm"
                            onClick={() => handleAssignSubstitute(teacher.id)}
                          >
                            Assign
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                    {findAvailableSubstitutes(selectedAbsence).length === 0 && (
                      <TableRow>
                        <TableCell colSpan={6} className="text-center text-muted-foreground py-8">
                          No available substitutes found for this absence
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}