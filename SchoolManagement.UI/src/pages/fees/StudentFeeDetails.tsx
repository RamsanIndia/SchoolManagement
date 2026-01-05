import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Search, User, GraduationCap } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";

interface FeeHistory {
  month: string;
  amount: number;
  paidOn: string;
  mode: string;
  receiptNo: string;
}

interface StudentFee {
  studentName: string;
  admissionNo: string;
  class: string;
  section: string;
  totalPayable: number;
  paid: number;
  balance: number;
  feeHistory: FeeHistory[];
}

const mockStudentData: StudentFee = {
  studentName: "Ananya Gupta",
  admissionNo: "S12345",
  class: "7",
  section: "A",
  totalPayable: 18000,
  paid: 12000,
  balance: 6000,
  feeHistory: [
    { month: "January 2025", amount: 1500, paidOn: "2025-01-05", mode: "UPI", receiptNo: "RCPT-2025-00101" },
    { month: "February 2025", amount: 1500, paidOn: "2025-02-06", mode: "Cash", receiptNo: "RCPT-2025-00145" },
    { month: "December 2024", amount: 1500, paidOn: "2024-12-10", mode: "Net Banking", receiptNo: "RCPT-2024-00987" },
    { month: "November 2024", amount: 1500, paidOn: "2024-11-08", mode: "UPI", receiptNo: "RCPT-2024-00876" },
    { month: "October 2024", amount: 1500, paidOn: "2024-10-05", mode: "Cash", receiptNo: "RCPT-2024-00765" },
    { month: "September 2024", amount: 1500, paidOn: "2024-09-07", mode: "Cheque", receiptNo: "RCPT-2024-00654" },
    { month: "August 2024", amount: 1500, paidOn: "2024-08-06", mode: "UPI", receiptNo: "RCPT-2024-00543" },
    { month: "July 2024", amount: 1500, paidOn: "2024-07-05", mode: "Cash", receiptNo: "RCPT-2024-00432" },
  ],
};

export default function StudentFeeDetails() {
  const [searchTerm, setSearchTerm] = useState("S12345");
  const [studentData] = useState<StudentFee>(mockStudentData);

  const pendingPercentage = ((studentData.balance / studentData.totalPayable) * 100).toFixed(1);
  const paidPercentage = ((studentData.paid / studentData.totalPayable) * 100).toFixed(1);

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Student Fee Details</h1>
        <p className="text-muted-foreground">View detailed fee information for individual students</p>
      </div>

      {/* Search Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by Admission No, Name, or Phone..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* Student Info Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-start gap-4">
            <Avatar className="h-20 w-20">
              <AvatarFallback className="text-2xl">
                {studentData.studentName
                  .split(" ")
                  .map((n) => n[0])
                  .join("")}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h2 className="text-2xl font-bold">{studentData.studentName}</h2>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
                <div>
                  <p className="text-sm text-muted-foreground">Admission No</p>
                  <p className="font-medium">{studentData.admissionNo}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Class</p>
                  <p className="font-medium">
                    {studentData.class} - {studentData.section}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Session</p>
                  <p className="font-medium">2024-25</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Status</p>
                  <Badge variant={studentData.balance > 0 ? "destructive" : "default"}>
                    {studentData.balance > 0 ? "Pending" : "Clear"}
                  </Badge>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Fee Summary Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Payable
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">₹{studentData.totalPayable.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground mt-1">Annual fee amount</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">Amount Paid</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              ₹{studentData.paid.toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground mt-1">{paidPercentage}% paid</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Balance Due
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-destructive">
              ₹{studentData.balance.toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground mt-1">{pendingPercentage}% pending</p>
          </CardContent>
        </Card>
      </div>

      {/* Fee History */}
      <Card>
        <CardHeader>
          <CardTitle>Payment History</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Receipt No</TableHead>
                <TableHead>Month</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Payment Date</TableHead>
                <TableHead>Mode</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {studentData.feeHistory.map((record, index) => (
                <TableRow key={index}>
                  <TableCell className="font-mono text-sm">{record.receiptNo}</TableCell>
                  <TableCell>{record.month}</TableCell>
                  <TableCell className="font-medium">₹{record.amount.toLocaleString()}</TableCell>
                  <TableCell>{record.paidOn}</TableCell>
                  <TableCell>
                    <Badge variant="outline">{record.mode}</Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
