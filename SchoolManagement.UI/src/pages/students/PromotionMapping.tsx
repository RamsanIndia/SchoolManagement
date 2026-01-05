import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
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
import {
  ArrowLeft,
  ArrowRight,
  Search,
  CheckCircle,
  XCircle,
  AlertTriangle,
} from "lucide-react";

interface Student {
  id: string;
  name: string;
  rollNo: string;
  oldClass: string;
  newClass: string;
  attendance: number;
  marks: number;
  status: "promoted" | "failed" | "review";
  selected: boolean;
}

const mockStudents: Student[] = [
  { id: "1", name: "Rohan Sharma", rollNo: "5A-01", oldClass: "5-A", newClass: "6-A", attendance: 92, marks: 78, status: "promoted", selected: true },
  { id: "2", name: "Aisha Verma", rollNo: "5A-02", oldClass: "5-A", newClass: "6-A", attendance: 88, marks: 85, status: "promoted", selected: true },
  { id: "3", name: "Kabir Khan", rollNo: "5A-03", oldClass: "5-A", newClass: "6-A", attendance: 95, marks: 72, status: "promoted", selected: true },
  { id: "4", name: "Priya Singh", rollNo: "5A-04", oldClass: "5-A", newClass: "5-A", attendance: 65, marks: 45, status: "review", selected: false },
  { id: "5", name: "Rahul Gupta", rollNo: "5A-05", oldClass: "5-A", newClass: "5-A", attendance: 80, marks: 28, status: "failed", selected: false },
  { id: "6", name: "Sneha Patel", rollNo: "5A-06", oldClass: "5-A", newClass: "6-A", attendance: 90, marks: 82, status: "promoted", selected: true },
  { id: "7", name: "Arjun Reddy", rollNo: "5A-07", oldClass: "5-A", newClass: "6-A", attendance: 87, marks: 75, status: "promoted", selected: true },
  { id: "8", name: "Meera Nair", rollNo: "5A-08", oldClass: "5-A", newClass: "6-A", attendance: 93, marks: 88, status: "promoted", selected: true },
];

export default function PromotionMapping() {
  const navigate = useNavigate();
  const [students, setStudents] = useState<Student[]>(mockStudents);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");
  const [selectAll, setSelectAll] = useState(false);

  const filteredStudents = students.filter((student) => {
    const matchesSearch = student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      student.rollNo.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === "all" || student.status === filterStatus;
    return matchesSearch && matchesStatus;
  });

  const handleSelectAll = (checked: boolean) => {
    setSelectAll(checked);
    setStudents((prev) =>
      prev.map((s) => ({ ...s, selected: s.status === "promoted" ? checked : s.selected }))
    );
  };

  const handleSelectStudent = (id: string, checked: boolean) => {
    setStudents((prev) =>
      prev.map((s) => (s.id === id ? { ...s, selected: checked } : s))
    );
  };

  const handleStatusChange = (id: string, status: Student["status"]) => {
    setStudents((prev) =>
      prev.map((s) => (s.id === id ? { ...s, status, newClass: status === "promoted" ? "6-A" : "5-A" } : s))
    );
  };

  const getStatusBadge = (status: Student["status"]) => {
    switch (status) {
      case "promoted":
        return <Badge className="bg-green-500/10 text-green-600"><CheckCircle className="h-3 w-3 mr-1" /> Promoted</Badge>;
      case "failed":
        return <Badge className="bg-red-500/10 text-red-600"><XCircle className="h-3 w-3 mr-1" /> Failed</Badge>;
      case "review":
        return <Badge className="bg-orange-500/10 text-orange-600"><AlertTriangle className="h-3 w-3 mr-1" /> Review</Badge>;
    }
  };

  const stats = {
    promoted: students.filter((s) => s.status === "promoted").length,
    failed: students.filter((s) => s.status === "failed").length,
    review: students.filter((s) => s.status === "review").length,
    selected: students.filter((s) => s.selected).length,
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Student Mapping</h1>
            <p className="text-muted-foreground">Class 5-A → Class 6-A (2023-24 → 2024-25)</p>
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card className="bg-green-500/10 border-green-500/20">
          <CardContent className="pt-4">
            <div className="text-center">
              <p className="text-2xl font-bold text-green-600">{stats.promoted}</p>
              <p className="text-sm text-green-600">Promoted</p>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-red-500/10 border-red-500/20">
          <CardContent className="pt-4">
            <div className="text-center">
              <p className="text-2xl font-bold text-red-600">{stats.failed}</p>
              <p className="text-sm text-red-600">Failed</p>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-orange-500/10 border-orange-500/20">
          <CardContent className="pt-4">
            <div className="text-center">
              <p className="text-2xl font-bold text-orange-600">{stats.review}</p>
              <p className="text-sm text-orange-600">Needs Review</p>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-blue-500/10 border-blue-500/20">
          <CardContent className="pt-4">
            <div className="text-center">
              <p className="text-2xl font-bold text-blue-600">{stats.selected}</p>
              <p className="text-sm text-blue-600">Selected</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex gap-4 mb-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by name or roll number..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={filterStatus} onValueChange={setFilterStatus}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Students</SelectItem>
                <SelectItem value="promoted">Promoted</SelectItem>
                <SelectItem value="failed">Failed</SelectItem>
                <SelectItem value="review">Needs Review</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Table */}
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">
                    <Checkbox
                      checked={selectAll}
                      onCheckedChange={handleSelectAll}
                    />
                  </TableHead>
                  <TableHead>Roll No</TableHead>
                  <TableHead>Student Name</TableHead>
                  <TableHead>Old Class</TableHead>
                  <TableHead>New Class</TableHead>
                  <TableHead>Attendance</TableHead>
                  <TableHead>Marks (%)</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Action</TableHead>
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
                    <TableCell>{student.oldClass}</TableCell>
                    <TableCell>
                      <Badge variant={student.newClass !== student.oldClass ? "default" : "secondary"}>
                        {student.newClass}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <span className={student.attendance < 75 ? "text-red-600" : ""}>
                        {student.attendance}%
                      </span>
                    </TableCell>
                    <TableCell>
                      <span className={student.marks < 35 ? "text-red-600" : ""}>
                        {student.marks}%
                      </span>
                    </TableCell>
                    <TableCell>{getStatusBadge(student.status)}</TableCell>
                    <TableCell>
                      <Select
                        value={student.status}
                        onValueChange={(v) => handleStatusChange(student.id, v as Student["status"])}
                      >
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="promoted">Promote</SelectItem>
                          <SelectItem value="failed">Retain</SelectItem>
                          <SelectItem value="review">Review</SelectItem>
                        </SelectContent>
                      </Select>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-between">
        <Button variant="outline" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Setup
        </Button>
        <div className="flex gap-4">
          <Button variant="outline">Save as Draft</Button>
          <Button onClick={() => navigate("/students/promotion/preview")}>
            Preview & Finalize
            <ArrowRight className="h-4 w-4 ml-2" />
          </Button>
        </div>
      </div>
    </div>
  );
}
