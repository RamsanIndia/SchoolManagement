import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
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
import {
  Search,
  CheckCircle,
  XCircle,
  Clock,
  Eye,
  FileText,
} from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface LeaveRequest {
  id: string;
  studentName: string;
  class: string;
  parentName: string;
  fromDate: string;
  toDate: string;
  days: number;
  type: string;
  reason: string;
  status: "pending" | "approved" | "rejected";
  appliedOn: string;
  attachment?: string;
}

const mockRequests: LeaveRequest[] = [
  {
    id: "1",
    studentName: "Rohan Sharma",
    class: "5-A",
    parentName: "Rajesh Sharma",
    fromDate: "2024-12-02",
    toDate: "2024-12-04",
    days: 3,
    type: "Medical",
    reason: "Fever and cold. Doctor advised rest for 3 days.",
    status: "pending",
    appliedOn: "2024-12-01",
    attachment: "medical_certificate.pdf",
  },
  {
    id: "2",
    studentName: "Aisha Verma",
    class: "6-B",
    parentName: "Anita Verma",
    fromDate: "2024-12-15",
    toDate: "2024-12-15",
    days: 1,
    type: "Travel",
    reason: "Family travel to attend a wedding in hometown.",
    status: "pending",
    appliedOn: "2024-12-05",
  },
  {
    id: "3",
    studentName: "Kabir Khan",
    class: "4-C",
    parentName: "Imran Khan",
    fromDate: "2024-12-10",
    toDate: "2024-12-12",
    days: 3,
    type: "Personal",
    reason: "Family emergency - grandfather unwell.",
    status: "pending",
    appliedOn: "2024-12-08",
  },
  {
    id: "4",
    studentName: "Sneha Patel",
    class: "5-A",
    parentName: "Vikram Patel",
    fromDate: "2024-11-20",
    toDate: "2024-11-22",
    days: 3,
    type: "Medical",
    reason: "Dengue fever. Under medical care.",
    status: "approved",
    appliedOn: "2024-11-18",
  },
];

