/**
 * Subject Mapping Page
 * Map subjects to teachers for a specific class
 */

import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, BookOpen, Edit, Save, X, Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { toast } from "@/hooks/use-toast";

interface SubjectMapping {
  id: string;
  subjectName: string;
  subjectCode: string;
  teacher: string;
  periodsPerWeek: number;
}

const mockSubjectMappings: SubjectMapping[] = [
  { id: "1", subjectName: "English", subjectCode: "ENG101", teacher: "Priya Singh", periodsPerWeek: 6 },
  { id: "2", subjectName: "Mathematics", subjectCode: "MTH101", teacher: "Ravi Kumar", periodsPerWeek: 5 },
  { id: "3", subjectName: "Science", subjectCode: "SCI101", teacher: "Anil Mehta", periodsPerWeek: 4 },
  { id: "4", subjectName: "Hindi", subjectCode: "HIN101", teacher: "Rohan Verma", periodsPerWeek: 4 },
  { id: "5", subjectName: "Social Studies", subjectCode: "SOC101", teacher: "Neha Singh", periodsPerWeek: 3 },
  { id: "6", subjectName: "Computer", subjectCode: "COM101", teacher: "Amit Patel", periodsPerWeek: 2 },
];

const mockTeachers = [
  "Anita Sharma", "Ravi Kumar", "Sneha Patel", "Mohan Verma", 
  "Priya Singh", "Anil Mehta", "Kavita Joshi", "Suresh Gupta",
  "Rohan Verma", "Neha Singh", "Amit Patel", "Sunita Rao"
];

const classNames: Record<string, string> = {
  "1": "Grade 1",
  "2": "Grade 2",
  "3": "Grade 3",
  "4": "Grade 4",
  "5": "Grade 5",
};

