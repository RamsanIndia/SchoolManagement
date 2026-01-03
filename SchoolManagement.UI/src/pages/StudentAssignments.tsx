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
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Search, Plus, Eye, Edit, Trash2, FileText, Calendar } from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface Assignment {
  id: string;
  title: string;
  subject: string;
  class: string;
  dueDate: string;
  status: "pending" | "submitted" | "graded";
  grade?: string;
  description: string;
  assignedDate: string;
}

const initialAssignments: Assignment[] = [
  {
    id: "1",
    title: "Math Problem Set 5",
    subject: "Mathematics",
    class: "10A",
    dueDate: "2024-12-15",
    status: "pending",
    description: "Complete exercises 1-20 from Chapter 5",
    assignedDate: "2024-12-01",
  },
  {
    id: "2",
    title: "Science Project",
    subject: "Physics",
    class: "10A",
    dueDate: "2024-12-20",
    status: "submitted",
    description: "Solar system model presentation",
    assignedDate: "2024-11-25",
  },
  {
    id: "3",
    title: "History Essay",
    subject: "History",
    class: "10B",
    dueDate: "2024-12-10",
    status: "graded",
    grade: "A",
    description: "World War II impact analysis - 1500 words",
    assignedDate: "2024-11-28",
  },
];

export default function StudentAssignments() {
  const { user } = useAuth();
  const [assignments, setAssignments] = useState<Assignment[]>(initialAssignments);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null);
  const [isViewDialogOpen, setIsViewDialogOpen] = useState(false);
  
  const [formData, setFormData] = useState({
    title: "",
    subject: "",
    class: "",
    dueDate: "",
    description: "",
  });

  if (!user || !["admin", "teacher"].includes(user.role)) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-muted-foreground">
          Access denied. This page is only available to administrators and teachers.
        </p>
      </div>
    );
  }

  const filteredAssignments = assignments.filter((assignment) => {
    const matchesSearch =
      assignment.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      assignment.subject.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === "all" || assignment.status === filterStatus;
    return matchesSearch && matchesStatus;
  });

  const handleAddAssignment = () => {
    setSelectedAssignment(null);
    setFormData({
      title: "",
      subject: "",
      class: "",
      dueDate: "",
      description: "",
    });
    setIsDialogOpen(true);
  };

  const handleEditAssignment = (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    setFormData({
      title: assignment.title,
      subject: assignment.subject,
      class: assignment.class,
      dueDate: assignment.dueDate,
      description: assignment.description,
    });
    setIsDialogOpen(true);
  };

  const handleViewAssignment = (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    setIsViewDialogOpen(true);
  };

  const handleSaveAssignment = () => {
    if (selectedAssignment) {
      setAssignments(
        assignments.map((a) =>
          a.id === selectedAssignment.id
            ? { ...a, ...formData }
            : a
        )
      );
      toast({
        title: "Success",
        description: "Assignment updated successfully",
      });
    } else {
      const newAssignment: Assignment = {
        id: (assignments.length + 1).toString(),
        ...formData,
        status: "pending",
        assignedDate: new Date().toISOString().split("T")[0],
      };
      setAssignments([...assignments, newAssignment]);
      toast({
        title: "Success",
        description: "Assignment created successfully",
      });
    }
    setIsDialogOpen(false);
  };

  const handleDeleteAssignment = (id: string) => {
    setAssignments(assignments.filter((a) => a.id !== id));
    toast({
      title: "Success",
      description: "Assignment deleted successfully",
    });
  };

  const getStatusBadge = (status: string) => {
    const variants: { [key: string]: "default" | "secondary" | "outline" } = {
      pending: "secondary",
      submitted: "default",
      graded: "outline",
    };
    return (
      <Badge variant={variants[status] || "default"}>
        {status.charAt(0).toUpperCase() + status.slice(1)}
      </Badge>
    );
  };

  const stats = {
    total: assignments.length,
    pending: assignments.filter((a) => a.status === "pending").length,
    submitted: assignments.filter((a) => a.status === "submitted").length,
    graded: assignments.filter((a) => a.status === "graded").length,
  };

  return (
    <div className="p-8 space-y-8">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Student Assignments</h1>
          <p className="text-muted-foreground">
            Manage and track student assignments
          </p>
        </div>
        <Button onClick={handleAddAssignment}>
          <Plus className="mr-2 h-4 w-4" /> Create Assignment
        </Button>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Assignments</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.pending}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Submitted</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.submitted}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Graded</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.graded}</div>
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
                  placeholder="Search assignments..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <Select value={filterStatus} onValueChange={setFilterStatus}>
              <SelectTrigger className="w-full md:w-[180px]">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="pending">Pending</SelectItem>
                <SelectItem value="submitted">Submitted</SelectItem>
                <SelectItem value="graded">Graded</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Title</TableHead>
                <TableHead>Subject</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Due Date</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Grade</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredAssignments.map((assignment) => (
                <TableRow key={assignment.id}>
                  <TableCell className="font-medium">{assignment.title}</TableCell>
                  <TableCell>{assignment.subject}</TableCell>
                  <TableCell>{assignment.class}</TableCell>
                  <TableCell>{assignment.dueDate}</TableCell>
                  <TableCell>{getStatusBadge(assignment.status)}</TableCell>
                  <TableCell>{assignment.grade || "-"}</TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleViewAssignment(assignment)}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleEditAssignment(assignment)}
                      >
                        <Edit className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDeleteAssignment(assignment.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {selectedAssignment ? "Edit Assignment" : "Create Assignment"}
            </DialogTitle>
            <DialogDescription>
              {selectedAssignment
                ? "Update the assignment details below"
                : "Fill in the details to create a new assignment"}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="title">Title</Label>
              <Input
                id="title"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                placeholder="Assignment title"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="subject">Subject</Label>
                <Input
                  id="subject"
                  value={formData.subject}
                  onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                  placeholder="Subject"
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="class">Class</Label>
                <Input
                  id="class"
                  value={formData.class}
                  onChange={(e) => setFormData({ ...formData, class: e.target.value })}
                  placeholder="Class"
                />
              </div>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="dueDate">Due Date</Label>
              <Input
                id="dueDate"
                type="date"
                value={formData.dueDate}
                onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Assignment description and instructions"
                rows={4}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSaveAssignment}>
              {selectedAssignment ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={isViewDialogOpen} onOpenChange={setIsViewDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{selectedAssignment?.title}</DialogTitle>
            <DialogDescription>Assignment Details</DialogDescription>
          </DialogHeader>
          {selectedAssignment && (
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-muted-foreground">Subject</Label>
                  <p className="font-medium">{selectedAssignment.subject}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Class</Label>
                  <p className="font-medium">{selectedAssignment.class}</p>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-muted-foreground">Assigned Date</Label>
                  <p className="font-medium">{selectedAssignment.assignedDate}</p>
                </div>
                <div>
                  <Label className="text-muted-foreground">Due Date</Label>
                  <p className="font-medium">{selectedAssignment.dueDate}</p>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label className="text-muted-foreground">Status</Label>
                  <div className="mt-1">{getStatusBadge(selectedAssignment.status)}</div>
                </div>
                {selectedAssignment.grade && (
                  <div>
                    <Label className="text-muted-foreground">Grade</Label>
                    <p className="font-medium text-lg">{selectedAssignment.grade}</p>
                  </div>
                )}
              </div>
              <div>
                <Label className="text-muted-foreground">Description</Label>
                <p className="mt-2 text-sm">{selectedAssignment.description}</p>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button onClick={() => setIsViewDialogOpen(false)}>Close</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
