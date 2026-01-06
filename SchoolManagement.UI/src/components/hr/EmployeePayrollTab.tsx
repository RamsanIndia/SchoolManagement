import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { PayrollRecord, PayrollItem } from "@/types/navigation.types";
import { DollarSign, Eye, Download, TrendingUp, TrendingDown } from "lucide-react";

interface Props {
  employeeId: string;
}

export default function EmployeePayrollTab({ employeeId }: Props) {
  const [payrollRecords, setPayrollRecords] = useState<PayrollRecord[]>([]);
  const [selectedRecord, setSelectedRecord] = useState<PayrollRecord | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);

  useEffect(() => {
    // Mock data - replace with actual API call
    const mockPayroll: PayrollRecord[] = [
      {
        id: "1",
        employeeId,
        employeeName: "John Doe",
        department: "Mathematics",
        designation: "Senior Teacher",
        month: "January",
        year: 2024,
        baseSalary: 65000,
        allowances: [
          { id: "a1", allowanceDeductionId: "ad1", code: "HRA", name: "House Rent Allowance", type: "allowance", amount: 15000 },
          { id: "a2", allowanceDeductionId: "ad2", code: "TA", name: "Transport Allowance", type: "allowance", amount: 3000 },
          { id: "a3", allowanceDeductionId: "ad3", code: "MA", name: "Medical Allowance", type: "allowance", amount: 2000 },
        ],
        deductions: [
          { id: "d1", allowanceDeductionId: "ad4", code: "TAX", name: "Income Tax", type: "deduction", amount: 8500 },
          { id: "d2", allowanceDeductionId: "ad5", code: "PF", name: "Provident Fund", type: "deduction", amount: 6500 },
          { id: "d3", allowanceDeductionId: "ad6", code: "ESI", name: "Employee State Insurance", type: "deduction", amount: 1000 },
        ],
        grossSalary: 85000,
        totalAllowances: 20000,
        totalDeductions: 16000,
        netSalary: 69000,
        workingDays: 22,
        presentDays: 22,
        leaveDays: 0,
        overtimeHours: 4,
        overtimePay: 1000,
        status: "paid",
        paymentDate: "2024-01-31",
        paymentMethod: "bank-transfer",
        transactionId: "TXN123456",
        createdAt: "2024-01-28T10:00:00Z",
        updatedAt: "2024-01-31T15:00:00Z",
      },
    ];

    setPayrollRecords(mockPayroll);
  }, [employeeId]);

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      draft: "bg-muted",
      processed: "bg-blue-500",
      paid: "bg-status-present",
      hold: "bg-amber-500",
    };
    return <Badge className={colors[status] || "bg-muted"}>{status}</Badge>;
  };

  const handleViewDetails = (record: PayrollRecord) => {
    setSelectedRecord(record);
    setIsDetailOpen(true);
  };

  return (
    <div className="space-y-4">
      {/* Summary Cards */}
      {payrollRecords[0] && (
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Base Salary
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                ${payrollRecords[0].baseSalary.toLocaleString()}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <TrendingUp className="h-4 w-4" />
                Total Allowances
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-status-present">
                +${payrollRecords[0].totalAllowances.toLocaleString()}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <TrendingDown className="h-4 w-4" />
                Total Deductions
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-status-absent">
                -${payrollRecords[0].totalDeductions.toLocaleString()}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <DollarSign className="h-4 w-4" />
                Net Salary
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                ${payrollRecords[0].netSalary.toLocaleString()}
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Payroll Records Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="h-5 w-5" />
            Payroll History
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Period</TableHead>
                <TableHead>Base Salary</TableHead>
                <TableHead>Gross Salary</TableHead>
                <TableHead>Deductions</TableHead>
                <TableHead>Net Salary</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {payrollRecords.map((record) => (
                <TableRow key={record.id}>
                  <TableCell>{record.month} {record.year}</TableCell>
                  <TableCell>${record.baseSalary.toLocaleString()}</TableCell>
                  <TableCell>${record.grossSalary.toLocaleString()}</TableCell>
                  <TableCell className="text-status-absent">
                    -${record.totalDeductions.toLocaleString()}
                  </TableCell>
                  <TableCell className="font-bold">
                    ${record.netSalary.toLocaleString()}
                  </TableCell>
                  <TableCell>{getStatusBadge(record.status)}</TableCell>
                  <TableCell>
                    <div className="flex gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleViewDetails(record)}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button variant="outline" size="sm">
                        <Download className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Detailed Payslip Dialog */}
      <Dialog open={isDetailOpen} onOpenChange={setIsDetailOpen}>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Payslip - {selectedRecord?.month} {selectedRecord?.year}</DialogTitle>
          </DialogHeader>
          {selectedRecord && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-muted-foreground">Employee Name</p>
                  <p className="font-medium">{selectedRecord.employeeName}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Department</p>
                  <p className="font-medium">{selectedRecord.department}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Designation</p>
                  <p className="font-medium">{selectedRecord.designation}</p>
                </div>
                <div>
                  <p className="text-muted-foreground">Payment Date</p>
                  <p className="font-medium">
                    {selectedRecord.paymentDate ? new Date(selectedRecord.paymentDate).toLocaleDateString() : "-"}
                  </p>
                </div>
              </div>

              <div className="border-t pt-4">
                <h3 className="font-semibold mb-2">Earnings</h3>
                <Table>
                  <TableBody>
                    <TableRow>
                      <TableCell>Base Salary</TableCell>
                      <TableCell className="text-right">
                        ${selectedRecord.baseSalary.toLocaleString()}
                      </TableCell>
                    </TableRow>
                    {selectedRecord.allowances.map((allowance) => (
                      <TableRow key={allowance.id}>
                        <TableCell>{allowance.name}</TableCell>
                        <TableCell className="text-right">
                          ${allowance.amount.toLocaleString()}
                        </TableCell>
                      </TableRow>
                    ))}
                    {selectedRecord.overtimePay > 0 && (
                      <TableRow>
                        <TableCell>Overtime Pay ({selectedRecord.overtimeHours}h)</TableCell>
                        <TableCell className="text-right">
                          ${selectedRecord.overtimePay.toLocaleString()}
                        </TableCell>
                      </TableRow>
                    )}
                    <TableRow className="font-bold">
                      <TableCell>Gross Salary</TableCell>
                      <TableCell className="text-right">
                        ${selectedRecord.grossSalary.toLocaleString()}
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </div>

              <div className="border-t pt-4">
                <h3 className="font-semibold mb-2">Deductions</h3>
                <Table>
                  <TableBody>
                    {selectedRecord.deductions.map((deduction) => (
                      <TableRow key={deduction.id}>
                        <TableCell>{deduction.name}</TableCell>
                        <TableCell className="text-right text-status-absent">
                          -${deduction.amount.toLocaleString()}
                        </TableCell>
                      </TableRow>
                    ))}
                    <TableRow className="font-bold">
                      <TableCell>Total Deductions</TableCell>
                      <TableCell className="text-right text-status-absent">
                        -${selectedRecord.totalDeductions.toLocaleString()}
                      </TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </div>

              <div className="border-t pt-4">
                <div className="flex justify-between items-center text-lg font-bold">
                  <span>Net Salary</span>
                  <span>${selectedRecord.netSalary.toLocaleString()}</span>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
