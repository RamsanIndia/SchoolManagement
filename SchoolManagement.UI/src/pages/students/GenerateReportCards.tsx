import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Checkbox } from "@/components/ui/checkbox";
import { Search, FileText, Eye, Download, Printer, CheckCircle } from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface Student {
  id: string;
  name: string;
  rollNo: string;
  marks: {
    english: number;
    hindi: number;
    math: number;
    science: number;
    social: number;
  };
  total: number;
  percentage: number;
  grade: string;
  attendance: number;
  selected: boolean;
  generated: boolean;
}

const calculateGrade = (percentage: number): string => {
  if (percentage >= 90) return "A+";
  if (percentage >= 80) return "A";
  if (percentage >= 70) return "B+";
  if (percentage >= 60) return "B";
  if (percentage >= 50) return "C";
  if (percentage >= 40) return "D";
  return "F";
};

const mockStudents: Student[] = [
  { id: "1", name: "Rohan Sharma", rollNo: "5A-01", marks: { english: 85, hindi: 78, math: 92, science: 88, social: 82 }, total: 425, percentage: 85, grade: "A", attendance: 92, selected: false, generated: false },
  { id: "2", name: "Aisha Verma", rollNo: "5A-02", marks: { english: 90, hindi: 85, math: 88, science: 92, social: 87 }, total: 442, percentage: 88.4, grade: "A", attendance: 88, selected: false, generated: false },
  { id: "3", name: "Kabir Khan", rollNo: "5A-03", marks: { english: 75, hindi: 80, math: 85, science: 78, social: 82 }, total: 400, percentage: 80, grade: "A", attendance: 95, selected: false, generated: true },
  { id: "4", name: "Priya Singh", rollNo: "5A-04", marks: { english: 68, hindi: 72, math: 65, science: 70, social: 68 }, total: 343, percentage: 68.6, grade: "B+", attendance: 78, selected: false, generated: false },
  { id: "5", name: "Rahul Gupta", rollNo: "5A-05", marks: { english: 45, hindi: 52, math: 48, science: 55, social: 50 }, total: 250, percentage: 50, grade: "C", attendance: 80, selected: false, generated: false },
];

