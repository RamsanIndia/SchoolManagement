import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
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
  ArrowLeft,
  Calendar,
  Upload,
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
  Plus,
} from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface LeaveApplication {
  id: string;
  fromDate: string;
  toDate: string;
  days: number;
  reason: string;
  type: string;
  status: "pending" | "approved" | "rejected";
  appliedOn: string;
  reviewedBy?: string;
  remarks?: string;
}

const leaveHistory: LeaveApplication[] = [
  {
    id: "1",
    fromDate: "2024-12-02",
    toDate: "2024-12-04",
    days: 3,
    reason: "Fever and cold",
    type: "Medical",
    status: "pending",
    appliedOn: "2024-12-01",
  },
  {
    id: "2",
    fromDate: "2024-11-15",
    toDate: "2024-11-15",
    days: 1,
    reason: "Family function",
    type: "Personal",
    status: "approved",
    appliedOn: "2024-11-10",
    reviewedBy: "Mrs. Priya Sharma",
    remarks: "Approved. Please ensure homework is completed.",
  },
  {
    id: "3",
    fromDate: "2024-10-20",
    toDate: "2024-10-22",
    days: 3,
    reason: "Travel to hometown",
    type: "Personal",
    status: "rejected",
    appliedOn: "2024-10-18",
    reviewedBy: "Mrs. Priya Sharma",
    remarks: "Rejected due to upcoming exams.",
  },
];

export default function LeaveApply() {
  const navigate = useNavigate();
  const [showApplyDialog, setShowApplyDialog] = useState(false);
  const [applications, setApplications] = useState<LeaveApplication[]>(leaveHistory);
  const [formData, setFormData] = useState({
    fromDate: "",
    toDate: "",
    type: "",
    reason: "",
  });

  const calculateDays = () => {
    if (!formData.fromDate || !formData.toDate) return 0;
    const from = new Date(formData.fromDate);
    const to = new Date(formData.toDate);
    const diff = Math.ceil((to.getTime() - from.getTime()) / (1000 * 60 * 60 * 24)) + 1;
    return diff > 0 ? diff : 0;
  };

  const handleSubmit = () => {
    const newApplication: LeaveApplication = {
      id: (applications.length + 1).toString(),
      fromDate: formData.fromDate,
      toDate: formData.toDate,
      days: calculateDays(),
      reason: formData.reason,
      type: formData.type,
      status: "pending",
      appliedOn: new Date().toISOString().split("T")[0],
    };

    setApplications([newApplication, ...applications]);
    setShowApplyDialog(false);
    setFormData({ fromDate: "", toDate: "", type: "", reason: "" });
    toast({
      title: "Leave Application Submitted",
      description: "Your leave application has been submitted for review.",
    });
  };

  const getStatusBadge = (status: LeaveApplication["status"]) => {
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
    total: applications.length,
    approved: applications.filter((a) => a.status === "approved").length,
    pending: applications.filter((a) => a.status === "pending").length,
    rejected: applications.filter((a) => a.status === "rejected").length,
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Leave Application</h1>
            <p className="text-muted-foreground">Apply for leave and view application history</p>
          </div>
        </div>
        <Button onClick={() => setShowApplyDialog(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Apply for Leave
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold">{stats.total}</p>
            <p className="text-sm text-muted-foreground">Total Applications</p>
          </CardContent>
        </Card>
        <Card className="bg-green-500/10 border-green-500/20">
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-green-600">{stats.approved}</p>
            <p className="text-sm text-green-600">Approved</p>
          </CardContent>
        </Card>
        <Card className="bg-orange-500/10 border-orange-500/20">
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-orange-600">{stats.pending}</p>
            <p className="text-sm text-orange-600">Pending</p>
          </CardContent>
        </Card>
        <Card className="bg-red-500/10 border-red-500/20">
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-red-600">{stats.rejected}</p>
            <p className="text-sm text-red-600">Rejected</p>
          </CardContent>
        </Card>
      </div>

      {/* Leave History */}
      <Card>
        <CardHeader>
          <CardTitle>Leave History</CardTitle>
          <CardDescription>View all your leave applications</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Applied On</TableHead>
                  <TableHead>From Date</TableHead>
                  <TableHead>To Date</TableHead>
                  <TableHead>Days</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Reason</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Remarks</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {applications.map((application) => (
                  <TableRow key={application.id}>
                    <TableCell>{new Date(application.appliedOn).toLocaleDateString()}</TableCell>
                    <TableCell>{new Date(application.fromDate).toLocaleDateString()}</TableCell>
                    <TableCell>{new Date(application.toDate).toLocaleDateString()}</TableCell>
                    <TableCell>{application.days}</TableCell>
                    <TableCell><Badge variant="outline">{application.type}</Badge></TableCell>
                    <TableCell className="max-w-xs truncate">{application.reason}</TableCell>
                    <TableCell>{getStatusBadge(application.status)}</TableCell>
                    <TableCell className="max-w-xs truncate text-sm text-muted-foreground">
                      {application.remarks || "-"}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Apply Leave Dialog */}
      <Dialog open={showApplyDialog} onOpenChange={setShowApplyDialog}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Apply for Leave</DialogTitle>
            <DialogDescription>
              Submit a new leave application for your child
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>From Date *</Label>
                <Input
                  type="date"
                  value={formData.fromDate}
                  onChange={(e) => setFormData({ ...formData, fromDate: e.target.value })}
                  min={new Date().toISOString().split("T")[0]}
                />
              </div>
              <div className="space-y-2">
                <Label>To Date *</Label>
                <Input
                  type="date"
                  value={formData.toDate}
                  onChange={(e) => setFormData({ ...formData, toDate: e.target.value })}
                  min={formData.fromDate || new Date().toISOString().split("T")[0]}
                />
              </div>
            </div>

            {calculateDays() > 0 && (
              <div className="p-3 bg-muted rounded-lg text-center">
                <span className="text-sm text-muted-foreground">Total Days: </span>
                <span className="font-bold">{calculateDays()}</span>
              </div>
            )}

            <div className="space-y-2">
              <Label>Leave Type *</Label>
              <Select value={formData.type} onValueChange={(v) => setFormData({ ...formData, type: v })}>
                <SelectTrigger>
                  <SelectValue placeholder="Select leave type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Medical">Medical</SelectItem>
                  <SelectItem value="Personal">Personal</SelectItem>
                  <SelectItem value="Family Emergency">Family Emergency</SelectItem>
                  <SelectItem value="Travel">Travel</SelectItem>
                  <SelectItem value="Other">Other</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Reason *</Label>
              <Textarea
                value={formData.reason}
                onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                placeholder="Please provide the reason for leave..."
                rows={4}
              />
            </div>

            <div className="space-y-2">
              <Label>Attachment (Optional)</Label>
              <div className="flex items-center gap-2">
                <Button variant="outline" size="sm" className="w-full">
                  <Upload className="h-4 w-4 mr-2" />
                  Upload Document
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                Upload medical certificate or supporting document (PDF, JPG, PNG)
              </p>
            </div>

            <div className="p-3 border rounded-lg bg-muted/30">
              <div className="flex items-start gap-2">
                <AlertCircle className="h-4 w-4 text-muted-foreground mt-0.5" />
                <p className="text-xs text-muted-foreground">
                  Leave applications are subject to approval by the class teacher. 
                  You will be notified once your application is reviewed.
                </p>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowApplyDialog(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleSubmit}
              disabled={!formData.fromDate || !formData.toDate || !formData.type || !formData.reason}
            >
              Submit Application
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
