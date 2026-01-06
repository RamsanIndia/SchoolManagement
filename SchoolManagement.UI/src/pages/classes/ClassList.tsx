/**
 * Class List Page
 * Main page for viewing and managing all classes
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { 
  Plus, Search, Edit, Trash2, Eye, Users, Layers, 
  GraduationCap, Calendar, MoreHorizontal, BookOpen 
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { toast } from "@/hooks/use-toast";

interface ClassData {
  id: string;
  name: string;
  totalSections: number;
  totalStudents: number;
  classTeacher: string;
  academicYear: string;
  status: "active" | "inactive";
  sections: string[];
  subjects: string[];
}

const mockClasses: ClassData[] = [
  { id: "1", name: "Grade 1", totalSections: 2, totalStudents: 64, classTeacher: "Anita Sharma", academicYear: "2024–25", status: "active", sections: ["A", "B"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies", "Computer"] },
  { id: "2", name: "Grade 2", totalSections: 3, totalStudents: 79, classTeacher: "Ravi Kumar", academicYear: "2024–25", status: "active", sections: ["A", "B", "C"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies"] },
  { id: "3", name: "Grade 3", totalSections: 2, totalStudents: 58, classTeacher: "Sneha Patel", academicYear: "2024–25", status: "inactive", sections: ["A", "B"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies", "Computer"] },
  { id: "4", name: "Grade 4", totalSections: 3, totalStudents: 85, classTeacher: "Mohan Verma", academicYear: "2024–25", status: "active", sections: ["A", "B", "C"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies"] },
  { id: "5", name: "Grade 5", totalSections: 2, totalStudents: 72, classTeacher: "Priya Singh", academicYear: "2024–25", status: "active", sections: ["A", "B"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies", "Computer"] },
  { id: "6", name: "Grade 6", totalSections: 3, totalStudents: 90, classTeacher: "Anil Mehta", academicYear: "2024–25", status: "active", sections: ["A", "B", "C"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies", "Computer"] },
  { id: "7", name: "Grade 7", totalSections: 2, totalStudents: 68, classTeacher: "Kavita Joshi", academicYear: "2024–25", status: "active", sections: ["A", "B"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies"] },
  { id: "8", name: "Grade 8", totalSections: 3, totalStudents: 95, classTeacher: "Suresh Gupta", academicYear: "2024–25", status: "active", sections: ["A", "B", "C"], subjects: ["Math", "English", "Hindi", "Science", "Social Studies", "Computer"] },
];

const mockTeachers = [
  "Anita Sharma", "Ravi Kumar", "Sneha Patel", "Mohan Verma", 
  "Priya Singh", "Anil Mehta", "Kavita Joshi", "Suresh Gupta",
  "Rohan Verma", "Neha Singh", "Amit Patel", "Sunita Rao"
];

const mockSubjects = ["Math", "Science", "English", "Hindi", "Social Studies", "Computer", "Physical Education", "Art", "Music"];

export default function ClassList() {
  const navigate = useNavigate();
  const [classes, setClasses] = useState<ClassData[]>(mockClasses);
  const [searchTerm, setSearchTerm] = useState("");
  const [yearFilter, setYearFilter] = useState("all");
  const [teacherFilter, setTeacherFilter] = useState("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedClass, setSelectedClass] = useState<ClassData | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    academicYear: "2024–25",
    status: true,
    classTeacher: "",
    subjects: [] as string[],
  });

  const filteredClasses = classes.filter(cls => {
    const matchesSearch = cls.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      cls.classTeacher.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesYear = yearFilter === "all" || cls.academicYear === yearFilter;
    const matchesTeacher = teacherFilter === "all" || cls.classTeacher === teacherFilter;
    return matchesSearch && matchesYear && matchesTeacher;
  });

  const stats = {
    totalClasses: classes.length,
    activeClasses: classes.filter(c => c.status === "active").length,
    totalStudents: classes.reduce((sum, c) => sum + c.totalStudents, 0),
    totalSections: classes.reduce((sum, c) => sum + c.totalSections, 0),
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedClass) {
      setClasses(classes.map(c => 
        c.id === selectedClass.id 
          ? { ...c, name: formData.name, academicYear: formData.academicYear, status: formData.status ? "active" : "inactive", classTeacher: formData.classTeacher, subjects: formData.subjects }
          : c
      ));
      toast({ title: "Success", description: "Class updated successfully" });
    } else {
      const newClass: ClassData = {
        id: String(classes.length + 1),
        name: formData.name,
        totalSections: 1,
        totalStudents: 0,
        classTeacher: formData.classTeacher,
        academicYear: formData.academicYear,
        status: formData.status ? "active" : "inactive",
        sections: ["A"],
        subjects: formData.subjects,
      };
      setClasses([...classes, newClass]);
      toast({ title: "Success", description: "Class created successfully" });
    }
    resetForm();
  };

  const handleEdit = (cls: ClassData) => {
    setSelectedClass(cls);
    setFormData({
      name: cls.name,
      academicYear: cls.academicYear,
      status: cls.status === "active",
      classTeacher: cls.classTeacher,
      subjects: cls.subjects,
    });
    setIsDialogOpen(true);
  };

  const handleDelete = (id: string) => {
    if (confirm("Are you sure you want to delete this class?")) {
      setClasses(classes.filter(c => c.id !== id));
      toast({ title: "Success", description: "Class deleted successfully" });
    }
  };

  const resetForm = () => {
    setFormData({ name: "", academicYear: "2024–25", status: true, classTeacher: "", subjects: [] });
    setSelectedClass(null);
    setIsDialogOpen(false);
  };

  const toggleSubject = (subject: string) => {
    setFormData(prev => ({
      ...prev,
      subjects: prev.subjects.includes(subject)
        ? prev.subjects.filter(s => s !== subject)
        : [...prev.subjects, subject]
    }));
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Classes</h1>
          <p className="text-muted-foreground mt-1">Manage school classes, sections, and assignments</p>
        </div>
        <Button onClick={() => setIsDialogOpen(true)} className="bg-primary hover:bg-primary/90">
          <Plus className="mr-2 h-4 w-4" />
          Add New Class
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Classes</p>
                <p className="text-2xl font-bold text-foreground">{stats.totalClasses}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-primary/10 flex items-center justify-center">
                <BookOpen className="h-6 w-6 text-primary" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Active Classes</p>
                <p className="text-2xl font-bold text-foreground">{stats.activeClasses}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-green-500/10 flex items-center justify-center">
                <GraduationCap className="h-6 w-6 text-green-500" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Students</p>
                <p className="text-2xl font-bold text-foreground">{stats.totalStudents}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-blue-500/10 flex items-center justify-center">
                <Users className="h-6 w-6 text-blue-500" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Sections</p>
                <p className="text-2xl font-bold text-foreground">{stats.totalSections}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-orange-500/10 flex items-center justify-center">
                <Layers className="h-6 w-6 text-orange-500" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Search and Filters */}
      <Card className="bg-card border-border">
        <CardContent className="p-4">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
              <Input
                placeholder="Search by class name or teacher..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 bg-background"
              />
            </div>
            <Select value={yearFilter} onValueChange={setYearFilter}>
              <SelectTrigger className="w-full md:w-[180px] bg-background">
                <SelectValue placeholder="Academic Year" />
              </SelectTrigger>
              <SelectContent className="bg-popover">
                <SelectItem value="all">All Years</SelectItem>
                <SelectItem value="2024–25">2024–25</SelectItem>
                <SelectItem value="2023–24">2023–24</SelectItem>
              </SelectContent>
            </Select>
            <Select value={teacherFilter} onValueChange={setTeacherFilter}>
              <SelectTrigger className="w-full md:w-[200px] bg-background">
                <SelectValue placeholder="Class Teacher" />
              </SelectTrigger>
              <SelectContent className="bg-popover">
                <SelectItem value="all">All Teachers</SelectItem>
                {mockTeachers.map(teacher => (
                  <SelectItem key={teacher} value={teacher}>{teacher}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Classes Table */}
      <Card className="bg-card border-border">
        <Table>
          <TableHeader>
            <TableRow className="border-border hover:bg-muted/50">
              <TableHead className="text-muted-foreground">Class Name</TableHead>
              <TableHead className="text-muted-foreground">Sections</TableHead>
              <TableHead className="text-muted-foreground">Total Students</TableHead>
              <TableHead className="text-muted-foreground">Class Teacher</TableHead>
              <TableHead className="text-muted-foreground">Academic Year</TableHead>
              <TableHead className="text-muted-foreground">Status</TableHead>
              <TableHead className="text-right text-muted-foreground">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredClasses.map((cls) => (
              <TableRow key={cls.id} className="border-border hover:bg-muted/50">
                <TableCell className="font-medium text-foreground">{cls.name}</TableCell>
                <TableCell className="text-foreground">{cls.totalSections}</TableCell>
                <TableCell className="text-foreground">{cls.totalStudents}</TableCell>
                <TableCell className="text-foreground">{cls.classTeacher}</TableCell>
                <TableCell className="text-foreground">{cls.academicYear}</TableCell>
                <TableCell>
                  <Badge variant={cls.status === "active" ? "default" : "secondary"} 
                    className={cls.status === "active" ? "bg-green-500/20 text-green-600 border-green-500/30" : ""}>
                    {cls.status === "active" ? "Active" : "Inactive"}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="sm">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" className="bg-popover border-border">
                      <DropdownMenuItem onClick={() => navigate(`/classes/${cls.id}/sections`)}>
                        <Layers className="mr-2 h-4 w-4" /> Manage Sections
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate(`/classes/${cls.id}/subjects`)}>
                        <BookOpen className="mr-2 h-4 w-4" /> Subject Mapping
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate(`/classes/${cls.id}/timetable`)}>
                        <Calendar className="mr-2 h-4 w-4" /> View Timetable
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate(`/classes/${cls.id}/students`)}>
                        <Users className="mr-2 h-4 w-4" /> View Students
                      </DropdownMenuItem>
                      <DropdownMenuSeparator className="bg-border" />
                      <DropdownMenuItem onClick={() => handleEdit(cls)}>
                        <Edit className="mr-2 h-4 w-4" /> Edit
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => handleDelete(cls.id)} className="text-destructive">
                        <Trash2 className="mr-2 h-4 w-4" /> Delete
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      {/* Add/Edit Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-w-lg bg-card border-border">
          <DialogHeader>
            <DialogTitle className="text-foreground">{selectedClass ? "Edit Class" : "Add New Class"}</DialogTitle>
            <DialogDescription>
              {selectedClass ? "Update class information" : "Create a new class with basic details"}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              {/* Basic Details */}
              <div className="space-y-4">
                <h4 className="font-medium text-sm text-muted-foreground">Basic Details</h4>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Class Name *</Label>
                    <Input
                      id="name"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      placeholder="e.g., Grade 1"
                      className="bg-background"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="year">Academic Year</Label>
                    <Select value={formData.academicYear} onValueChange={(v) => setFormData({ ...formData, academicYear: v })}>
                      <SelectTrigger className="bg-background">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent className="bg-popover">
                        <SelectItem value="2024–25">2024–25</SelectItem>
                        <SelectItem value="2023–24">2023–24</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <Label htmlFor="status">Status (Active)</Label>
                  <Switch
                    id="status"
                    checked={formData.status}
                    onCheckedChange={(v) => setFormData({ ...formData, status: v })}
                  />
                </div>
              </div>

              {/* Class Teacher Assignment */}
              <div className="space-y-4 pt-4 border-t border-border">
                <h4 className="font-medium text-sm text-muted-foreground">Class Teacher Assignment</h4>
                <div className="space-y-2">
                  <Label htmlFor="teacher">Select Teacher</Label>
                  <Select value={formData.classTeacher} onValueChange={(v) => setFormData({ ...formData, classTeacher: v })}>
                    <SelectTrigger className="bg-background">
                      <SelectValue placeholder="Select a teacher" />
                    </SelectTrigger>
                    <SelectContent className="bg-popover">
                      {mockTeachers.map(teacher => (
                        <SelectItem key={teacher} value={teacher}>{teacher}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* Subject Assignment */}
              <div className="space-y-4 pt-4 border-t border-border">
                <h4 className="font-medium text-sm text-muted-foreground">Subject Assignment</h4>
                <div className="flex flex-wrap gap-2">
                  {mockSubjects.map(subject => (
                    <Badge
                      key={subject}
                      variant={formData.subjects.includes(subject) ? "default" : "outline"}
                      className={`cursor-pointer transition-colors ${formData.subjects.includes(subject) ? "bg-primary" : "hover:bg-muted"}`}
                      onClick={() => toggleSubject(subject)}
                    >
                      {subject}
                    </Badge>
                  ))}
                </div>
                <p className="text-xs text-muted-foreground">
                  {formData.subjects.length} subject(s) selected
                </p>
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={resetForm}>Cancel</Button>
              <Button type="submit" className="bg-primary hover:bg-primary/90">
                {selectedClass ? "Update Class" : "Create Class"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