export default function GenerateReportCards() {
  const navigate = useNavigate();
  const [students, setStudents] = useState<Student[]>(mockStudents);
  const [selectedClass, setSelectedClass] = useState("5");
  const [selectedSection, setSelectedSection] = useState("A");
  const [selectedTerm, setSelectedTerm] = useState("term1");
  const [searchTerm, setSearchTerm] = useState("");
  const [isGenerating, setIsGenerating] = useState(false);
  const [generationProgress, setGenerationProgress] = useState(0);

  const filteredStudents = students.filter((student) =>
    student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    student.rollNo.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleSelectAll = (checked: boolean) => {
    setStudents((prev) => prev.map((s) => ({ ...s, selected: checked })));
  };

  const handleSelectStudent = (id: string, checked: boolean) => {
    setStudents((prev) =>
      prev.map((s) => (s.id === id ? { ...s, selected: checked } : s))
    );
  };

  const handleGenerateSelected = () => {
    const selectedStudents = students.filter((s) => s.selected);
    if (selectedStudents.length === 0) {
      toast({
        title: "No Students Selected",
        description: "Please select at least one student to generate report cards.",
        variant: "destructive",
      });
      return;
    }

    setIsGenerating(true);
    setGenerationProgress(0);

    const interval = setInterval(() => {
      setGenerationProgress((prev) => {
        if (prev >= 100) {
          clearInterval(interval);
          setIsGenerating(false);
          setStudents((prev) =>
            prev.map((s) => (s.selected ? { ...s, generated: true, selected: false } : s))
          );
          toast({
            title: "Report Cards Generated",
            description: `Successfully generated ${selectedStudents.length} report cards.`,
          });
          return 100;
        }
        return prev + 20;
      });
    }, 500);
  };

  const getGradeBadge = (grade: string) => {
    const colors: Record<string, string> = {
      "A+": "bg-green-500/10 text-green-600",
      "A": "bg-green-500/10 text-green-600",
      "B+": "bg-blue-500/10 text-blue-600",
      "B": "bg-blue-500/10 text-blue-600",
      "C": "bg-orange-500/10 text-orange-600",
      "D": "bg-orange-500/10 text-orange-600",
      "F": "bg-red-500/10 text-red-600",
    };
    return <Badge className={colors[grade] || ""}>{grade}</Badge>;
  };

  const selectedCount = students.filter((s) => s.selected).length;
  const generatedCount = students.filter((s) => s.generated).length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Generate Report Cards</h1>
          <p className="text-muted-foreground">Generate and manage student report cards</p>
        </div>
        <Button
          onClick={handleGenerateSelected}
          disabled={selectedCount === 0 || isGenerating}
        >
          <FileText className="h-4 w-4 mr-2" />
          Generate Selected ({selectedCount})
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Class</label>
              <Select value={selectedClass} onValueChange={setSelectedClass}>
                <SelectTrigger>
                  <SelectValue placeholder="Select class" />
                </SelectTrigger>
                <SelectContent>
                  {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map((c) => (
                    <SelectItem key={c} value={c.toString()}>Class {c}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Section</label>
              <Select value={selectedSection} onValueChange={setSelectedSection}>
                <SelectTrigger>
                  <SelectValue placeholder="Select section" />
                </SelectTrigger>
                <SelectContent>
                  {["A", "B", "C", "D"].map((s) => (
                    <SelectItem key={s} value={s}>Section {s}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Term</label>
              <Select value={selectedTerm} onValueChange={setSelectedTerm}>
                <SelectTrigger>
                  <SelectValue placeholder="Select term" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="term1">Term 1</SelectItem>
                  <SelectItem value="term2">Term 2</SelectItem>
                  <SelectItem value="annual">Annual</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Search</label>
              <div className="relative">
                <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search students..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Progress Bar */}
      {isGenerating && (
        <Card>
          <CardContent className="pt-6">
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Generating report cards...</span>
                <span>{generationProgress}%</span>
              </div>
              <Progress value={generationProgress} className="h-2" />
            </div>
          </CardContent>
        </Card>
      )}

      {/* Stats */}
      <div className="grid grid-cols-3 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold">{students.length}</p>
            <p className="text-sm text-muted-foreground">Total Students</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-green-600">{generatedCount}</p>
            <p className="text-sm text-muted-foreground">Generated</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-orange-600">{students.length - generatedCount}</p>
            <p className="text-sm text-muted-foreground">Pending</p>
          </CardContent>
        </Card>
      </div>

      {/* Students Table */}
      <Card>
        <CardHeader>
          <CardTitle>Students - Class {selectedClass}-{selectedSection}</CardTitle>
          <CardDescription>Select students to generate report cards</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="border rounded-lg overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      onCheckedChange={handleSelectAll}
                    />
                  </TableHead>
                  <TableHead>Roll No</TableHead>
                  <TableHead>Student Name</TableHead>
                  <TableHead className="text-center">English</TableHead>
                  <TableHead className="text-center">Hindi</TableHead>
                  <TableHead className="text-center">Math</TableHead>
                  <TableHead className="text-center">Science</TableHead>
                  <TableHead className="text-center">Social</TableHead>
                  <TableHead className="text-center">Total</TableHead>
                  <TableHead className="text-center">%</TableHead>
                  <TableHead className="text-center">Grade</TableHead>
                  <TableHead className="text-center">Attendance</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredStudents.map((student) => (
                  <TableRow key={student.id}>
                    <TableCell>
                      <Checkbox
                        checked={student.selected}
                        onCheckedChange={(checked) =>
                          handleSelectStudent(student.id, checked as boolean)
                        }
                      />
                    </TableCell>
                    <TableCell className="font-mono">{student.rollNo}</TableCell>
                    <TableCell className="font-medium">{student.name}</TableCell>
                    <TableCell className="text-center">{student.marks.english}</TableCell>
                    <TableCell className="text-center">{student.marks.hindi}</TableCell>
                    <TableCell className="text-center">{student.marks.math}</TableCell>
                    <TableCell className="text-center">{student.marks.science}</TableCell>
                    <TableCell className="text-center">{student.marks.social}</TableCell>
                    <TableCell className="text-center font-medium">{student.total}</TableCell>
                    <TableCell className="text-center font-medium">{student.percentage}%</TableCell>
                    <TableCell className="text-center">{getGradeBadge(student.grade)}</TableCell>
                    <TableCell className="text-center">{student.attendance}%</TableCell>
                    <TableCell>
                      {student.generated ? (
                        <Badge className="bg-green-500/10 text-green-600">
                          <CheckCircle className="h-3 w-3 mr-1" />
                          Generated
                        </Badge>
                      ) : (
                        <Badge variant="secondary">Pending</Badge>
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="flex gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => navigate(`/students/report-cards/view/${student.id}`)}
                          disabled={!student.generated}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          disabled={!student.generated}
                        >
                          <Download className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          disabled={!student.generated}
                        >
                          <Printer className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
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
