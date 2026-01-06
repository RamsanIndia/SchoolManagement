import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { LeaveApplication } from "@/types/navigation.types";
import { useToast } from "@/hooks/use-toast";
import { 
  Calendar, Search, Plus, CheckCircle, XCircle, Clock,
  ArrowUpDown, ArrowUp, ArrowDown, Eye, FileText
} from "lucide-react";

export default function LeaveManagement() {
  const [leaves, setLeaves] = useState<LeaveApplication[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedLeave, setSelectedLeave] = useState<LeaveApplication | null>(null);
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const { toast } = useToast();

  const [formData, setFormData] = useState({
    employeeId: "",
    employeeName: "",
    leaveType: "casual" as const,
    startDate: "",
    endDate: "",
    reason: "",
  });

  useEffect(() => {
    loadLeaves();
  }, []);

  const loadLeaves = () => {
    // Mock data - replace with actual API call
    const mockData: LeaveApplication[] = [
      {
        id: "1",
        employeeId: "EMP001",
        employeeName: "John Doe",
        leaveType: "casual",
        startDate: "2024-02-01",
        endDate: "2024-02-02",
        days: 2,
        reason: "Personal work",
        status: "pending",
        appliedDate: "2024-01-25",
        createdAt: "2024-01-25T10:00:00Z",
        updatedAt: "2024-01-25T10:00:00Z",
      },
      {
        id: "2",
        employeeId: "EMP002",
        employeeName: "Jane Smith",
        leaveType: "sick",
        startDate: "2024-01-28",
        endDate: "2024-01-29",
        days: 2,
        reason: "Fever and cold",
        status: "approved",
        appliedDate: "2024-01-27",
        approvedBy: "HR Manager",
        approvedDate: "2024-01-27",
        documents: ["medical-certificate.pdf"],
        createdAt: "2024-01-27T09:00:00Z",
        updatedAt: "2024-01-27T14:00:00Z",
      },
      {
        id: "3",
        employeeId: "EMP003",
        employeeName: "Michael Brown",
        leaveType: "earned",
        startDate: "2024-02-10",
        endDate: "2024-02-14",
        days: 5,
        reason: "Family vacation",
        status: "approved",
        appliedDate: "2024-01-20",
        approvedBy: "Department Head",
        approvedDate: "2024-01-21",
        createdAt: "2024-01-20T11:00:00Z",
        updatedAt: "2024-01-21T15:00:00Z",
      },
      {
        id: "4",
        employeeId: "EMP004",
        employeeName: "Emily Davis",
        leaveType: "casual",
        startDate: "2024-01-30",
        endDate: "2024-01-30",
        days: 1,
        reason: "Personal emergency",
        status: "rejected",
        appliedDate: "2024-01-29",
        approvedBy: "HR Manager",
        approvedDate: "2024-01-29",
        rejectionReason: "Insufficient leave balance",
        createdAt: "2024-01-29T16:00:00Z",
        updatedAt: "2024-01-29T17:00:00Z",
      },
      {
        id: "5",
        employeeId: "EMP005",
        employeeName: "David Wilson",
        leaveType: "sick",
        startDate: "2024-02-05",
        endDate: "2024-02-06",
        days: 2,
        reason: "Medical checkup",
        status: "pending",
        appliedDate: "2024-02-03",
        createdAt: "2024-02-03T10:00:00Z",
        updatedAt: "2024-02-03T10:00:00Z",
      },
      {
        id: "6",
        employeeId: "EMP006",
        employeeName: "Sarah Johnson",
        leaveType: "maternity",
        startDate: "2024-03-01",
        endDate: "2024-05-30",
        days: 90,
        reason: "Maternity leave",
        status: "approved",
        appliedDate: "2024-01-15",
        approvedBy: "HR Director",
        approvedDate: "2024-01-16",
        documents: ["maternity-certificate.pdf"],
        createdAt: "2024-01-15T09:00:00Z",
        updatedAt: "2024-01-16T10:00:00Z",
      },
    ];
    setLeaves(mockData);
  };

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
    const styles: Record<string, { bg: string; icon: React.ReactNode }> = {
      pending: { bg: "bg-amber-500", icon: <Clock className="h-3 w-3 mr-1" /> },
      approved: { bg: "bg-status-present", icon: <CheckCircle className="h-3 w-3 mr-1" /> },
      rejected: { bg: "bg-status-absent", icon: <XCircle className="h-3 w-3 mr-1" /> },
      cancelled: { bg: "bg-muted", icon: <XCircle className="h-3 w-3 mr-1" /> },
    };
    const style = styles[status] || { bg: "bg-muted", icon: null };
    return (
      <Badge className={`${style.bg} flex items-center w-fit`}>
        {style.icon}
        {status}
      </Badge>
    );
  };

  const getLeaveTypeLabel = (type: string) => {
    const labels: Record<string, string> = {
      casual: "Casual Leave",
      sick: "Sick Leave",
      earned: "Earned Leave",
      maternity: "Maternity Leave",
      paternity: "Paternity Leave",
      unpaid: "Unpaid Leave",
    };
    return labels[type] || type;
  };

  const filteredLeaves = leaves
    .filter(leave => {
      const matchesSearch = leave.employeeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          leave.employeeId.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter === "all" || leave.status === statusFilter;
      return matchesSearch && matchesStatus;
    })
    .sort((a, b) => {
      if (!sortColumn) return 0;
      const aValue: any = a[sortColumn as keyof typeof a];
      const bValue: any = b[sortColumn as keyof typeof b];
      if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
      if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });

  const stats = {
    total: leaves.length,
    pending: leaves.filter(l => l.status === "pending").length,
    approved: leaves.filter(l => l.status === "approved").length,
    rejected: leaves.filter(l => l.status === "rejected").length,
  };

  const handleApprove = (id: string) => {
    setLeaves(leaves.map(l => 
      l.id === id ? { ...l, status: "approved", approvedBy: "Current User", approvedDate: new Date().toISOString() } : l
    ));
    toast({ title: "Success", description: "Leave application approved" });
  };

  const handleReject = (id: string) => {
    setLeaves(leaves.map(l => 
      l.id === id ? { ...l, status: "rejected", approvedBy: "Current User", approvedDate: new Date().toISOString(), rejectionReason: "Not approved" } : l
    ));
    toast({ title: "Success", description: "Leave application rejected" });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const newLeave: LeaveApplication = {
      id: String(leaves.length + 1),
      ...formData,
      days: Math.ceil((new Date(formData.endDate).getTime() - new Date(formData.startDate).getTime()) / (1000 * 60 * 60 * 24)) + 1,
      status: "pending",
      appliedDate: new Date().toISOString().split('T')[0],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    setLeaves([newLeave, ...leaves]);
    toast({ title: "Success", description: "Leave application submitted" });
    setIsDialogOpen(false);
    setFormData({
      employeeId: "",
      employeeName: "",
      leaveType: "casual",
      startDate: "",
      endDate: "",
      reason: "",
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Leave Management</h1>
          <p className="text-muted-foreground">Manage employee leave applications</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button className="bg-gradient-primary">
              <Plus className="mr-2 h-4 w-4" />
              Apply Leave
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Apply for Leave</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div className="space-y-2">
                <Label>Employee ID</Label>
                <Input
                  value={formData.employeeId}
                  onChange={(e) => setFormData({ ...formData, employeeId: e.target.value })}
                  placeholder="EMP001"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label>Employee Name</Label>
                <Input
                  value={formData.employeeName}
                  onChange={(e) => setFormData({ ...formData, employeeName: e.target.value })}
                  placeholder="John Doe"
                  required
                />
              </div>
              <div className="space-y-2">
                <Label>Leave Type</Label>
                <Select value={formData.leaveType} onValueChange={(value: any) => setFormData({ ...formData, leaveType: value })}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="casual">Casual Leave</SelectItem>
                    <SelectItem value="sick">Sick Leave</SelectItem>
                    <SelectItem value="earned">Earned Leave</SelectItem>
                    <SelectItem value="maternity">Maternity Leave</SelectItem>
                    <SelectItem value="paternity">Paternity Leave</SelectItem>
                    <SelectItem value="unpaid">Unpaid Leave</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Start Date</Label>
                  <Input
                    type="date"
                    value={formData.startDate}
                    onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label>End Date</Label>
                  <Input
                    type="date"
                    value={formData.endDate}
                    onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                    required
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label>Reason</Label>
                <Textarea
                  value={formData.reason}
                  onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                  placeholder="Reason for leave"
                  required
                />
              </div>
              <Button type="submit" className="w-full bg-gradient-primary">Submit Application</Button>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <FileText className="h-4 w-4" />
              Total Applications
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Pending
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-amber-500">{stats.pending}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <CheckCircle className="h-4 w-4" />
              Approved
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-present">{stats.approved}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <XCircle className="h-4 w-4" />
              Rejected
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-absent">{stats.rejected}</div>
          </CardContent>
        </Card>
      </div>

      {/* Table */}
      <Card>
        <CardHeader>
          <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Leave Applications
            </CardTitle>
            <div className="flex flex-wrap gap-2 items-center w-full md:w-auto">
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-[150px]">
                  <SelectValue placeholder="Filter by status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="pending">Pending</SelectItem>
                  <SelectItem value="approved">Approved</SelectItem>
                  <SelectItem value="rejected">Rejected</SelectItem>
                </SelectContent>
              </Select>

              <div className="flex items-center gap-2">
                <Search className="h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search employees..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-[200px]"
                />
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("employeeId")}
                  >
                    <div className="flex items-center">
                      Employee
                      {getSortIcon("employeeId")}
                    </div>
                  </TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("leaveType")}
                  >
                    <div className="flex items-center">
                      Leave Type
                      {getSortIcon("leaveType")}
                    </div>
                  </TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("startDate")}
                  >
                    <div className="flex items-center">
                      Start Date
                      {getSortIcon("startDate")}
                    </div>
                  </TableHead>
                  <TableHead>End Date</TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("days")}
                  >
                    <div className="flex items-center">
                      Days
                      {getSortIcon("days")}
                    </div>
                  </TableHead>
                  <TableHead>Reason</TableHead>
                  <TableHead 
                    className="cursor-pointer select-none hover:bg-muted/50"
                    onClick={() => handleSort("status")}
                  >
                    <div className="flex items-center">
                      Status
                      {getSortIcon("status")}
                    </div>
                  </TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredLeaves.map((leave) => (
                  <TableRow key={leave.id}>
                    <TableCell>
                      <div>
                        <div className="font-medium">{leave.employeeName}</div>
                        <div className="text-sm text-muted-foreground">{leave.employeeId}</div>
                      </div>
                    </TableCell>
                    <TableCell>{getLeaveTypeLabel(leave.leaveType)}</TableCell>
                    <TableCell>{new Date(leave.startDate).toLocaleDateString()}</TableCell>
                    <TableCell>{new Date(leave.endDate).toLocaleDateString()}</TableCell>
                    <TableCell>{leave.days}</TableCell>
                    <TableCell className="max-w-[200px] truncate">{leave.reason}</TableCell>
                    <TableCell>{getStatusBadge(leave.status)}</TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        {leave.status === "pending" && (
                          <>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleApprove(leave.id)}
                              className="text-status-present"
                            >
                              <CheckCircle className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleReject(leave.id)}
                              className="text-status-absent"
                            >
                              <XCircle className="h-4 w-4" />
                            </Button>
                          </>
                        )}
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setSelectedLeave(leave)}
                        >
                          <Eye className="h-4 w-4" />
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
