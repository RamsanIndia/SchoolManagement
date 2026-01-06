/**
 * Section Management Page
 * Manage sections for a specific class
 */

import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { 
  Plus, Edit, Trash2, Users, ArrowLeft, UserPlus, 
  DoorOpen, MoreHorizontal, GraduationCap
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
import { toast } from "@/hooks/use-toast";

interface Section {
  id: string;
  name: string;
  strength: number;
  teacher: string;
  room: string;
  capacity: number;
}

const mockSections: Record<string, Section[]> = {
  "1": [
    { id: "1", name: "A", strength: 32, teacher: "Anita Sharma", room: "Room 102", capacity: 40 },
    { id: "2", name: "B", strength: 32, teacher: "Rohan Verma", room: "Room 103", capacity: 40 },
  ],
  "2": [
    { id: "1", name: "A", strength: 28, teacher: "Ravi Kumar", room: "Room 104", capacity: 35 },
    { id: "2", name: "B", strength: 26, teacher: "Neha Singh", room: "Room 105", capacity: 35 },
    { id: "3", name: "C", strength: 25, teacher: "Amit Patel", room: "Room 106", capacity: 35 },
  ],
};

const mockTeachers = [
  "Anita Sharma", "Ravi Kumar", "Sneha Patel", "Mohan Verma", 
  "Priya Singh", "Anil Mehta", "Kavita Joshi", "Suresh Gupta",
  "Rohan Verma", "Neha Singh", "Amit Patel", "Sunita Rao"
];

const mockRooms = [
  "Room 101", "Room 102", "Room 103", "Room 104", "Room 105",
  "Room 106", "Room 107", "Room 108", "Room 201", "Room 202"
];

const classNames: Record<string, string> = {
  "1": "Grade 1",
  "2": "Grade 2",
  "3": "Grade 3",
  "4": "Grade 4",
  "5": "Grade 5",
};

export default function SectionManagement() {
  const { classId } = useParams();
  const navigate = useNavigate();
  const className = classNames[classId || "1"] || "Grade 1";
  
  const [sections, setSections] = useState<Section[]>(mockSections[classId || "1"] || mockSections["1"]);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedSection, setSelectedSection] = useState<Section | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    teacher: "",
    room: "",
    capacity: 40,
  });

  const stats = {
    totalSections: sections.length,
    totalStudents: sections.reduce((sum, s) => sum + s.strength, 0),
    avgStrength: sections.length > 0 ? Math.round(sections.reduce((sum, s) => sum + s.strength, 0) / sections.length) : 0,
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedSection) {
      setSections(sections.map(s => 
        s.id === selectedSection.id 
          ? { ...s, name: formData.name, teacher: formData.teacher, room: formData.room, capacity: formData.capacity }
          : s
      ));
      toast({ title: "Success", description: "Section updated successfully" });
    } else {
      const newSection: Section = {
        id: String(sections.length + 1),
        name: formData.name,
        strength: 0,
        teacher: formData.teacher,
        room: formData.room,
        capacity: formData.capacity,
      };
      setSections([...sections, newSection]);
      toast({ title: "Success", description: "Section added successfully" });
    }
    resetForm();
  };

  const handleEdit = (section: Section) => {
    setSelectedSection(section);
    setFormData({
      name: section.name,
      teacher: section.teacher,
      room: section.room,
      capacity: section.capacity,
    });
    setIsDialogOpen(true);
  };

  const handleDelete = (id: string) => {
    if (confirm("Are you sure you want to delete this section?")) {
      setSections(sections.filter(s => s.id !== id));
      toast({ title: "Success", description: "Section deleted successfully" });
    }
  };

  const resetForm = () => {
    setFormData({ name: "", teacher: "", room: "", capacity: 40 });
    setSelectedSection(null);
    setIsDialogOpen(false);
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
            <h1 className="text-3xl font-bold text-foreground">Manage Sections</h1>
            <p className="text-muted-foreground mt-1">{className}</p>
          </div>
        </div>
        <Button onClick={() => setIsDialogOpen(true)} className="bg-primary hover:bg-primary/90">
          <Plus className="mr-2 h-4 w-4" />
          Add Section
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Total Sections</p>
                <p className="text-2xl font-bold text-foreground">{stats.totalSections}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-primary/10 flex items-center justify-center">
                <GraduationCap className="h-6 w-6 text-primary" />
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
                <p className="text-sm text-muted-foreground">Avg. Strength</p>
                <p className="text-2xl font-bold text-foreground">{stats.avgStrength}</p>
              </div>
              <div className="h-12 w-12 rounded-lg bg-green-500/10 flex items-center justify-center">
                <Users className="h-6 w-6 text-green-500" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Sections Table */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle className="text-foreground">Sections</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="border-border hover:bg-muted/50">
                <TableHead className="text-muted-foreground">Section</TableHead>
                <TableHead className="text-muted-foreground">Strength</TableHead>
                <TableHead className="text-muted-foreground">Class Teacher</TableHead>
                <TableHead className="text-muted-foreground">Assigned Room</TableHead>
                <TableHead className="text-right text-muted-foreground">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sections.map((section) => (
                <TableRow key={section.id} className="border-border hover:bg-muted/50">
                  <TableCell>
                    <Badge variant="outline" className="bg-primary/10 text-primary border-primary/30">
                      Section {section.name}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-foreground">
                    <div className="flex items-center gap-2">
                      <span>{section.strength}</span>
                      <span className="text-muted-foreground text-xs">/ {section.capacity}</span>
                    </div>
                  </TableCell>
                  <TableCell className="text-foreground">{section.teacher}</TableCell>
                  <TableCell className="text-foreground">
                    <div className="flex items-center gap-2">
                      <DoorOpen className="h-4 w-4 text-muted-foreground" />
                      {section.room}
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="sm">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" className="bg-popover border-border">
                        <DropdownMenuItem onClick={() => navigate(`/classes/${classId}/assign-teacher?section=${section.name}`)}>
                          <UserPlus className="mr-2 h-4 w-4" /> Assign Teacher
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => navigate(`/classes/${classId}/students?section=${section.name}`)}>
                          <Users className="mr-2 h-4 w-4" /> View Students
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => handleEdit(section)}>
                          <Edit className="mr-2 h-4 w-4" /> Edit
                        </DropdownMenuItem>
                        <DropdownMenuItem onClick={() => handleDelete(section.id)} className="text-destructive">
                          <Trash2 className="mr-2 h-4 w-4" /> Delete
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Add/Edit Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-w-md bg-card border-border">
          <DialogHeader>
            <DialogTitle className="text-foreground">{selectedSection ? "Edit Section" : "Add New Section"}</DialogTitle>
            <DialogDescription>
              {selectedSection ? "Update section details" : "Add a new section to this class"}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="name">Section Name *</Label>
                <Input
                  id="name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., A, B, C"
                  className="bg-background"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="teacher">Class Teacher</Label>
                <Select value={formData.teacher} onValueChange={(v) => setFormData({ ...formData, teacher: v })}>
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
                <Label htmlFor="room">Assigned Room</Label>
                <Select value={formData.room} onValueChange={(v) => setFormData({ ...formData, room: v })}>
                  <SelectTrigger className="bg-background">
                    <SelectValue placeholder="Select a room" />
                  </SelectTrigger>
                  <SelectContent className="bg-popover">
                    {mockRooms.map(room => (
                      <SelectItem key={room} value={room}>{room}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="capacity">Capacity</Label>
                <Input
                  id="capacity"
                  type="number"
                  value={formData.capacity}
                  onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) })}
                  className="bg-background"
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={resetForm}>Cancel</Button>
              <Button type="submit" className="bg-primary hover:bg-primary/90">
                {selectedSection ? "Update Section" : "Add Section"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
