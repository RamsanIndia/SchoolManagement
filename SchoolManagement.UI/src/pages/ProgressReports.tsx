import { useState } from "react";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Progress } from "@/components/ui/progress";
import { Search, FileText, TrendingUp, TrendingDown, Minus, Download } from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface StudentProgress {
  id: string;
  name: string;
  rollNumber: string;
  class: string;
  overallGrade: string;
  overallPercentage: number;
  attendance: number;
  subjects: {
    name: string;
    grade: string;
    percentage: number;
    trend: "up" | "down" | "stable";
  }[];
  behavior: string;
  remarks: string;
}

const initialProgressData: StudentProgress[] = [
  {
    id: "1",
    name: "Alice Johnson",
    rollNumber: "2024001",
    class: "10A",
    overallGrade: "A",
    overallPercentage: 92,
    attendance: 94,
    subjects: [
      { name: "Mathematics", grade: "A", percentage: 95, trend: "up" },
      { name: "Physics", grade: "A", percentage: 90, trend: "stable" },
      { name: "Chemistry", grade: "B+", percentage: 88, trend: "up" },
      { name: "English", grade: "A", percentage: 94, trend: "stable" },
    ],
    behavior: "Excellent",
    remarks: "Outstanding performance. Shows great improvement in all subjects.",
  },
  {
    id: "2",
    name: "Bob Smith",
    rollNumber: "2024002",
    class: "10B",
    overallGrade: "B+",
    overallPercentage: 85,
    attendance: 87,
    subjects: [
      { name: "Mathematics", grade: "B", percentage: 82, trend: "down" },
      { name: "Physics", grade: "A", percentage: 90, trend: "up" },
      { name: "Chemistry", grade: "B+", percentage: 85, trend: "stable" },
      { name: "English", grade: "B+", percentage: 83, trend: "up" },
    ],
    behavior: "Good",
    remarks: "Good progress overall. Needs to focus more on Mathematics.",
  },
  {
    id: "3",
    name: "Carol Davis",
    rollNumber: "2024003",
    class: "9A",
    overallGrade: "A",
    overallPercentage: 88,
    attendance: 91,
    subjects: [
      { name: "Mathematics", grade: "B+", percentage: 86, trend: "stable" },
      { name: "Physics", grade: "A", percentage: 92, trend: "up" },
      { name: "Chemistry", grade: "B+", percentage: 85, trend: "stable" },
      { name: "English", grade: "A", percentage: 90, trend: "up" },
    ],
    behavior: "Very Good",
    remarks: "Consistent performance with steady improvement.",
  },
];

