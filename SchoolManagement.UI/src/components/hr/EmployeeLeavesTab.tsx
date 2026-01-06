import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { LeaveApplication, LeaveBalance } from "@/types/navigation.types";
import { Plus, Calendar } from "lucide-react";

interface Props {
  employeeId: string;
}

export default function EmployeeLeavesTab({ employeeId }: Props) {
  const [leaves, setLeaves] = useState<LeaveApplication[]>([]);
  const [balance, setBalance] = useState<LeaveBalance | null>(null);

  useEffect(() => {
    // Mock data - replace with actual API call
    const mockLeaves: LeaveApplication[] = [
      {
        id: "1",
        employeeId,
        employeeName: "John Doe",
        leaveType: "casual",
        startDate: "2024-02-01",
        endDate: "2024-02-02",
        days: 2,
        reason: "Personal work",
        status: "approved",
        appliedDate: "2024-01-25",
        approvedBy: "Manager",
        approvedDate: "2024-01-26",
        createdAt: "2024-01-25T10:00:00Z",
        updatedAt: "2024-01-26T14:00:00Z",
      },
    ];

    const mockBalance: LeaveBalance = {
      employeeId,
      year: 2024,
      casual: { total: 12, used: 2, remaining: 10 },
      sick: { total: 10, used: 1, remaining: 9 },
      earned: { total: 15, used: 0, remaining: 15 },
      maternity: { total: 0, used: 0, remaining: 0 },
      paternity: { total: 0, used: 0, remaining: 0 },
    };

    setLeaves(mockLeaves);
    setBalance(mockBalance);
  }, [employeeId]);

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      pending: "bg-amber-500",
      approved: "bg-status-present",
      rejected: "bg-status-absent",
      cancelled: "bg-muted",
    };
    return <Badge className={colors[status] || "bg-muted"}>{status}</Badge>;
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

  return (
    <div className="space-y-4">
      {/* Leave Balance Cards */}
      {balance && (
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Casual Leave
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{balance.casual.remaining}</div>
              <p className="text-xs text-muted-foreground">
                of {balance.casual.total} remaining ({balance.casual.used} used)
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Sick Leave
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{balance.sick.remaining}</div>
              <p className="text-xs text-muted-foreground">
                of {balance.sick.total} remaining ({balance.sick.used} used)
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Earned Leave
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{balance.earned.remaining}</div>
              <p className="text-xs text-muted-foreground">
                of {balance.earned.total} remaining ({balance.earned.used} used)
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Leave Applications Table */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Leave Applications
            </CardTitle>
            <Button className="bg-gradient-primary">
              <Plus className="mr-2 h-4 w-4" />
              Apply Leave
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Leave Type</TableHead>
                <TableHead>Start Date</TableHead>
                <TableHead>End Date</TableHead>
                <TableHead>Days</TableHead>
                <TableHead>Reason</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Applied On</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {leaves.map((leave) => (
                <TableRow key={leave.id}>
                  <TableCell>{getLeaveTypeLabel(leave.leaveType)}</TableCell>
                  <TableCell>{new Date(leave.startDate).toLocaleDateString()}</TableCell>
                  <TableCell>{new Date(leave.endDate).toLocaleDateString()}</TableCell>
                  <TableCell>{leave.days}</TableCell>
                  <TableCell>{leave.reason}</TableCell>
                  <TableCell>{getStatusBadge(leave.status)}</TableCell>
                  <TableCell>{new Date(leave.appliedDate).toLocaleDateString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