export default function SubjectMapping() {
  const { classId } = useParams();
  const navigate = useNavigate();
  const className = classNames[classId || "1"] || "Grade 1";
  
  const [subjects, setSubjects] = useState<SubjectMapping[]>(mockSubjectMappings);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editData, setEditData] = useState<Partial<SubjectMapping>>({});
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [newSubject, setNewSubject] = useState({
    subjectName: "",
    subjectCode: "",
    teacher: "",
    periodsPerWeek: 4,
  });

  const totalPeriods = subjects.reduce((sum, s) => sum + s.periodsPerWeek, 0);

  const startEdit = (subject: SubjectMapping) => {
    setEditingId(subject.id);
    setEditData({ teacher: subject.teacher, periodsPerWeek: subject.periodsPerWeek });
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditData({});
  };

  const saveEdit = (id: string) => {
    setSubjects(subjects.map(s => 
      s.id === id ? { ...s, ...editData } : s
    ));
    setEditingId(null);
    setEditData({});
    toast({ title: "Success", description: "Subject mapping updated" });
  };

  const handleAddSubject = (e: React.FormEvent) => {
    e.preventDefault();
    const newMapping: SubjectMapping = {
      id: String(subjects.length + 1),
      ...newSubject,
    };
    setSubjects([...subjects, newMapping]);
    setNewSubject({ subjectName: "", subjectCode: "", teacher: "", periodsPerWeek: 4 });
    setIsAddDialogOpen(false);
    toast({ title: "Success", description: "Subject added successfully" });
  };

  const handleDeleteSubject = (id: string) => {
    if (confirm("Are you sure you want to remove this subject?")) {
      setSubjects(subjects.filter(s => s.id !== id));
      toast({ title: "Success", description: "Subject removed" });
    }
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
            <h1 className="text-3xl font-bold text-foreground">Subject Mapping</h1>
            <p className="text-muted-foreground mt-1">{className}</p>
          </div>
        </div>
        <Button onClick={() => setIsAddDialogOpen(true)} className="bg-primary hover:bg-primary/90">
          <Plus className="mr-2 h-4 w-4" />
          Add Subject
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Subjects</p>
                <p className="text-2xl font-bold text-foreground">{subjects.length}</p>
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
                <p className="text-sm text-muted-foreground">Total Periods/Week</p>
                <p className="text-2xl font-bold text-foreground">{totalPeriods}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-blue-500/10 flex items-center justify-center">
                <BookOpen className="h-6 w-6 text-blue-500" />
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Teachers Assigned</p>
                <p className="text-2xl font-bold text-foreground">
                  {new Set(subjects.map(s => s.teacher)).size}
                </p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-green-500/10 flex items-center justify-center">
                <BookOpen className="h-6 w-6 text-green-500" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Subject Mapping Table */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle className="text-foreground">Subject-Teacher Mapping</CardTitle>
          <CardDescription>Assign teachers and set periods for each subject</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="border-border hover:bg-muted/50">
                <TableHead className="text-muted-foreground">Subject Name</TableHead>
                <TableHead className="text-muted-foreground">Subject Code</TableHead>
                <TableHead className="text-muted-foreground">Assigned Teacher</TableHead>
                <TableHead className="text-muted-foreground">Periods/Week</TableHead>
                <TableHead className="text-right text-muted-foreground">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {subjects.map((subject) => (
                <TableRow key={subject.id} className="border-border hover:bg-muted/50">
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <BookOpen className="h-4 w-4 text-primary" />
                      <span className="font-medium text-foreground">{subject.subjectName}</span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline">{subject.subjectCode}</Badge>
                  </TableCell>
                  <TableCell>
                    {editingId === subject.id ? (
                      <Select 
                        value={editData.teacher} 
                        onValueChange={(v) => setEditData({ ...editData, teacher: v })}
                      >
                        <SelectTrigger className="w-[180px] bg-background">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent className="bg-popover">
                          {mockTeachers.map(teacher => (
                            <SelectItem key={teacher} value={teacher}>{teacher}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    ) : (
                      <span className="text-foreground">{subject.teacher}</span>
                    )}
                  </TableCell>
                  <TableCell>
                    {editingId === subject.id ? (
                      <Input
                        type="number"
                        value={editData.periodsPerWeek}
                        onChange={(e) => setEditData({ ...editData, periodsPerWeek: parseInt(e.target.value) })}
                        className="w-20 bg-background"
                        min={1}
                        max={10}
                      />
                    ) : (
                      <Badge className="bg-primary/10 text-primary border-primary/30">
                        {subject.periodsPerWeek} periods
                      </Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    {editingId === subject.id ? (
                      <div className="flex justify-end gap-2">
                        <Button variant="ghost" size="sm" onClick={() => saveEdit(subject.id)}>
                          <Save className="h-4 w-4 text-green-500" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={cancelEdit}>
                          <X className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    ) : (
                      <div className="flex justify-end gap-2">
                        <Button variant="ghost" size="sm" onClick={() => startEdit(subject)}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDeleteSubject(subject.id)}>
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Add Subject Dialog */}
      <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
        <DialogContent className="max-w-md bg-card border-border">
          <DialogHeader>
            <DialogTitle className="text-foreground">Add New Subject</DialogTitle>
            <DialogDescription>Add a subject and assign a teacher</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleAddSubject}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="subjectName">Subject Name *</Label>
                <Input
                  id="subjectName"
                  value={newSubject.subjectName}
                  onChange={(e) => setNewSubject({ ...newSubject, subjectName: e.target.value })}
                  placeholder="e.g., Mathematics"
                  className="bg-background"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="subjectCode">Subject Code *</Label>
                <Input
                  id="subjectCode"
                  value={newSubject.subjectCode}
                  onChange={(e) => setNewSubject({ ...newSubject, subjectCode: e.target.value })}
                  placeholder="e.g., MTH101"
                  className="bg-background"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="teacher">Assigned Teacher *</Label>
                <Select 
                  value={newSubject.teacher} 
                  onValueChange={(v) => setNewSubject({ ...newSubject, teacher: v })}
                >
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
              <div className="space-y-2">
                <Label htmlFor="periods">Periods per Week</Label>
                <Input
                  id="periods"
                  type="number"
                  value={newSubject.periodsPerWeek}
                  onChange={(e) => setNewSubject({ ...newSubject, periodsPerWeek: parseInt(e.target.value) })}
                  className="bg-background"
                  min={1}
                  max={10}
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" className="bg-primary hover:bg-primary/90">Add Subject</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
