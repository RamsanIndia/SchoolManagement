import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Award, TrendingUp, Download, FileText, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";

interface SubjectResult {
  name: string;
  marks: number;
  maxMarks: number;
  grade: string;
}

interface StudentResult {
  studentName: string;
  class: string;
  section: string;
  rollNo: number;
  total: number;
  maxTotal: number;
  percentage: number;
  rank: number;
  grade: string;
  status: "Pass" | "Fail";
  subjects: SubjectResult[];
}

const mockResult: StudentResult = {
  studentName: "Amit Kumar",
  class: "10",
  section: "A",
  rollNo: 1,
  total: 430,
  maxTotal: 500,
  percentage: 86,
  rank: 4,
  grade: "A",
  status: "Pass",
  subjects: [
    { name: "Mathematics", marks: 87, maxMarks: 100, grade: "A" },
    { name: "Science", marks: 90, maxMarks: 100, grade: "A+" },
    { name: "English", marks: 82, maxMarks: 100, grade: "A" },
    { name: "Social Studies", marks: 85, maxMarks: 100, grade: "A" },
    { name: "Computer", marks: 86, maxMarks: 100, grade: "A" },
  ],
};

const mockClassResults: StudentResult[] = [
  { ...mockResult, studentName: "Amit Kumar", rollNo: 1, total: 430, percentage: 86, rank: 4, grade: "A", status: "Pass" },
  { ...mockResult, studentName: "Sana Sharma", rollNo: 2, total: 460, percentage: 92, rank: 1, grade: "A+", status: "Pass" },
  { ...mockResult, studentName: "Vikram Singh", rollNo: 3, total: 380, percentage: 76, rank: 8, grade: "B+", status: "Pass" },
  { ...mockResult, studentName: "Priya Patel", rollNo: 4, total: 475, percentage: 95, rank: 2, grade: "A+", status: "Pass" },
  { ...mockResult, studentName: "Rahul Verma", rollNo: 5, total: 340, percentage: 68, rank: 12, grade: "B", status: "Pass" },
];

