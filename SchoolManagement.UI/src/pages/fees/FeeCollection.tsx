import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Search, DollarSign, AlertCircle, CheckCircle } from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface PendingFee {
  head: string;
  amount: number;
  lateFee: number;
  dueDate: string;
}

interface Student {
  name: string;
  class: string;
  admissionNo: string;
  pendingFees: PendingFee[];
}

const mockStudent: Student = {
  name: "Amit Kumar",
  class: "10 - A",
  admissionNo: "S12345",
  pendingFees: [
    { head: "Tuition Fee", amount: 1500, lateFee: 50, dueDate: "2025-02-10" },
    { head: "Computer Fee", amount: 300, lateFee: 0, dueDate: "2025-02-10" },
    { head: "Sports Fee", amount: 500, lateFee: 25, dueDate: "2025-01-15" },
  ],
};

export default function FeeCollection() {
  const [searchTerm, setSearchTerm] = useState("");
  const [studentData, setStudentData] = useState<Student | null>(null);
  const [selectedFees, setSelectedFees] = useState<number[]>([]);
  const [paymentMode, setPaymentMode] = useState("cash");
  const [discount, setDiscount] = useState("0");
  const [chequeNo, setChequeNo] = useState("");
  const [bankName, setBankName] = useState("");
  const [isPaymentDialogOpen, setIsPaymentDialogOpen] = useState(false);
  const [isReceiptDialogOpen, setIsReceiptDialogOpen] = useState(false);
  const [receiptNo, setReceiptNo] = useState("");

  const handleSearch = () => {
    if (searchTerm.toLowerCase().includes("s12345") || searchTerm.toLowerCase().includes("amit")) {
      setStudentData(mockStudent);
      setSelectedFees([0, 1, 2]); // Select all by default
    } else {
      toast({ title: "Not Found", description: "Student not found", variant: "destructive" });
    }
  };

  const toggleFeeSelection = (index: number) => {
    if (selectedFees.includes(index)) {
      setSelectedFees(selectedFees.filter((i) => i !== index));
    } else {
      setSelectedFees([...selectedFees, index]);
    }
  };

  const calculateTotal = () => {
    if (!studentData) return { subtotal: 0, lateFee: 0, discount: 0, total: 0 };

    const subtotal = selectedFees.reduce(
      (sum, index) => sum + studentData.pendingFees[index].amount,
      0
    );
    const lateFee = selectedFees.reduce(
      (sum, index) => sum + studentData.pendingFees[index].lateFee,
      0
    );
    const discountAmount = Number(discount) || 0;
    const total = subtotal + lateFee - discountAmount;

    return { subtotal, lateFee, discount: discountAmount, total };
  };

  const handlePayment = () => {
    if (selectedFees.length === 0) {
      toast({ title: "Error", description: "Please select at least one fee", variant: "destructive" });
      return;
    }

    if (paymentMode === "cheque" && (!chequeNo || !bankName)) {
      toast({
        title: "Error",
        description: "Please enter cheque details",
        variant: "destructive",
      });
      return;
    }

    const newReceiptNo = `RCPT-2025-${String(Math.floor(Math.random() * 10000)).padStart(5, "0")}`;
    setReceiptNo(newReceiptNo);
    setIsPaymentDialogOpen(false);
    setIsReceiptDialogOpen(true);
  };

  const totals = calculateTotal();

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Fee Collection</h1>
        <p className="text-muted-foreground">Collect fees from students</p>
      </div>

      {/* Search Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex gap-2">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by Admission No, Name, or Phone..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                className="pl-10"
              />
            </div>
            <Button onClick={handleSearch}>Search</Button>
          </div>
        </CardContent>
      </Card>

      {studentData && (
        <>
          {/* Student Card */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <Avatar className="h-16 w-16">
                  <AvatarFallback className="text-xl">
                    {studentData.name
                      .split(" ")
                      .map((n) => n[0])
                      .join("")}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1">
                  <h2 className="text-xl font-bold">{studentData.name}</h2>
                  <div className="flex gap-6 mt-2 text-sm text-muted-foreground">
                    <span>Class: {studentData.class}</span>
                    <span>Admission No: {studentData.admissionNo}</span>
                  </div>
                </div>
                <Badge variant="destructive" className="text-lg px-4 py-2">
                  Due: ₹{totals.total.toLocaleString()}
                </Badge>
              </div>
            </CardContent>
          </Card>

          {/* Pending Fees */}
          <Card>
            <CardHeader>
              <CardTitle>Pending Fees</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12"></TableHead>
                    <TableHead>Fee Head</TableHead>
                    <TableHead>Amount</TableHead>
                    <TableHead>Late Fee</TableHead>
                    <TableHead>Due Date</TableHead>
                    <TableHead className="text-right">Total</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {studentData.pendingFees.map((fee, index) => (
                    <TableRow key={index} className="cursor-pointer hover:bg-muted/50">
                      <TableCell>
                        <input
                          type="checkbox"
                          checked={selectedFees.includes(index)}
                          onChange={() => toggleFeeSelection(index)}
                          className="h-4 w-4"
                        />
                      </TableCell>
                      <TableCell className="font-medium">{fee.head}</TableCell>
                      <TableCell>₹{fee.amount}</TableCell>
                      <TableCell>
                        {fee.lateFee > 0 ? (
                          <span className="text-destructive">₹{fee.lateFee}</span>
                        ) : (
                          "-"
                        )}
                      </TableCell>
                      <TableCell>{fee.dueDate}</TableCell>
                      <TableCell className="text-right font-medium">
                        ₹{fee.amount + fee.lateFee}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              <div className="mt-6 space-y-2 border-t pt-4">
                <div className="flex justify-between text-sm">
                  <span>Subtotal:</span>
                  <span>₹{totals.subtotal.toLocaleString()}</span>
                </div>
                <div className="flex justify-between text-sm text-destructive">
                  <span>Late Fee:</span>
                  <span>₹{totals.lateFee.toLocaleString()}</span>
                </div>
                <div className="flex justify-between items-center text-sm">
                  <span>Discount:</span>
                  <Input
                    type="number"
                    value={discount}
                    onChange={(e) => setDiscount(e.target.value)}
                    className="w-32 h-8 text-right"
                    placeholder="0"
                  />
                </div>
                <div className="flex justify-between text-lg font-bold border-t pt-2">
                  <span>Total Amount:</span>
                  <span>₹{totals.total.toLocaleString()}</span>
                </div>
              </div>

              <Button
                onClick={() => setIsPaymentDialogOpen(true)}
                size="lg"
                className="w-full mt-6"
                disabled={selectedFees.length === 0}
              >
                <DollarSign className="mr-2 h-5 w-5" />
                Proceed to Payment
              </Button>
            </CardContent>
          </Card>
        </>
      )}

      {/* Payment Dialog */}
      <Dialog open={isPaymentDialogOpen} onOpenChange={setIsPaymentDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Payment Details</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Payment Mode</Label>
              <RadioGroup value={paymentMode} onValueChange={setPaymentMode}>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="cash" id="cash" />
                  <Label htmlFor="cash" className="font-normal cursor-pointer">
                    Cash
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="upi" id="upi" />
                  <Label htmlFor="upi" className="font-normal cursor-pointer">
                    UPI
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="netbanking" id="netbanking" />
                  <Label htmlFor="netbanking" className="font-normal cursor-pointer">
                    Net Banking
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="cheque" id="cheque" />
                  <Label htmlFor="cheque" className="font-normal cursor-pointer">
                    Cheque
                  </Label>
                </div>
              </RadioGroup>
            </div>

            {paymentMode === "cheque" && (
              <>
                <div className="space-y-2">
                  <Label>Cheque Number</Label>
                  <Input
                    value={chequeNo}
                    onChange={(e) => setChequeNo(e.target.value)}
                    placeholder="Enter cheque number"
                  />
                </div>
                <div className="space-y-2">
                  <Label>Bank Name</Label>
                  <Input
                    value={bankName}
                    onChange={(e) => setBankName(e.target.value)}
                    placeholder="Enter bank name"
                  />
                </div>
              </>
            )}

            <div className="bg-muted p-4 rounded-lg space-y-2">
              <div className="flex justify-between text-sm">
                <span>Amount to Pay:</span>
                <span className="font-bold">₹{totals.total.toLocaleString()}</span>
              </div>
            </div>

            <Button onClick={handlePayment} className="w-full" size="lg">
              Confirm Payment
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* Receipt Dialog */}
      <Dialog open={isReceiptDialogOpen} onOpenChange={setIsReceiptDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-green-600" />
              Payment Successful
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="text-center p-6 bg-muted rounded-lg">
              <p className="text-sm text-muted-foreground mb-2">Receipt Number</p>
              <p className="text-2xl font-bold font-mono">{receiptNo}</p>
            </div>

            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span>Student:</span>
                <span className="font-medium">{studentData?.name}</span>
              </div>
              <div className="flex justify-between">
                <span>Class:</span>
                <span className="font-medium">{studentData?.class}</span>
              </div>
              <div className="flex justify-between">
                <span>Payment Mode:</span>
                <span className="font-medium capitalize">{paymentMode}</span>
              </div>
              <div className="flex justify-between">
                <span>Date:</span>
                <span className="font-medium">{new Date().toLocaleDateString()}</span>
              </div>
              <div className="flex justify-between pt-2 border-t">
                <span>Amount Paid:</span>
                <span className="font-bold text-lg">₹{totals.total.toLocaleString()}</span>
              </div>
            </div>

            <Button
              onClick={() => {
                setIsReceiptDialogOpen(false);
                setStudentData(null);
                setSearchTerm("");
                toast({ title: "Success", description: "Payment recorded successfully" });
              }}
              className="w-full"
            >
              Done
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
