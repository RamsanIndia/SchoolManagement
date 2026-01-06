/**
 * Student List by Class & Section
 * View all students in a specific class and section
 */

import { useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { 
  ArrowLeft, Search, Eye, Phone, User, Download, 
  Filter, GraduationCap, Users
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
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
import { Label } from "@/components/ui/label";
import { toast } from "@/hooks/use-toast";

interface Student {
  id: string;
  rollNo: number;
  name: string;
  gender: "Male" | "Female";
  parentName: string;
  mobile: string;
  email: string;
  admissionDate: string;
  avatar?: string;
}

const generateMockStudents = (): Student[] => {
  const firstNames = ["Aarav", "Vivaan", "Aditya", "Vihaan", "Arjun", "Sai", "Reyansh", "Ayaan", "Krishna", "Ishaan",
    "Ananya", "Aadhya", "Diya", "Saanvi", "Aanya", "Pari", "Aarohi", "Navya", "Myra", "Sara",
    "Kabir", "Rudra", "Aryan", "Shaurya", "Dhruv", "Arnav", "Atharva", "Advait", "Kiaan", "Viraj"];
  const lastNames = ["Sharma", "Patel", "Gupta", "Singh", "Kumar", "Verma", "Joshi", "Reddy", "Mehta", "Rao"];
  
  return Array.from({ length: 32 }, (_, i) => ({
    id: String(i + 1),
    rollNo: i + 1,
    name: `${firstNames[i % firstNames.length]} ${lastNames[i % lastNames.length]}`,
    gender: i % 3 === 0 ? "Female" : "Male",
    parentName: `${["Mr.", "Mrs."][Math.floor(Math.random() * 2)]} ${lastNames[(i + 1) % lastNames.length]}`,
    mobile: `+91 ${9000000000 + Math.floor(Math.random() * 999999999)}`,
    email: `parent${i + 1}@email.com`,
    admissionDate: `2024-0${(i % 9) + 1}-${10 + (i % 20)}`,
  }));
};

const mockStudents = generateMockStudents();

const classNames: Record<string, string> = {
  "1": "Grade 1",
  "2": "Grade 2",
  "3": "Grade 3",
  "4": "Grade 4",
  "5": "Grade 5",
};

const sections = ["A", "B", "C"];

export default function ClassStudentList() {
  const { classId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const className = classNames[classId || "1"] || "Grade 1";
  const initialSection = searchParams.get("section") || "A";
  
  const [students] = useState<Student[]>(mockStudents);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedSection, setSelectedSection] = useState(initialSection);
  const [genderFilter, setGenderFilter] = useState("all");
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [isViewDialogOpen, setIsViewDialogOpen] = useState(false);

  const filteredStudents = students.filter(student => {
    const matchesSearch = student.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      student.parentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      String(student.rollNo).includes(searchTerm);
    const matchesGender = genderFilter === "all" || student.gender === genderFilter;
    return matchesSearch && matchesGender;
  });

  const stats = {
    totalStudents: filteredStudents.length,
    maleStudents: filteredStudents.filter(s => s.gender === "Male").length,
    femaleStudents: filteredStudents.filter(s => s.gender === "Female").length,
  };

  const handleViewProfile = (student: Student) => {
    setSelectedStudent(student);
    setIsViewDialogOpen(true);
  };

  const handleExport = () => {
    toast({ title: "Export", description: "Exporting student list to CSV..." });
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
            <h1 className="text-3xl font-bold text-foreground">Students</h1>
            <p className="text-muted-foreground mt-1">{className} (Section {selectedSection})</p>
          </div>
        </div>
        <Button variant="outline" onClick={handleExport}>
          <Download className="mr-2 h-4 w-4" />
          Export List
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Students</p>
                <p className="text-2xl font-bold text-foreground">{stats.totalStudents}</p>
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
                <p className="text-sm text-muted-foreground">Male Students</p>
                <p className="text-2xl font-bold text-foreground">{stats.maleStudents}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-blue-500/10 flex items-center justify-center">
                <User className="h-6 w-6 text-blue-500" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Female Students</p>
                <p className="text-2xl font-bold text-foreground">{stats.femaleStudents}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-pink-500/10 flex items-center justify-center">
                <User className="h-6 w-6 text-pink-500" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card className="bg-card border-border">
        <CardContent className="p-4">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
              <Input
                placeholder="Search by name, roll no, or parent name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 bg-background"
              />
            </div>
            <Select value={selectedSection} onValueChange={setSelectedSection}>
              <SelectTrigger className="w-full md:w-[150px] bg-background">
                <SelectValue />
              </SelectTrigger>
              <SelectContent className="bg-popover">
                {sections.map(section => (
                  <SelectItem key={section} value={section}>Section {section}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={genderFilter} onValueChange={setGenderFilter}>
              <SelectTrigger className="w-full md:w-[150px] bg-background">
                <SelectValue placeholder="Gender" />
              </SelectTrigger>
              <SelectContent className="bg-popover">
                <SelectItem value="all">All Genders</SelectItem>
                <SelectItem value="Male">Male</SelectItem>
                <SelectItem value="Female">Female</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Students Table */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle className="text-foreground flex items-center gap-2">
            <GraduationCap className="h-5 w-5" />
            Student List
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="border-border hover:bg-muted/50">
                <TableHead className="text-muted-foreground w-16">Photo</TableHead>
                <TableHead className="text-muted-foreground">Student Name</TableHead>
                <TableHead className="text-muted-foreground">Roll No</TableHead>
                <TableHead className="text-muted-foreground">Gender</TableHead>
                <TableHead className="text-muted-foreground">Parent Name</TableHead>
                <TableHead className="text-muted-foreground">Mobile</TableHead>
                <TableHead className="text-right text-muted-foreground">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredStudents.map((student) => (
                <TableRow key={student.id} className="border-border hover:bg-muted/50">
                  <TableCell>
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={student.avatar} />
                      <AvatarFallback className="bg-primary/10 text-primary">
                        {student.name.split(' ').map(n => n[0]).join('')}
                      </AvatarFallback>
                    </Avatar>
                  </TableCell>
                  <TableCell className="font-medium text-foreground">{student.name}</TableCell>
                  <TableCell>
                    <Badge variant="outline">{student.rollNo}</Badge>
                  </TableCell>
                  <TableCell>
                    <Badge className={student.gender === "Male" 
                      ? "bg-blue-500/20 text-blue-600 border-blue-500/30" 
                      : "bg-pink-500/20 text-pink-600 border-pink-500/30"
                    }>
                      {student.gender}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-foreground">{student.parentName}</TableCell>
                  <TableCell className="text-foreground">
                    <div className="flex items-center gap-2">
                      <Phone className="h-4 w-4 text-muted-foreground" />
                      {student.mobile}
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button variant="ghost" size="sm" onClick={() => handleViewProfile(student)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* View Profile Dialog */}
      <Dialog open={isViewDialogOpen} onOpenChange={setIsViewDialogOpen}>
        <DialogContent className="max-w-md bg-card border-border">
          <DialogHeader>
            <DialogTitle className="text-foreground">Student Profile</DialogTitle>
          </DialogHeader>
          {selectedStudent && (
            <div className="space-y-6">
              {/* Profile Header */}
              <div className="flex items-center gap-4">
                <Avatar className="h-20 w-20">
                  <AvatarImage src={selectedStudent.avatar} />
                  <AvatarFallback className="bg-primary/10 text-primary text-xl">
                    {selectedStudent.name.split(' ').map(n => n[0]).join('')}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <h3 className="text-lg font-semibold text-foreground">{selectedStudent.name}</h3>
                  <p className="text-sm text-muted-foreground">{className} - Section {selectedSection}</p>
                  <Badge className="mt-1" variant="outline">Roll No: {selectedStudent.rollNo}</Badge>
                </div>
              </div>

              {/* Details */}
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label className="text-muted-foreground">Gender</Label>
                    <p className="text-foreground mt-1">{selectedStudent.gender}</p>
                  </div>
                  <div>
                    <Label className="text-muted-foreground">Admission Date</Label>
                    <p className="text-foreground mt-1">{selectedStudent.admissionDate}</p>
                  </div>
                </div>
                
                <div className="pt-4 border-t border-border">
                  <h4 className="font-medium text-foreground mb-3">Parent/Guardian Details</h4>
                  <div className="space-y-3">
                    <div>
                      <Label className="text-muted-foreground">Parent Name</Label>
                      <p className="text-foreground mt-1">{selectedStudent.parentName}</p>
                    </div>
                    <div>
                      <Label className="text-muted-foreground">Mobile</Label>
                      <p className="text-foreground mt-1 flex items-center gap-2">
                        <Phone className="h-4 w-4" />
                        {selectedStudent.mobile}
                      </p>
                    </div>
                    <div>
                      <Label className="text-muted-foreground">Email</Label>
                      <p className="text-foreground mt-1">{selectedStudent.email}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
