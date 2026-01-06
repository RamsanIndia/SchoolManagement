import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
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
  CheckCircle,
  XCircle,
  AlertTriangle,
  Download,
  Printer,
  Check,
} from "lucide-react";
import { toast } from "@/hooks/use-toast";

const promotionSummary = {
  fromYear: "2023-24",
  toYear: "2024-25",
  fromClass: "5-A",
  toClass: "6-A",
  total: 45,
  promoted: 42,
  retained: 2,
  notEligible: 1,
};

const promotedStudents = [
  { id: "1", name: "Rohan Sharma", rollNo: "5A-01", newRollNo: "6A-01", marks: 78, attendance: 92 },
  { id: "2", name: "Aisha Verma", rollNo: "5A-02", newRollNo: "6A-02", marks: 85, attendance: 88 },
  { id: "3", name: "Kabir Khan", rollNo: "5A-03", newRollNo: "6A-03", marks: 72, attendance: 95 },
  { id: "6", name: "Sneha Patel", rollNo: "5A-06", newRollNo: "6A-04", marks: 82, attendance: 90 },
  { id: "7", name: "Arjun Reddy", rollNo: "5A-07", newRollNo: "6A-05", marks: 75, attendance: 87 },
];

const retainedStudents = [
  { id: "5", name: "Rahul Gupta", rollNo: "5A-05", reason: "Failed - Below minimum marks", marks: 28, attendance: 80 },
  { id: "4", name: "Priya Singh", rollNo: "5A-04", reason: "Low Attendance", marks: 45, attendance: 65 },
];

export default function PromotionPreview() {
  const navigate = useNavigate();
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const handleFinalize = () => {
    setIsProcessing(true);
    setTimeout(() => {
      setIsProcessing(false);
      setShowConfirmDialog(false);
      toast({
        title: "Promotion Completed",
        description: `Successfully promoted ${promotionSummary.promoted} students to Class ${promotionSummary.toClass}`,
      });
      navigate("/students");
    }, 2000);
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
            <h1 className="text-3xl font-bold">Promotion Preview</h1>
            <p className="text-muted-foreground">Review and finalize student promotions</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export Report
          </Button>
          <Button variant="outline">
            <Printer className="h-4 w-4 mr-2" />
            Print Summary
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-sm text-muted-foreground">From</p>
            <p className="text-xl font-bold">{promotionSummary.fromYear}</p>
            <p className="text-sm">Class {promotionSummary.fromClass}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-sm text-muted-foreground">To</p>
            <p className="text-xl font-bold">{promotionSummary.toYear}</p>
            <p className="text-sm">Class {promotionSummary.toClass}</p>
          </CardContent>
        </Card>
        <Card className="bg-green-500/10 border-green-500/20">
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-green-600">{promotionSummary.promoted}</p>
            <p className="text-sm text-green-600">Promoted</p>
          </CardContent>
        </Card>
        <Card className="bg-orange-500/10 border-orange-500/20">
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-orange-600">{promotionSummary.retained}</p>
            <p className="text-sm text-orange-600">Retained</p>
          </CardContent>
        </Card>
        <Card className="bg-red-500/10 border-red-500/20">
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold text-red-600">{promotionSummary.notEligible}</p>
            <p className="text-sm text-red-600">Not Eligible</p>
          </CardContent>
        </Card>
      </div>

      {/* Promoted Students */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckCircle className="h-5 w-5 text-green-500" />
            Promoted Students ({promotedStudents.length})
          </CardTitle>
          <CardDescription>Students being promoted to Class {promotionSummary.toClass}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Current Roll No</TableHead>
                  <TableHead>Student Name</TableHead>
                  <TableHead>New Roll No</TableHead>
                  <TableHead>Marks (%)</TableHead>
                  <TableHead>Attendance (%)</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {promotedStudents.map((student) => (
                  <TableRow key={student.id}>
                    <TableCell className="font-mono">{student.rollNo}</TableCell>
                    <TableCell className="font-medium">{student.name}</TableCell>
                    <TableCell className="font-mono">{student.newRollNo}</TableCell>
                    <TableCell>{student.marks}%</TableCell>
                    <TableCell>{student.attendance}%</TableCell>
                    <TableCell>
                      <Badge className="bg-green-500/10 text-green-600">
                        <CheckCircle className="h-3 w-3 mr-1" />
                        Promoted
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Retained Students */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-orange-500" />
            Retained Students ({retainedStudents.length})
          </CardTitle>
          <CardDescription>Students being retained in Class {promotionSummary.fromClass}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Roll No</TableHead>
                  <TableHead>Student Name</TableHead>
                  <TableHead>Marks (%)</TableHead>
                  <TableHead>Attendance (%)</TableHead>
                  <TableHead>Reason</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {retainedStudents.map((student) => (
                  <TableRow key={student.id}>
                    <TableCell className="font-mono">{student.rollNo}</TableCell>
                    <TableCell className="font-medium">{student.name}</TableCell>
                    <TableCell className={student.marks < 35 ? "text-red-600" : ""}>
                      {student.marks}%
                    </TableCell>
                    <TableCell className={student.attendance < 75 ? "text-red-600" : ""}>
                      {student.attendance}%
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">{student.reason}</TableCell>
                    <TableCell>
                      <Badge className="bg-orange-500/10 text-orange-600">
                        <XCircle className="h-3 w-3 mr-1" />
                        Retained
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-between">
        <Button variant="outline" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Mapping
        </Button>
        <Button onClick={() => setShowConfirmDialog(true)}>
          <Check className="h-4 w-4 mr-2" />
          Finalize Promotion
        </Button>
      </div>

      {/* Confirmation Dialog */}
      <Dialog open={showConfirmDialog} onOpenChange={setShowConfirmDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Promotion</DialogTitle>
            <DialogDescription>
              Are you sure you want to finalize this promotion? This action will:
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="flex items-center gap-3">
              <CheckCircle className="h-5 w-5 text-green-500" />
              <span>Promote {promotionSummary.promoted} students to Class {promotionSummary.toClass}</span>
            </div>
            <div className="flex items-center gap-3">
              <XCircle className="h-5 w-5 text-orange-500" />
              <span>Retain {promotionSummary.retained} students in Class {promotionSummary.fromClass}</span>
            </div>
            <div className="flex items-center gap-3">
              <AlertTriangle className="h-5 w-5 text-red-500" />
              <span>This action cannot be undone</span>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirmDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleFinalize} disabled={isProcessing}>
              {isProcessing ? "Processing..." : "Confirm Promotion"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
