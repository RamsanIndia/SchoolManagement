import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { useToast } from "@/hooks/use-toast";
import { BookOpen, Plus, Pencil, Trash2, Search, Calendar, ArrowUpDown, ArrowUp, ArrowDown, FileText, ClipboardList } from "lucide-react";
import { useNavigate } from "react-router-dom";

interface Exam {
  id: number;
  name: string;
  type: string;
  session: string;
  status: "Draft" | "Scheduled" | "Published" | "Completed";
  class?: string;
  startDate?: string;
  endDate?: string;
}

const mockExams: Exam[] = [
  { id: 1, name: "Term 1 Examination", type: "Term", session: "2024-25", status: "Scheduled", class: "10-A", startDate: "2025-02-10", endDate: "2025-02-20" },
  { id: 2, name: "Unit Test 2", type: "Monthly Test", session: "2024-25", status: "Draft", class: "9-B", startDate: "2025-03-01", endDate: "2025-03-05" },
  { id: 3, name: "Final Examination", type: "Term", session: "2024-25", status: "Completed", class: "12-A", startDate: "2024-12-01", endDate: "2024-12-15" },
  { id: 4, name: "Mid Term Exam", type: "Term", session: "2024-25", status: "Published", class: "11-A", startDate: "2025-01-15", endDate: "2025-01-25" },
  { id: 5, name: "Unit Test 1", type: "Unit Test", session: "2024-25", status: "Completed", class: "10-B", startDate: "2024-11-10", endDate: "2024-11-12" },
];

