import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { useToast } from "@/hooks/use-toast";
import { Calendar, Plus, Pencil, Trash2, Clock, MapPin, User, ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";

interface ExamSchedule {
  id: number;
  subject: string;
  date: string;
  startTime: string;
  endTime: string;
  room: string;
  invigilator: string;
  examName: string;
}

const mockSchedule: ExamSchedule[] = [
  { id: 1, subject: "Mathematics", date: "2025-02-10", startTime: "09:00 AM", endTime: "12:00 PM", room: "Room 101", invigilator: "Mr. Ramesh", examName: "Term 1 Examination" },
  { id: 2, subject: "Science", date: "2025-02-12", startTime: "09:00 AM", endTime: "12:00 PM", room: "Lab 1", invigilator: "Mrs. Kavitha", examName: "Term 1 Examination" },
  { id: 3, subject: "English", date: "2025-02-14", startTime: "09:00 AM", endTime: "12:00 PM", room: "Room 102", invigilator: "Ms. Priya", examName: "Term 1 Examination" },
  { id: 4, subject: "Social Studies", date: "2025-02-16", startTime: "09:00 AM", endTime: "12:00 PM", room: "Room 103", invigilator: "Mr. Suresh", examName: "Term 1 Examination" },
  { id: 5, subject: "Computer", date: "2025-02-18", startTime: "09:00 AM", endTime: "12:00 PM", room: "Computer Lab", invigilator: "Mr. Anil", examName: "Term 1 Examination" },
];

export default function ExamSchedule() {
  const [schedule, setSchedule] = useState<ExamSchedule[]>(mockSchedule);
  const [filterExam, setFilterExam] = useState("all");
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingSchedule, setEditingSchedule] = useState<ExamSchedule | null>(null);
  const { toast } = useToast();

  const [formData, setFormData] = useState({
    subject: "",
    date: "",
    startTime: "",
    endTime: "",
    room: "",
    invigilator: "",
    examName: "Term 1 Examination",
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

  const filteredSchedule = schedule
    .filter(item => filterExam === "all" || item.examName === filterExam)
    .sort((a, b) => {
      if (!sortColumn) return 0;
      const aValue = a[sortColumn as keyof ExamSchedule];
      const bValue = b[sortColumn as keyof ExamSchedule];
      if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
      if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingSchedule) {
      setSchedule(schedule.map(item => item.id === editingSchedule.id ? { ...editingSchedule, ...formData } : item));
      toast({ title: "Success", description: "Schedule updated successfully" });
    } else {
      const newSchedule = { ...formData, id: Math.max(...schedule.map(s => s.id)) + 1 };
      setSchedule([...schedule, newSchedule]);
      toast({ title: "Success", description: "Schedule created successfully" });
    }
    handleCloseDialog();
  };

  const handleDelete = (id: number) => {
    setSchedule(schedule.filter(item => item.id !== id));
    toast({ title: "Success", description: "Schedule deleted successfully" });
  };

  const handleEdit = (item: ExamSchedule) => {
    setEditingSchedule(item);
    setFormData(item);
    setIsDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setIsDialogOpen(false);
    setEditingSchedule(null);
    setFormData({ subject: "", date: "", startTime: "", endTime: "", room: "", invigilator: "", examName: "Term 1 Examination" });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Exam Schedule</h1>
          <p className="text-muted-foreground">View and manage examination timetable</p>
        </div>
        
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button className="bg-gradient-primary">
              <Plus className="mr-2 h-4 w-4" />
              Add Schedule
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[600px]">
            <DialogHeader>
              <DialogTitle>{editingSchedule ? 'Edit Schedule' : 'Add Exam Schedule'}</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="examName">Exam Name</Label>
                  <Select value={formData.examName} onValueChange={(value) => setFormData({ ...formData, examName: value })}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Term 1 Examination">Term 1 Examination</SelectItem>
                      <SelectItem value="Unit Test 2">Unit Test 2</SelectItem>
                      <SelectItem value="Final Examination">Final Examination</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="subject">Subject</Label>
                  <Select value={formData.subject} onValueChange={(value) => setFormData({ ...formData, subject: value })}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select subject" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Mathematics">Mathematics</SelectItem>
                      <SelectItem value="Science">Science</SelectItem>
                      <SelectItem value="English">English</SelectItem>
                      <SelectItem value="Social Studies">Social Studies</SelectItem>
                      <SelectItem value="Computer">Computer</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="date">Date</Label>
                  <Input
                    id="date"
                    type="date"
                    value={formData.date}
                    onChange={(e) => setFormData({ ...formData, date: e.target.value })}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="startTime">Start Time</Label>
                  <Input
                    id="startTime"
                    type="time"
                    value={formData.startTime}
                    onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="endTime">End Time</Label>
                  <Input
                    id="endTime"
                    type="time"
                    value={formData.endTime}
                    onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                    required
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="room">Room No</Label>
                  <Input
                    id="room"
                    value={formData.room}
                    onChange={(e) => setFormData({ ...formData, room: e.target.value })}
                    placeholder="Room 101"
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="invigilator">Invigilator</Label>
                  <Input
                    id="invigilator"
                    value={formData.invigilator}
                    onChange={(e) => setFormData({ ...formData, invigilator: e.target.value })}
                    placeholder="Mr. Ramesh"
                    required
                  />
                </div>
              </div>
              <div className="flex justify-end space-x-2">
                <Button type="button" variant="outline" onClick={handleCloseDialog}>Cancel</Button>
                <Button type="submit" className="bg-gradient-primary">{editingSchedule ? 'Update' : 'Add'}</Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Filter */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Calendar className="h-5 w-5" />
            Filter Schedule
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <Label>Select Exam</Label>
            <Select value={filterExam} onValueChange={setFilterExam}>
              <SelectTrigger className="w-full md:w-[300px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Exams</SelectItem>
                <SelectItem value="Term 1 Examination">Term 1 Examination</SelectItem>
                <SelectItem value="Unit Test 2">Unit Test 2</SelectItem>
                <SelectItem value="Final Examination">Final Examination</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Schedule Table */}
      <Card>
        <CardHeader>
          <CardTitle>Examination Schedule</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("subject")}>
                    <div className="flex items-center">
                      Subject
                      {getSortIcon("subject")}
                    </div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("date")}>
                    <div className="flex items-center">
                      Exam Date
                      {getSortIcon("date")}
                    </div>
                  </TableHead>
                  <TableHead className="cursor-pointer select-none hover:bg-muted/50" onClick={() => handleSort("startTime")}>
                    <div className="flex items-center">
                      Start Time
                      {getSortIcon("startTime")}
                    </div>
                  </TableHead>
                  <TableHead>End Time</TableHead>
                  <TableHead>Room No</TableHead>
                  <TableHead>Invigilator</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredSchedule.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.subject}</TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Calendar className="h-3 w-3 text-muted-foreground" />
                        <span>{new Date(item.date).toLocaleDateString()}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Clock className="h-3 w-3 text-muted-foreground" />
                        <span>{item.startTime}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Clock className="h-3 w-3 text-muted-foreground" />
                        <span>{item.endTime}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <MapPin className="h-3 w-3 text-muted-foreground" />
                        <span>{item.room}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <User className="h-3 w-3 text-muted-foreground" />
                        <span>{item.invigilator}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <Button variant="outline" size="sm" onClick={() => handleEdit(item)}>
                          <Pencil className="h-3 w-3" />
                        </Button>
                        <Button variant="outline" size="sm" onClick={() => handleDelete(item.id)}>
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