export default function ProgressReports() {
  const { user } = useAuth();
  const [progressData, setProgressData] = useState<StudentProgress[]>(initialProgressData);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterClass, setFilterClass] = useState("all");
  const [selectedStudent, setSelectedStudent] = useState<StudentProgress | null>(null);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);

  if (!user || !["admin", "teacher"].includes(user.role)) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-muted-foreground">
          Access denied. This page is only available to administrators and teachers.
        </p>
      </div>
    );
  }

  const filteredData = progressData.filter((student) => {
    const matchesSearch =
      student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      student.rollNumber.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesClass = filterClass === "all" || student.class === filterClass;
    return matchesSearch && matchesClass;
  });

  const handleViewDetails = (student: StudentProgress) => {
    setSelectedStudent(student);
    setIsDetailsOpen(true);
  };

  const handleDownloadReport = (student: StudentProgress) => {
    toast({
      title: "Downloading Report",
      description: `Generating progress report for ${student.name}...`,
    });
  };

  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case "up":
        return <TrendingUp className="h-4 w-4 text-green-600" />;
      case "down":
        return <TrendingDown className="h-4 w-4 text-red-600" />;
      default:
        return <Minus className="h-4 w-4 text-muted-foreground" />;
    }
  };

  const getGradeColor = (grade: string) => {
    if (grade.startsWith("A")) return "text-green-600";
    if (grade.startsWith("B")) return "text-blue-600";
    if (grade.startsWith("C")) return "text-yellow-600";
    return "text-red-600";
  };

  const stats = {
    totalStudents: progressData.length,
    averagePercentage: Math.round(
      progressData.reduce((acc, s) => acc + s.overallPercentage, 0) / progressData.length
    ),
    excellentPerformers: progressData.filter((s) => s.overallGrade.startsWith("A")).length,
    needsAttention: progressData.filter((s) => s.overallPercentage < 75).length,
  };

  return (
    <div className="p-8 space-y-8">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Student Progress Reports</h1>
          <p className="text-muted-foreground">
            Track and monitor student academic progress
          </p>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Students</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalStudents}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Average Performance</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.averagePercentage}%</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Excellent Performers</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.excellentPerformers}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Needs Attention</CardTitle>
            <TrendingDown className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.needsAttention}</div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search by name or roll number..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <Select value={filterClass} onValueChange={setFilterClass}>
              <SelectTrigger className="w-full md:w-[180px]">
                <SelectValue placeholder="Filter by class" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Classes</SelectItem>
                <SelectItem value="9A">Class 9A</SelectItem>
                <SelectItem value="10A">Class 10A</SelectItem>
                <SelectItem value="10B">Class 10B</SelectItem>
                <SelectItem value="11A">Class 11A</SelectItem>
                <SelectItem value="12A">Class 12A</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Student</TableHead>
                <TableHead>Roll Number</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Overall Grade</TableHead>
                <TableHead>Performance</TableHead>
                <TableHead>Attendance</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredData.map((student) => (
                <TableRow key={student.id}>
                  <TableCell className="font-medium">{student.name}</TableCell>
                  <TableCell>{student.rollNumber}</TableCell>
                  <TableCell>{student.class}</TableCell>
                  <TableCell>
                    <Badge className={getGradeColor(student.overallGrade)}>
                      {student.overallGrade}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="space-y-1">
                      <div className="flex items-center justify-between text-sm">
                        <span>{student.overallPercentage}%</span>
                      </div>
                      <Progress value={student.overallPercentage} className="h-2" />
                    </div>
                  </TableCell>
                  <TableCell>{student.attendance}%</TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleViewDetails(student)}
                      >
                        <FileText className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDownloadReport(student)}
                      >
                        <Download className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={isDetailsOpen} onOpenChange={setIsDetailsOpen}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Progress Report - {selectedStudent?.name}</DialogTitle>
            <DialogDescription>
              Detailed academic progress and performance analysis
            </DialogDescription>
          </DialogHeader>
          {selectedStudent && (
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-muted-foreground">Roll Number</p>
                  <p className="font-medium">{selectedStudent.rollNumber}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Class</p>
                  <p className="font-medium">{selectedStudent.class}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Overall Grade</p>
                  <p className={`text-xl font-bold ${getGradeColor(selectedStudent.overallGrade)}`}>
                    {selectedStudent.overallGrade}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Attendance</p>
                  <p className="font-medium">{selectedStudent.attendance}%</p>
                </div>
              </div>

              <div>
                <h3 className="font-semibold mb-3">Subject-wise Performance</h3>
                <div className="space-y-3">
                  {selectedStudent.subjects.map((subject, index) => (
                    <div key={index} className="flex items-center gap-4">
                      <div className="flex-1">
                        <div className="flex items-center justify-between mb-1">
                          <span className="text-sm font-medium">{subject.name}</span>
                          <div className="flex items-center gap-2">
                            {getTrendIcon(subject.trend)}
                            <Badge className={getGradeColor(subject.grade)}>
                              {subject.grade}
                            </Badge>
                            <span className="text-sm font-medium">{subject.percentage}%</span>
                          </div>
                        </div>
                        <Progress value={subject.percentage} className="h-2" />
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <div>
                <h3 className="font-semibold mb-2">Behavior</h3>
                <Badge variant="outline">{selectedStudent.behavior}</Badge>
              </div>

              <div>
                <h3 className="font-semibold mb-2">Teacher's Remarks</h3>
                <p className="text-sm text-muted-foreground">{selectedStudent.remarks}</p>
              </div>

              <div className="flex justify-end gap-2 pt-4">
                <Button variant="outline" onClick={() => setIsDetailsOpen(false)}>
                  Close
                </Button>
                <Button onClick={() => handleDownloadReport(selectedStudent)}>
                  <Download className="mr-2 h-4 w-4" /> Download Report
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