export default function ExamDashboard() {
  const [exams, setExams] = useState<Exam[]>(mockExams);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterSession, setFilterSession] = useState("all");
  const [filterClass, setFilterClass] = useState("all");
  const [filterType, setFilterType] = useState("all");
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingExam, setEditingExam] = useState<Exam | null>(null);
  const { toast } = useToast();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    name: "",
    type: "Term",
    session: "2024-25",
    status: "Draft" as Exam["status"],
    class: "",
    startDate: "",
    endDate: "",
  });

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

  const getStatusBadge = (status: string) => {
    const variants: Record<string, string> = {
      Draft: "bg-muted text-muted-foreground",
      Scheduled: "bg-blue-500 text-white",
      Published: "bg-status-late text-white",
      Completed: "bg-status-present text-white",
    };
    return <Badge className={variants[status] || "bg-muted"}>{status}</Badge>;
  };

  const filteredExams = exams
    .filter(exam => {
      const matchesSearch = exam.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          exam.type.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesSession = filterSession === "all" || exam.session === filterSession;
      const matchesClass = filterClass === "all" || exam.class === filterClass;
      const matchesType = filterType === "all" || exam.type === filterType;
      return matchesSearch && matchesSession && matchesClass && matchesType;
    })
    .sort((a, b) => {
      if (!sortColumn) return 0;
      const aValue = a[sortColumn as keyof Exam];
      const bValue = b[sortColumn as keyof Exam];
      if (aValue! < bValue!) return sortDirection === "asc" ? -1 : 1;
      if (aValue! > bValue!) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingExam) {
      setExams(exams.map(ex => ex.id === editingExam.id ? { ...editingExam, ...formData } : ex));
      toast({ title: "Success", description: "Exam updated successfully" });
    } else {
      const newExam = { ...formData, id: Math.max(...exams.map(e => e.id)) + 1 };
      setExams([...exams, newExam]);
      toast({ title: "Success", description: "Exam created successfully" });
    }
    handleCloseDialog();
  };

  const handleDelete = (id: number) => {
    setExams(exams.filter(ex => ex.id !== id));
    toast({ title: "Success", description: "Exam deleted successfully" });
  };

  const handleEdit = (exam: Exam) => {
    setEditingExam(exam);
    setFormData({
      name: exam.name,
      type: exam.type,
      session: exam.session,
      status: exam.status,
      class: exam.class || "",
      startDate: exam.startDate || "",
      endDate: exam.endDate || "",
    });
    setIsDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setIsDialogOpen(false);
    setEditingExam(null);
    setFormData({ name: "", type: "Term", session: "2024-25", status: "Draft", class: "", startDate: "", endDate: "" });
  };

  const stats = {
    total: exams.length,
    draft: exams.filter(e => e.status === 'Draft').length,
    scheduled: exams.filter(e => e.status === 'Scheduled').length,
    published: exams.filter(e => e.status === 'Published').length,
    completed: exams.filter(e => e.status === 'Completed').length,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Examination Dashboard</h1>
          <p className="text-muted-foreground">Manage all examinations and assessments</p>
        </div>
        
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate("/examinations/schedule")}>
            <Calendar className="mr-2 h-4 w-4" />
            View Schedule
          </Button>
          <Button variant="outline" onClick={() => navigate("/examinations/marks-entry")}>
            <ClipboardList className="mr-2 h-4 w-4" />
            Enter Marks
          </Button>
          <Button variant="outline" onClick={() => navigate("/examinations/results")}>
            <FileText className="mr-2 h-4 w-4" />
            Results
          </Button>
          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogTrigger asChild>
              <Button className="bg-gradient-primary">
                <Plus className="mr-2 h-4 w-4" />
                Create Exam
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[600px]">
              <DialogHeader>
                <DialogTitle>{editingExam ? 'Edit Exam' : 'Create New Exam'}</DialogTitle>
              </DialogHeader>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">Exam Name</Label>
                    <Input
                      id="name"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      placeholder="Term 1 Examination"
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="type">Exam Type</Label>
                    <Select value={formData.type} onValueChange={(value) => setFormData({ ...formData, type: value })}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Term">Term</SelectItem>
                        <SelectItem value="Monthly Test">Monthly Test</SelectItem>
                        <SelectItem value="Unit Test">Unit Test</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="session">Session</Label>
                    <Select value={formData.session} onValueChange={(value) => setFormData({ ...formData, session: value })}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="2024-25">2024-25</SelectItem>
                        <SelectItem value="2025-26">2025-26</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="class">Class</Label>
                    <Input
                      id="class"
                      value={formData.class}
                      onChange={(e) => setFormData({ ...formData, class: e.target.value })}
                      placeholder="10-A"
                      required
                    />
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="startDate">Start Date</Label>
                    <Input
                      id="startDate"
                      type="date"
                      value={formData.startDate}
                      onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="endDate">End Date</Label>
                    <Input
                      id="endDate"
                      type="date"
                      value={formData.endDate}
                      onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                      required
                    />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="status">Status</Label>
                  <Select value={formData.status} onValueChange={(value: Exam["status"]) => setFormData({ ...formData, status: value })}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Draft">Draft</SelectItem>
                      <SelectItem value="Scheduled">Scheduled</SelectItem>
                      <SelectItem value="Published">Published</SelectItem>
                      <SelectItem value="Completed">Completed</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="flex justify-end space-x-2">
                  <Button type="button" variant="outline" onClick={handleCloseDialog}>Cancel</Button>
                  <Button type="submit" className="bg-gradient-primary">{editingExam ? 'Update' : 'Create'}</Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Exams</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Draft</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-muted-foreground">{stats.draft}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Scheduled</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-blue-500">{stats.scheduled}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Published</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-late">{stats.published}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">Completed</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-present">{stats.completed}</div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <BookOpen className="h-5 w-5" />
            Filter Exams
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-4 gap-4">
            <div className="space-y-2">
              <Label>Search</Label>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search exams..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label>Session</Label>
              <Select value={filterSession} onValueChange={setFilterSession}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Sessions</SelectItem>
                  <SelectItem value="2024-25">2024-25</SelectItem>
                  <SelectItem value="2025-26">2025-26</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Class</Label>
              <Select value={filterClass} onValueChange={setFilterClass}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Classes</SelectItem>
                  <SelectItem value="9-A">9-A</SelectItem>
                  <SelectItem value="10-A">10-A</SelectItem>
                  <SelectItem value="11-A">11-A</SelectItem>
                  <SelectItem value="12-A">12-A</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2">
              <Label>Exam Type</Label>
              <Select value={filterType} onValueChange={setFilterType}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Types</SelectItem>
                  <SelectItem value="Term">Term</SelectItem>
                  <SelectItem value="Monthly Test">Monthly Test</SelectItem>
                  <SelectItem value="Unit Test">Unit Test</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Exams Table */}
      <Card>
        <CardHeader>
          <CardTitle>Examination List</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("name")}>
                    <div className="flex items-center">
                      Exam Name
                      {getSortIcon("name")}
                    </div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("type")}>
                    <div className="flex items-center">
                      Type
                      {getSortIcon("type")}
                    </div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("session")}>
                    <div className="flex items-center">
                      Session
                      {getSortIcon("session")}
                    </div>
                  </TableHead>
                  <TableHead>Class</TableHead>
                  <TableHead>Duration</TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("status")}>
                    <div className="flex items-center">
                      Status
                      {getSortIcon("status")}
                    </div>
                  </TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredExams.map((exam) => (
                  <TableRow key={exam.id}>
                    <TableCell className="font-medium">{exam.name}</TableCell>
                    <TableCell>{exam.type}</TableCell>
                    <TableCell>{exam.session}</TableCell>
                    <TableCell>{exam.class}</TableCell>
                    <TableCell>
                      {exam.startDate && exam.endDate && (
                        <span className="text-sm text-muted-foreground">
                          {new Date(exam.startDate).toLocaleDateString()} - {new Date(exam.endDate).toLocaleDateString()}
                        </span>
                      )}
                    </TableCell>
                    <TableCell>{getStatusBadge(exam.status)}</TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Button variant="outline" size="sm" onClick={() => handleEdit(exam)}>
                          <Pencil className="h-3 w-3" />
                        </Button>
                        <Button variant="outline" size="sm" onClick={() => handleDelete(exam.id)}>
                          <Trash2 className="h-3 w-3" />
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