export default function ResultSummary() {
  const [selectedExam, setSelectedExam] = useState("term1");
  const [selectedClass, setSelectedClass] = useState("10");
  const [selectedSection, setSelectedSection] = useState("A");
  const [selectedStudent, setSelectedStudent] = useState("1");
  const [viewMode, setViewMode] = useState<"individual" | "class">("individual");
  const [sortColumn, setSortColumn] = useState<string>("rank");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");

  const currentResult = mockResult;

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

  const sortedClassResults = [...mockClassResults].sort((a, b) => {
    if (!sortColumn) return 0;
    const aValue = a[sortColumn as keyof StudentResult];
    const bValue = b[sortColumn as keyof StudentResult];
    if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
    if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
    return 0;
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Result Summary</h1>
          <p className="text-muted-foreground">View detailed examination results</p>
        </div>
        <Button className="bg-gradient-primary">
          <Download className="mr-2 h-4 w-4" />
          Download Report Card
        </Button>
      </div>

      {/* Selection and View Mode */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Select Details
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            <div className="space-y-2">
              <Label>View Mode</Label>
              <Select value={viewMode} onValueChange={(value: "individual" | "class") => setViewMode(value)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="individual">Individual Result</SelectItem>
                  <SelectItem value="class">Class Results</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Exam</Label>
              <Select value={selectedExam} onValueChange={setSelectedExam}>
                <SelectTrigger>
                  <SelectValue />
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
                  <SelectValue />
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
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="A">Section A</SelectItem>
                  <SelectItem value="B">Section B</SelectItem>
                  <SelectItem value="C">Section C</SelectItem>
                </SelectContent>
              </Select>
            </div>
            {viewMode === "individual" && (
              <div className="space-y-2">
                <Label>Student</Label>
                <Select value={selectedStudent} onValueChange={setSelectedStudent}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {mockClassResults.map((student) => (
                      <SelectItem key={student.rollNo} value={student.rollNo.toString()}>
                        {student.studentName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {viewMode === "individual" ? (
        <>
          {/* Summary Cards */}
          <div className="grid gap-4 md:grid-cols-5">
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Total Marks</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {currentResult.total}/{currentResult.maxTotal}
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Percentage</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-blue-500">{currentResult.percentage}%</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Grade</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-status-late">{currentResult.grade}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Class Rank</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-2">
                  <Award className="h-5 w-5 text-status-present" />
                  <div className="text-2xl font-bold text-status-present">#{currentResult.rank}</div>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">Status</CardTitle>
              </CardHeader>
              <CardContent>
                <Badge className={currentResult.status === "Pass" ? "bg-status-present text-white" : "bg-destructive text-white"}>
                  {currentResult.status}
                </Badge>
              </CardContent>
            </Card>
          </div>

          {/* Student Info */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <TrendingUp className="h-5 w-5" />
                Student Information
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div>
                  <Label className="text-muted-foreground">Student Name</Label>
                  <p className="font-medium">{currentResult.studentName}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Class</Label>
                  <p className="font-medium">{currentResult.class} - {currentResult.section}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Roll No</Label>
                  <p className="font-medium">{currentResult.rollNo}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Exam</Label>
                  <p className="font-medium">Term 1 Examination</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Subject-wise Results */}
          <Card>
            <CardHeader>
              <CardTitle>Subject-wise Performance</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Subject</TableHead>
                      <TableHead>Marks Obtained</TableHead>
                      <TableHead>Maximum Marks</TableHead>
                      <TableHead>Percentage</TableHead>
                      <TableHead>Grade</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {currentResult.subjects.map((subject, index) => {
                      const percentage = ((subject.marks / subject.maxMarks) * 100).toFixed(1);
                      return (
                        <TableRow key={index}>
                          <TableCell className="font-medium">{subject.name}</TableCell>
                          <TableCell>{subject.marks}</TableCell>
                          <TableCell>{subject.maxMarks}</TableCell>
                          <TableCell>{percentage}%</TableCell>
                          <TableCell>
                            <Badge className={
                              subject.grade === "A+" || subject.grade === "A" 
                                ? "bg-status-present text-white"
                                : "bg-blue-500 text-white"
                            }>
                              {subject.grade}
                            </Badge>
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
      ) : (
        /* Class Results Table */
        <Card>
          <CardHeader>
            <CardTitle>Class {selectedClass}-{selectedSection} Results</CardTitle>
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
                    <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("studentName")}>
                      <div className="flex items-center">
                        Student Name
                        {getSortIcon("studentName")}
                      </div>
                    </TableHead>
                    <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("total")}>
                      <div className="flex items-center">
                        Total Marks
                        {getSortIcon("total")}
                      </div>
                    </TableHead>
                    <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("percentage")}>
                      <div className="flex items-center">
                        Percentage
                        {getSortIcon("percentage")}
                      </div>
                    </TableHead>
                    <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("grade")}>
                      <div className="flex items-center">
                        Grade
                        {getSortIcon("grade")}
                      </div>
                    </TableHead>
                    <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("rank")}>
                      <div className="flex items-center">
                        Rank
                        {getSortIcon("rank")}
                      </div>
                    </TableHead>
                    <TableHead>Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {sortedClassResults.map((student) => (
                    <TableRow key={student.rollNo}>
                      <TableCell className="font-medium">{student.rollNo}</TableCell>
                      <TableCell>{student.studentName}</TableCell>
                      <TableCell>{student.total}/{student.maxTotal}</TableCell>
                      <TableCell>{student.percentage}%</TableCell>
                      <TableCell>
                        <Badge className={
                          student.grade === "A+" || student.grade === "A" 
                            ? "bg-status-present text-white"
                            : student.grade === "B+" || student.grade === "B"
                            ? "bg-blue-500 text-white"
                            : "bg-status-late text-white"
                        }>
                          {student.grade}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          {student.rank <= 3 && <Award className="h-4 w-4 text-status-present" />}
                          #{student.rank}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge className={student.status === "Pass" ? "bg-status-present text-white" : "bg-destructive text-white"}>
                          {student.status}
                        </Badge>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
