import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
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
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Plus,
  Search,
  Eye,
  Edit,
  Trash2,
  Upload,
  Download,
  CheckCircle,
  Clock,
  AlertTriangle,
  FileText,
} from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface Assignment {
  id: string;
  title: string;
  subject: string;
  class: string;
  section: string;
  type: "homework" | "classwork";
  description: string;
  dueDate: string;
  createdAt: string;
  attachments: string[];
  submissions: number;
  totalStudents: number;
}

interface Submission {
  id: string;
  studentName: string;
  rollNo: string;
  submittedAt: string;
  status: "submitted" | "late" | "pending";
  marks?: number;
  remarks?: string;
  attachment?: string;
}

const mockAssignments: Assignment[] = [
  {
    id: "1",
    title: "Chapter 2 Exercises",
    subject: "Mathematics",
    class: "6",
    section: "A",
    type: "homework",
    description: "Complete exercises 1-20 from Chapter 2",
    dueDate: "2024-12-12",
    createdAt: "2024-12-05",
    attachments: ["chapter2_worksheet.pdf"],
    submissions: 35,
    totalStudents: 45,
  },
  {
    id: "2",
    title: "Poem Summary",
    subject: "English",
    class: "5",
    section: "B",
    type: "homework",
    description: "Write a summary of the poem 'The Road Not Taken'",
    dueDate: "2024-12-15",
    createdAt: "2024-12-08",
    attachments: [],
    submissions: 28,
    totalStudents: 42,
  },
];

const mockSubmissions: Submission[] = [
  { id: "1", studentName: "Rohan Sharma", rollNo: "5A-01", submittedAt: "2024-12-10 10:30 AM", status: "submitted", marks: 8, remarks: "Good work!", attachment: "rohan_hw.pdf" },
  { id: "2", studentName: "Aisha Verma", rollNo: "5A-02", submittedAt: "2024-12-11 02:15 PM", status: "submitted", marks: 9, attachment: "aisha_hw.pdf" },
  { id: "3", studentName: "Kabir Khan", rollNo: "5A-03", submittedAt: "2024-12-12 11:00 PM", status: "late", attachment: "kabir_hw.pdf" },
  { id: "4", studentName: "Priya Singh", rollNo: "5A-04", submittedAt: "", status: "pending" },
  { id: "5", studentName: "Rahul Gupta", rollNo: "5A-05", submittedAt: "", status: "pending" },
];

