import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { CalendarIcon, Download, FileText, TrendingUp } from "lucide-react";
import { format } from "date-fns";
import { cn } from "@/lib/utils";

interface DailyCollection {
  receipt: string;
  name: string;
  class: string;
  amount: number;
  date: string;
  mode: string;
}

interface ClassWiseCollection {
  class: string;
  totalStudents: number;
  collected: number;
  pending: number;
  collectionRate: number;
}

const mockDailyCollection: DailyCollection[] = [
  { receipt: "RCPT-1201", name: "Anuj Sharma", class: "10-A", amount: 1500, date: "2025-03-01", mode: "Cash" },
  { receipt: "RCPT-1202", name: "Sanya Gupta", class: "8-B", amount: 1800, date: "2025-03-01", mode: "UPI" },
  { receipt: "RCPT-1203", name: "Rahul Kumar", class: "9-A", amount: 1500, date: "2025-03-01", mode: "Net Banking" },
  { receipt: "RCPT-1204", name: "Priya Singh", class: "7-C", amount: 1200, date: "2025-03-01", mode: "Cash" },
  { receipt: "RCPT-1205", name: "Vikram Patel", class: "10-B", amount: 2000, date: "2025-03-01", mode: "UPI" },
];

const mockClassWiseCollection: ClassWiseCollection[] = [
  { class: "1", totalStudents: 120, collected: 100000, pending: 20000, collectionRate: 83 },
  { class: "2", totalStudents: 115, collected: 95000, pending: 25000, collectionRate: 79 },
  { class: "3", totalStudents: 125, collected: 110000, pending: 15000, collectionRate: 88 },
  { class: "4", totalStudents: 118, collected: 105000, pending: 18000, collectionRate: 85 },
  { class: "5", totalStudents: 130, collected: 115000, pending: 22000, collectionRate: 84 },
  { class: "6", totalStudents: 122, collected: 108000, pending: 20000, collectionRate: 84 },
  { class: "7", totalStudents: 128, collected: 112000, pending: 19000, collectionRate: 85 },
  { class: "8", totalStudents: 135, collected: 120000, pending: 25000, collectionRate: 83 },
];

export default function FeeReports() {
  const [date, setDate] = useState<Date>(new Date());
  const [reportType, setReportType] = useState("daily");

  const dailyTotal = mockDailyCollection.reduce((sum, item) => sum + item.amount, 0);

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Fee Reports</h1>
          <p className="text-muted-foreground">Generate and view various fee collection reports</p>
        </div>
        <Button>
          <Download className="mr-2 h-4 w-4" />
          Export Report
        </Button>
      </div>

      <Tabs defaultValue="daily" className="space-y-6">
        <TabsList className="grid w-full max-w-md grid-cols-3">
          <TabsTrigger value="daily">Daily</TabsTrigger>
          <TabsTrigger value="monthly">Monthly</TabsTrigger>
          <TabsTrigger value="classwise">Class-wise</TabsTrigger>
        </TabsList>

        {/* Daily Collection Report */}
        <TabsContent value="daily" className="space-y-4">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <div className="flex-1">
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className={cn(
                          "w-full justify-start text-left font-normal",
                          !date && "text-muted-foreground"
                        )}
                      >
                        <CalendarIcon className="mr-2 h-4 w-4" />
                        {date ? format(date, "PPP") : "Pick a date"}
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-auto p-0">
                      <Calendar
                        mode="single"
                        selected={date}
                        onSelect={(newDate) => newDate && setDate(newDate)}
                        initialFocus
                        className="pointer-events-auto"
                      />
                    </PopoverContent>
                  </Popover>
                </div>
                <Button>Generate Report</Button>
              </div>
            </CardContent>
          </Card>

          <div className="grid gap-4 md:grid-cols-3">
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Total Collection
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">₹{dailyTotal.toLocaleString()}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Transactions
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{mockDailyCollection.length}</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Average Amount
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  ₹{Math.round(dailyTotal / mockDailyCollection.length).toLocaleString()}
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Daily Collection Details</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Receipt No</TableHead>
                    <TableHead>Student Name</TableHead>
                    <TableHead>Class</TableHead>
                    <TableHead>Amount</TableHead>
                    <TableHead>Payment Mode</TableHead>
                    <TableHead>Date</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mockDailyCollection.map((record) => (
                    <TableRow key={record.receipt}>
                      <TableCell className="font-mono text-sm">{record.receipt}</TableCell>
                      <TableCell className="font-medium">{record.name}</TableCell>
                      <TableCell>{record.class}</TableCell>
                      <TableCell className="font-bold">₹{record.amount.toLocaleString()}</TableCell>
                      <TableCell>
                        <Badge variant="outline">{record.mode}</Badge>
                      </TableCell>
                      <TableCell>{record.date}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Monthly Collection Report */}
        <TabsContent value="monthly" className="space-y-4">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <Select defaultValue="march-2025">
                  <SelectTrigger className="flex-1">
                    <SelectValue placeholder="Select month" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="march-2025">March 2025</SelectItem>
                    <SelectItem value="february-2025">February 2025</SelectItem>
                    <SelectItem value="january-2025">January 2025</SelectItem>
                  </SelectContent>
                </Select>
                <Button>Generate Report</Button>
              </div>
            </CardContent>
          </Card>

          <div className="grid gap-4 md:grid-cols-4">
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Total Collection
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">₹5,20,000</div>
                <p className="text-xs text-green-600 mt-1">+8% from last month</p>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Total Transactions
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">342</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Outstanding
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold text-destructive">₹1,42,000</div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  Collection Rate
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">78.5%</div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Class-wise Collection Report */}
        <TabsContent value="classwise" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <TrendingUp className="h-5 w-5" />
                Class-wise Fee Collection Overview
              </CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Class</TableHead>
                    <TableHead>Total Students</TableHead>
                    <TableHead>Amount Collected</TableHead>
                    <TableHead>Pending Amount</TableHead>
                    <TableHead>Collection Rate</TableHead>
                    <TableHead className="text-right">Status</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mockClassWiseCollection.map((record) => (
                    <TableRow key={record.class}>
                      <TableCell className="font-bold">Class {record.class}</TableCell>
                      <TableCell>{record.totalStudents}</TableCell>
                      <TableCell className="font-medium text-green-600">
                        ₹{record.collected.toLocaleString()}
                      </TableCell>
                      <TableCell className="text-destructive">
                        ₹{record.pending.toLocaleString()}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <div className="w-32 h-2 bg-muted rounded-full overflow-hidden">
                            <div
                              className="h-full bg-primary"
                              style={{ width: `${record.collectionRate}%` }}
                            />
                          </div>
                          <span className="text-sm font-medium">{record.collectionRate}%</span>
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        {record.collectionRate >= 85 ? (
                          <Badge className="bg-green-500">Excellent</Badge>
                        ) : record.collectionRate >= 80 ? (
                          <Badge>Good</Badge>
                        ) : (
                          <Badge variant="destructive">Needs Attention</Badge>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
