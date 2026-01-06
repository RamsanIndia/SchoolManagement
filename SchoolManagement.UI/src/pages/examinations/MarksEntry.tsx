import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { useToast } from "@/hooks/use-toast";
import { Save, ClipboardList, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";

interface StudentMark {
  rollNo: number;
  name: string;
  marks: number;
  grade: string;
}

const mockStudents: StudentMark[] = [
  { rollNo: 1, name: "Amit Kumar", marks: 87, grade: "A" },
  { rollNo: 2, name: "Sana Sharma", marks: 92, grade: "A+" },
  { rollNo: 3, name: "Vikram Singh", marks: 76, grade: "B+" },
  { rollNo: 4, name: "Priya Patel", marks: 95, grade: "A+" },
  { rollNo: 5, name: "Rahul Verma", marks: 68, grade: "B" },
  { rollNo: 6, name: "Anjali Gupta", marks: 82, grade: "A" },
  { rollNo: 7, name: "Karan Mehta", marks: 91, grade: "A+" },
  { rollNo: 8, name: "Neha Reddy", marks: 73, grade: "B+" },
];

const calculateGrade = (marks: number, maxMarks: number = 100): string => {
  const percentage = (marks / maxMarks) * 100;
  if (percentage >= 90) return "A+";
  if (percentage >= 80) return "A";
  if (percentage >= 70) return "B+";
  if (percentage >= 60) return "B";
  if (percentage >= 50) return "C";
  if (percentage >= 40) return "D";
  return "F";
};

export default function MarksEntry() {
  const [students, setStudents] = useState<StudentMark[]>(mockStudents);
  const [selectedExam, setSelectedExam] = useState("");
  const [selectedClass, setSelectedClass] = useState("");
  const [selectedSection, setSelectedSection] = useState("");
  const [selectedSubject, setSelectedSubject] = useState("");
  const [maxMarks, setMaxMarks] = useState(100);
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const [showTable, setShowTable] = useState(false);
  const { toast } = useToast();

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

  const sortedStudents = [...students].sort((a, b) => {
    if (!sortColumn) return 0;
    const aValue = a[sortColumn as keyof StudentMark];
    const bValue = b[sortColumn as keyof StudentMark];
    if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
    if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
    return 0;
  });

  const handleMarksChange = (rollNo: number, marks: number) => {
    const validMarks = Math.min(Math.max(0, marks), maxMarks);
    setStudents(students.map(student => 
      student.rollNo === rollNo 
        ? { ...student, marks: validMarks, grade: calculateGrade(validMarks, maxMarks) }
        : student
    ));
  };

  const handleLoadStudents = () => {
    if (!selectedExam || !selectedClass || !selectedSection || !selectedSubject) {
      toast({
        title: "Missing Information",
        description: "Please select all fields to load students",
        variant: "destructive",
      });
      return;
    }
    setShowTable(true);
    toast({
      title: "Students Loaded",
      description: `Loaded ${students.length} students for marks entry`,
    });
  };

  const handleSaveMarks = () => {
    toast({
      title: "Success",
      description: "Marks saved successfully for all students",
    });
  };

  const stats = {
    total: students.length,
    entered: students.filter(s => s.marks > 0).length,
    average: Math.round(students.reduce((sum, s) => sum + s.marks, 0) / students.length),
    passed: students.filter(s => s.marks >= (maxMarks * 0.4)).length,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Marks Entry</h1>
          <p className="text-muted-foreground">Enter examination marks for students</p>
        </div>
      </div>

      {/* Selection Form */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ClipboardList className="h-5 w-5" />
            Select Exam Details
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            <div className="space-y-2">
              <Label>Exam</Label>
              <Select value={selectedExam} onValueChange={setSelectedExam}>
                <SelectTrigger>
                  <SelectValue placeholder="Select exam" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="term1">Term 1 Examination</SelectItem>
                  <SelectItem value="unit2">Unit Test 2</SelectItem>
                  <SelectItem value="final">Final Examination</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Class</Label>
              <Select value={selectedClass} onValueChange={setSelectedClass}>
                <SelectTrigger>
                  <SelectValue placeholder="Select class" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="9">Class 9</SelectItem>
                  <SelectItem value="10">Class 10</SelectItem>
                  <SelectItem value="11">Class 11</SelectItem>
                  <SelectItem value="12">Class 12</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Section</Label>
              <Select value={selectedSection} onValueChange={setSelectedSection}>
                <SelectTrigger>
                  <SelectValue placeholder="Select section" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="A">Section A</SelectItem>
                  <SelectItem value="B">Section B</SelectItem>
                  <SelectItem value="C">Section C</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Subject</Label>
              <Select value={selectedSubject} onValueChange={setSelectedSubject}>
                <SelectTrigger>
                  <SelectValue placeholder="Select subject" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="mathematics">Mathematics</SelectItem>
                  <SelectItem value="science">Science</SelectItem>
                  <SelectItem value="english">English</SelectItem>
                  <SelectItem value="social">Social Studies</SelectItem>
                  <SelectItem value="computer">Computer</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Max Marks</Label>
              <Input
                type="number"
                value={maxMarks}
                onChange={(e) => setMaxMarks(parseInt(e.target.value) || 100)}
                min="1"
              />
            </div>
          </div>
          <Button onClick={handleLoadStudents} className="bg-gradient-primary">
            Load Students
          </Button>
        </CardContent>
      </Card>

      {showTable && (
        <>
          {/* Stats Cards */}
          <div className="grid gap-4 md:grid-cols-4">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Total Students</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stats.total}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Marks Entered</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-blue-500">{stats.entered}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Class Average</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-status-late">{stats.average}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Passed</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-status-present">{stats.passed}</div>
              </CardContent>
            </Card>
          </div>

          {/* Marks Entry Table */}
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Student Marks</CardTitle>
                <Button onClick={handleSaveMarks} className="bg-gradient-primary">
                  <Save className="mr-2 h-4 w-4" />
                  Save All Marks
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("rollNo")}>
                        <div className="flex items-center">
                          Roll No
                          {getSortIcon("rollNo")}
                        </div>
                      </TableHead>
                      <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("name")}>
                        <div className="flex items-center">
                          Student Name
                          {getSortIcon("name")}
                        </div>
                      </TableHead>
                      <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("marks")}>
                        <div className="flex items-center">
                          Marks Obtained (/{maxMarks})
                          {getSortIcon("marks")}
                        </div>
                      </TableHead>
                      <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("grade")}>
                        <div className="flex items-center">
                          Grade
                          {getSortIcon("grade")}
                        </div>
                      </TableHead>
                      <TableHead>Percentage</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {sortedStudents.map((student) => {
                      const percentage = ((student.marks / maxMarks) * 100).toFixed(1);
                      const isPassed = student.marks >= (maxMarks * 0.4);
                      return (
                        <TableRow key={student.rollNo}>
                          <TableCell className="font-medium">{student.rollNo}</TableCell>
                          <TableCell>{student.name}</TableCell>
                          <TableCell>
                            <Input
                              type="number"
                              value={student.marks}
                              onChange={(e) => handleMarksChange(student.rollNo, parseInt(e.target.value) || 0)}
                              className="w-24"
                              min="0"
                              max={maxMarks}
                            />
                          </TableCell>
                          <TableCell>
                            <Badge className={
                              student.grade === "A+" || student.grade === "A" 
                                ? "bg-status-present text-white"
                                : student.grade === "B+" || student.grade === "B"
                                ? "bg-blue-500 text-white"
                                : student.grade === "C"
                                ? "bg-status-late text-white"
                                : "bg-destructive text-white"
                            }>
                              {student.grade}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <span className={isPassed ? "text-status-present" : "text-destructive"}>
                              {percentage}%
                            </span>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              </div>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}