export default function LeaveApproval() {
  const [requests, setRequests] = useState<LeaveRequest[]>(mockRequests);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");
  const [filterClass, setFilterClass] = useState("all");
  const [selectedRequest, setSelectedRequest] = useState<LeaveRequest | null>(null);
  const [remarks, setRemarks] = useState("");

  const filteredRequests = requests.filter((req) => {
    const matchesSearch = req.studentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      req.parentName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === "all" || req.status === filterStatus;
    const matchesClass = filterClass === "all" || req.class === filterClass;
    return matchesSearch && matchesStatus && matchesClass;
  });

  const handleApprove = () => {
    if (!selectedRequest) return;
    setRequests((prev) =>
      prev.map((r) => (r.id === selectedRequest.id ? { ...r, status: "approved" } : r))
    );
    setSelectedRequest(null);
    setRemarks("");
    toast({
      title: "Leave Approved",
      description: `Leave application for ${selectedRequest.studentName} has been approved.`,
    });
  };

  const handleReject = () => {
    if (!selectedRequest) return;
    setRequests((prev) =>
      prev.map((r) => (r.id === selectedRequest.id ? { ...r, status: "rejected" } : r))
    );
    setSelectedRequest(null);
    setRemarks("");
    toast({
      title: "Leave Rejected",
      description: `Leave application for ${selectedRequest.studentName} has been rejected.`,
    });
  };

  const getStatusBadge = (status: LeaveRequest["status"]) => {
    switch (status) {
      case "approved":
        return <Badge className="bg-green-500/10 text-green-600"><CheckCircle className="h-3 w-3 mr-1" /> Approved</Badge>;
      case "rejected":
        return <Badge className="bg-red-500/10 text-red-600"><XCircle className="h-3 w-3 mr-1" /> Rejected</Badge>;
      case "pending":
        return <Badge className="bg-orange-500/10 text-orange-600"><Clock className="h-3 w-3 mr-1" /> Pending</Badge>;
    }
  };

  const stats = {
    pending: requests.filter((r) => r.status === "pending").length,
    approved: requests.filter((r) => r.status === "approved").length,
    rejected: requests.filter((r) => r.status === "rejected").length,
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold">Leave Approval</h1>
        <p className="text-muted-foreground">Review and approve student leave requests</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-4">
        <Card className="bg-orange-500/10 border-orange-500/20">
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-orange-500/20">
                <Clock className="h-6 w-6 text-orange-500" />
              </div>
              <div>
                <p className="text-3xl font-bold text-orange-600">{stats.pending}</p>
                <p className="text-sm text-orange-600">Pending Requests</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-green-500/10 border-green-500/20">
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-green-500/20">
                <CheckCircle className="h-6 w-6 text-green-500" />
              </div>
              <div>
                <p className="text-3xl font-bold text-green-600">{stats.approved}</p>
                <p className="text-sm text-green-600">Approved</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-red-500/10 border-red-500/20">
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-red-500/20">
                <XCircle className="h-6 w-6 text-red-500" />
              </div>
              <div>
                <p className="text-3xl font-bold text-red-600">{stats.rejected}</p>
                <p className="text-sm text-red-600">Rejected</p>
              </div>
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
                placeholder="Search by student or parent name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={filterStatus} onValueChange={setFilterStatus}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="pending">Pending</SelectItem>
                <SelectItem value="approved">Approved</SelectItem>
                <SelectItem value="rejected">Rejected</SelectItem>
              </SelectContent>
            </Select>
            <Select value={filterClass} onValueChange={setFilterClass}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Class" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Classes</SelectItem>
                <SelectItem value="4-C">4-C</SelectItem>
                <SelectItem value="5-A">5-A</SelectItem>
                <SelectItem value="6-B">6-B</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Table */}
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Student</TableHead>
                  <TableHead>Class</TableHead>
                  <TableHead>Parent</TableHead>
                  <TableHead>Duration</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Applied On</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredRequests.map((request) => (
                  <TableRow key={request.id}>
                    <TableCell>
                      <div className="flex items-center gap-3">
                        <Avatar className="h-8 w-8">
                          <AvatarFallback className="text-xs">
                            {request.studentName.split(" ").map((n) => n[0]).join("")}
                          </AvatarFallback>
                        </Avatar>
                        <span className="font-medium">{request.studentName}</span>
                      </div>
                    </TableCell>
                    <TableCell>{request.class}</TableCell>
                    <TableCell>{request.parentName}</TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{request.days} day(s)</p>
                        <p className="text-xs text-muted-foreground">
                          {new Date(request.fromDate).toLocaleDateString()} - {new Date(request.toDate).toLocaleDateString()}
                        </p>
                      </div>
                    </TableCell>
                    <TableCell><Badge variant="outline">{request.type}</Badge></TableCell>
                    <TableCell>{new Date(request.appliedOn).toLocaleDateString()}</TableCell>
                    <TableCell>{getStatusBadge(request.status)}</TableCell>
                    <TableCell>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setSelectedRequest(request)}
                      >
                        <Eye className="h-4 w-4 mr-1" />
                        View
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Request Detail Dialog */}
      <Dialog open={!!selectedRequest} onOpenChange={() => setSelectedRequest(null)}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Leave Request Details</DialogTitle>
            <DialogDescription>
              Review the leave application and take action
            </DialogDescription>
          </DialogHeader>
          {selectedRequest && (
            <div className="space-y-4 py-4">
              <div className="flex items-center gap-4 p-4 bg-muted/50 rounded-lg">
                <Avatar className="h-12 w-12">
                  <AvatarFallback>
                    {selectedRequest.studentName.split(" ").map((n) => n[0]).join("")}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-bold">{selectedRequest.studentName}</p>
                  <p className="text-sm text-muted-foreground">Class {selectedRequest.class}</p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-muted-foreground">Parent Name</p>
                  <p className="font-medium">{selectedRequest.parentName}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Leave Type</p>
                  <Badge variant="outline">{selectedRequest.type}</Badge>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">From Date</p>
                  <p className="font-medium">{new Date(selectedRequest.fromDate).toLocaleDateString()}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">To Date</p>
                  <p className="font-medium">{new Date(selectedRequest.toDate).toLocaleDateString()}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Total Days</p>
                  <p className="font-medium">{selectedRequest.days} day(s)</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Applied On</p>
                  <p className="font-medium">{new Date(selectedRequest.appliedOn).toLocaleDateString()}</p>
                </div>
              </div>

              <div>
                <p className="text-sm text-muted-foreground mb-1">Reason</p>
                <p className="p-3 bg-muted/50 rounded-lg">{selectedRequest.reason}</p>
              </div>

              {selectedRequest.attachment && (
                <div className="flex items-center gap-2 p-3 border rounded-lg">
                  <FileText className="h-5 w-5 text-muted-foreground" />
                  <span className="text-sm">{selectedRequest.attachment}</span>
                  <Button variant="ghost" size="sm" className="ml-auto">View</Button>
                </div>
              )}

              {selectedRequest.status === "pending" && (
                <div>
                  <p className="text-sm text-muted-foreground mb-1">Remarks (Optional)</p>
                  <Textarea
                    value={remarks}
                    onChange={(e) => setRemarks(e.target.value)}
                    placeholder="Add any remarks or instructions..."
                    rows={3}
                  />
                </div>
              )}
            </div>
          )}
          <DialogFooter>
            {selectedRequest?.status === "pending" ? (
              <>
                <Button variant="outline" onClick={handleReject}>
                  <XCircle className="h-4 w-4 mr-2" />
                  Reject
                </Button>
                <Button onClick={handleApprove}>
                  <CheckCircle className="h-4 w-4 mr-2" />
                  Approve
                </Button>
              </>
            ) : (
              <Button variant="outline" onClick={() => setSelectedRequest(null)}>
                Close
              </Button>
            )}
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