export default function TeacherAssignments() {
  const [assignments, setAssignments] = useState<Assignment[]>(mockAssignments);
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showSubmissionsDialog, setShowSubmissionsDialog] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null);
  const [activeTab, setActiveTab] = useState("all");
  const [submissions, setSubmissions] = useState<Submission[]>(mockSubmissions);

  const [formData, setFormData] = useState({
    title: "",
    subject: "",
    class: "",
    section: "",
    type: "homework" as "homework" | "classwork",
    description: "",
    dueDate: "",
  });

  const handleCreateAssignment = () => {
    const newAssignment: Assignment = {
      id: (assignments.length + 1).toString(),
      ...formData,
      createdAt: new Date().toISOString().split("T")[0],
      attachments: [],
      submissions: 0,
      totalStudents: 45,
    };
    setAssignments([newAssignment, ...assignments]);
    setShowCreateDialog(false);
    setFormData({
      title: "",
      subject: "",
      class: "",
      section: "",
      type: "homework",
      description: "",
      dueDate: "",
    });
    toast({
      title: "Assignment Created",
      description: "The assignment has been created and students have been notified.",
    });
  };

  const handleViewSubmissions = (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    setShowSubmissionsDialog(true);
  };

  const handleGradeSubmission = (id: string, marks: number, remarks: string) => {
    setSubmissions((prev) =>
      prev.map((s) => (s.id === id ? { ...s, marks, remarks } : s))
    );
    toast({ title: "Graded", description: "Submission has been graded successfully." });
  };

  const getStatusBadge = (status: Submission["status"]) => {
    switch (status) {
      case "submitted":
        return <Badge className="bg-green-500/10 text-green-600"><CheckCircle className="h-3 w-3 mr-1" /> Submitted</Badge>;
      case "late":
        return <Badge className="bg-orange-500/10 text-orange-600"><Clock className="h-3 w-3 mr-1" /> Late</Badge>;
      case "pending":
        return <Badge className="bg-red-500/10 text-red-600"><AlertTriangle className="h-3 w-3 mr-1" /> Pending</Badge>;
    }
  };

  const filteredAssignments = assignments.filter((a) =>
    a.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
    a.subject.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Assignment Management</h1>
          <p className="text-muted-foreground">Create, manage, and review student assignments</p>
        </div>
        <Button onClick={() => setShowCreateDialog(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Create Assignment
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold">{assignments.length}</p>
            <p className="text-sm text-muted-foreground">Total Assignments</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-green-600">
              {assignments.filter((a) => new Date(a.dueDate) > new Date()).length}
            </p>
            <p className="text-sm text-muted-foreground">Active</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-blue-600">
              {assignments.reduce((sum, a) => sum + a.submissions, 0)}
            </p>
            <p className="text-sm text-muted-foreground">Total Submissions</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-orange-600">
              {assignments.reduce((sum, a) => sum + (a.totalStudents - a.submissions), 0)}
            </p>
            <p className="text-sm text-muted-foreground">Pending</p>
          </CardContent>
        </Card>
      </div>

      {/* Assignments List */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Assignments</CardTitle>
            <div className="relative w-64">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search assignments..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {filteredAssignments.map((assignment) => (
              <Card key={assignment.id} className="hover:border-primary/50 transition-colors">
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between">
                    <div className="flex items-start gap-4">
                      <div className="p-3 rounded-lg bg-primary/10">
                        <FileText className="h-6 w-6 text-primary" />
                      </div>
                      <div>
                        <h3 className="font-bold text-lg">{assignment.title}</h3>
                        <div className="flex items-center gap-2 mt-1">
                          <Badge variant="outline">{assignment.subject}</Badge>
                          <Badge variant="secondary">Class {assignment.class}-{assignment.section}</Badge>
                          <Badge className={assignment.type === "homework" ? "bg-blue-500/10 text-blue-600" : "bg-purple-500/10 text-purple-600"}>
                            {assignment.type === "homework" ? "Homework" : "Classwork"}
                          </Badge>
                        </div>
                        <p className="text-sm text-muted-foreground mt-2">{assignment.description}</p>
                        <div className="flex items-center gap-4 mt-3 text-sm text-muted-foreground">
                          <span>Due: {new Date(assignment.dueDate).toLocaleDateString()}</span>
                          <span>•</span>
                          <span>Created: {new Date(assignment.createdAt).toLocaleDateString()}</span>
                          {assignment.attachments.length > 0 && (
                            <>
                              <span>•</span>
                              <span>{assignment.attachments.length} attachment(s)</span>
                            </>
                          )}
                        </div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="mb-2">
                        <span className="text-2xl font-bold">{assignment.submissions}</span>
                        <span className="text-muted-foreground">/{assignment.totalStudents}</span>
                      </div>
                      <p className="text-sm text-muted-foreground mb-3">submissions</p>
                      <div className="flex gap-2">
                        <Button variant="outline" size="sm" onClick={() => handleViewSubmissions(assignment)}>
                          <Eye className="h-4 w-4 mr-1" />
                          Review
                        </Button>
                        <Button variant="ghost" size="sm">
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="sm">
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Create Assignment Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Create New Assignment</DialogTitle>
            <DialogDescription>
              Create a new assignment for your students
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Title *</Label>
              <Input
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                placeholder="Assignment title"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Subject *</Label>
                <Select value={formData.subject} onValueChange={(v) => setFormData({ ...formData, subject: v })}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select subject" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Mathematics">Mathematics</SelectItem>
                    <SelectItem value="English">English</SelectItem>
                    <SelectItem value="Science">Science</SelectItem>
                    <SelectItem value="Hindi">Hindi</SelectItem>
                    <SelectItem value="Social Studies">Social Studies</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>Type *</Label>
                <Select value={formData.type} onValueChange={(v: "homework" | "classwork") => setFormData({ ...formData, type: v })}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="homework">Homework</SelectItem>
                    <SelectItem value="classwork">Classwork</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Class *</Label>
                <Select value={formData.class} onValueChange={(v) => setFormData({ ...formData, class: v })}>
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
                <Label>Section *</Label>
                <Select value={formData.section} onValueChange={(v) => setFormData({ ...formData, section: v })}>
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
            </div>
            <div className="space-y-2">
              <Label>Due Date *</Label>
              <Input
                type="date"
                value={formData.dueDate}
                onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                min={new Date().toISOString().split("T")[0]}
              />
            </div>
            <div className="space-y-2">
              <Label>Description</Label>
              <Textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Assignment instructions and details..."
                rows={4}
              />
            </div>
            <div className="space-y-2">
              <Label>Attachments</Label>
              <Button variant="outline" className="w-full">
                <Upload className="h-4 w-4 mr-2" />
                Upload Files
              </Button>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCreateDialog(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleCreateAssignment}
              disabled={!formData.title || !formData.subject || !formData.class || !formData.section || !formData.dueDate}
            >
              Create Assignment
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Submissions Review Dialog */}
      <Dialog open={showSubmissionsDialog} onOpenChange={setShowSubmissionsDialog}>
        <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Review Submissions: {selectedAssignment?.title}</DialogTitle>
            <DialogDescription>
              {selectedAssignment?.subject} • Class {selectedAssignment?.class}-{selectedAssignment?.section}
            </DialogDescription>
          </DialogHeader>
          <Tabs value={activeTab} onValueChange={setActiveTab} className="mt-4">
            <TabsList>
              <TabsTrigger value="all">All ({submissions.length})</TabsTrigger>
              <TabsTrigger value="submitted">Submitted ({submissions.filter((s) => s.status === "submitted").length})</TabsTrigger>
              <TabsTrigger value="late">Late ({submissions.filter((s) => s.status === "late").length})</TabsTrigger>
              <TabsTrigger value="pending">Pending ({submissions.filter((s) => s.status === "pending").length})</TabsTrigger>
            </TabsList>
            <TabsContent value={activeTab} className="mt-4">
              <div className="border rounded-lg">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Student</TableHead>
                      <TableHead>Roll No</TableHead>
                      <TableHead>Submitted At</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Attachment</TableHead>
                      <TableHead>Marks</TableHead>
                      <TableHead>Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {submissions
                      .filter((s) => activeTab === "all" || s.status === activeTab)
                      .map((submission) => (
                        <TableRow key={submission.id}>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Avatar className="h-8 w-8">
                                <AvatarFallback className="text-xs">
                                  {submission.studentName.split(" ").map((n) => n[0]).join("")}
                                </AvatarFallback>
                              </Avatar>
                              {submission.studentName}
                            </div>
                          </TableCell>
                          <TableCell>{submission.rollNo}</TableCell>
                          <TableCell>{submission.submittedAt || "-"}</TableCell>
                          <TableCell>{getStatusBadge(submission.status)}</TableCell>
                          <TableCell>
                            {submission.attachment ? (
                              <Button variant="ghost" size="sm">
                                <Download className="h-4 w-4 mr-1" />
                                View
                              </Button>
                            ) : (
                              "-"
                            )}
                          </TableCell>
                          <TableCell>
                            {submission.marks !== undefined ? (
                              <Badge variant="outline">{submission.marks}/10</Badge>
                            ) : (
                              "-"
                            )}
                          </TableCell>
                          <TableCell>
                            {submission.status !== "pending" && (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => handleGradeSubmission(submission.id, 8, "Good effort!")}
                              >
                                Grade
                              </Button>
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                  </TableBody>
                </Table>
              </div>
            </TabsContent>
          </Tabs>
        </DialogContent>
      </Dialog>
    </div>
  );
}
