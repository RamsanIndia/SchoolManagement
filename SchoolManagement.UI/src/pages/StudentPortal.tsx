import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  GraduationCap,
  Calendar,
  CreditCard,
  FileText,
  Download,
  BookOpen,
  Clock,
  CheckCircle,
  XCircle,
  AlertCircle,
  TrendingUp,
  Award,
  DollarSign,
  User,
  Printer,
} from "lucide-react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
} from "recharts";

// Mock student data
const studentInfo = {
  name: "Rahul Sharma",
  rollNo: "2024-A-001",
  class: "Class 10-A",
  section: "A",
  admissionNo: "ADM-2020-1234",
  dob: "2008-05-15",
  parentName: "Mr. Suresh Sharma",
  contact: "+91 98765 43210",
  email: "rahul.sharma@school.edu",
  photo: "/placeholder.svg",
};

const attendanceData = {
  totalDays: 180,
  present: 165,
  absent: 10,
  late: 5,
  percentage: 91.7,
};

const monthlyAttendance = [
  { month: "Apr", present: 22, absent: 1, late: 1 },
  { month: "May", present: 20, absent: 2, late: 0 },
  { month: "Jun", present: 18, absent: 1, late: 1 },
  { month: "Jul", present: 21, absent: 0, late: 1 },
  { month: "Aug", present: 22, absent: 2, late: 0 },
  { month: "Sep", present: 20, absent: 1, late: 1 },
  { month: "Oct", present: 21, absent: 2, late: 0 },
  { month: "Nov", present: 21, absent: 1, late: 1 },
];

const grades = [
  { subject: "Mathematics", marks: 92, total: 100, grade: "A+", teacher: "Mr. Rajesh Kumar" },
  { subject: "Science", marks: 88, total: 100, grade: "A", teacher: "Mrs. Anita Verma" },
  { subject: "English", marks: 85, total: 100, grade: "A", teacher: "Ms. Priya Sharma" },
  { subject: "Hindi", marks: 90, total: 100, grade: "A+", teacher: "Mr. Amit Joshi" },
  { subject: "Social Studies", marks: 82, total: 100, grade: "A", teacher: "Mrs. Kavita Reddy" },
  { subject: "Computer Science", marks: 95, total: 100, grade: "A+", teacher: "Mr. Vikram Singh" },
];

const performanceTrend = [
  { exam: "Unit Test 1", percentage: 85 },
  { exam: "Mid Term", percentage: 88 },
  { exam: "Unit Test 2", percentage: 90 },
  { exam: "Final Term", percentage: 89 },
];

const feeDetails = {
  totalFee: 85000,
  paidAmount: 65000,
  pendingAmount: 20000,
  dueDate: "2024-02-15",
  status: "Partial",
};

const feeHistory = [
  { id: "RCPT-2024-0001", date: "2024-04-15", amount: 25000, mode: "Online", status: "Paid" },
  { id: "RCPT-2024-0045", date: "2024-07-20", amount: 20000, mode: "Cheque", status: "Paid" },
  { id: "RCPT-2024-0089", date: "2024-10-10", amount: 20000, mode: "Cash", status: "Paid" },
];

const examSchedule = [
  { subject: "Mathematics", date: "2024-02-10", time: "09:00 AM", room: "Hall A" },
  { subject: "Science", date: "2024-02-12", time: "09:00 AM", room: "Hall B" },
  { subject: "English", date: "2024-02-14", time: "09:00 AM", room: "Hall A" },
  { subject: "Hindi", date: "2024-02-16", time: "09:00 AM", room: "Hall C" },
  { subject: "Social Studies", date: "2024-02-18", time: "09:00 AM", room: "Hall B" },
];

export default function StudentPortal() {
  const totalMarks = grades.reduce((sum, g) => sum + g.marks, 0);
  const totalPossible = grades.reduce((sum, g) => sum + g.total, 0);
  const overallPercentage = ((totalMarks / totalPossible) * 100).toFixed(1);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="w-20 h-20 rounded-full bg-muted flex items-center justify-center overflow-hidden">
            <User className="h-10 w-10 text-muted-foreground" />
          </div>
          <div>
            <h1 className="text-3xl font-bold">{studentInfo.name}</h1>
            <p className="text-muted-foreground">
              {studentInfo.class} | Roll No: {studentInfo.rollNo}
            </p>
            <p className="text-sm text-muted-foreground">
              Admission No: {studentInfo.admissionNo}
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Download Report Card
          </Button>
          <Button>
            <Printer className="h-4 w-4 mr-2" />
            Print Admit Card
          </Button>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Attendance</p>
                <p className="text-2xl font-bold text-green-400">{attendanceData.percentage}%</p>
                <p className="text-xs text-muted-foreground">{attendanceData.present}/{attendanceData.totalDays} days</p>
              </div>
              <Calendar className="h-8 w-8 text-green-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Overall Grade</p>
                <p className="text-2xl font-bold text-blue-400">{overallPercentage}%</p>
                <p className="text-xs text-muted-foreground">A+ Grade</p>
              </div>
              <Award className="h-8 w-8 text-blue-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Fee Status</p>
                <p className="text-2xl font-bold text-yellow-400">₹{(feeDetails.pendingAmount / 1000).toFixed(0)}K</p>
                <p className="text-xs text-muted-foreground">Pending</p>
              </div>
              <DollarSign className="h-8 w-8 text-yellow-400" />
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">Class Rank</p>
                <p className="text-2xl font-bold text-purple-400">#3</p>
                <p className="text-xs text-muted-foreground">Out of 45 students</p>
              </div>
              <TrendingUp className="h-8 w-8 text-purple-400" />
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="grades" className="space-y-4">
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="grades">Grades</TabsTrigger>
          <TabsTrigger value="attendance">Attendance</TabsTrigger>
          <TabsTrigger value="fees">Fees</TabsTrigger>
          <TabsTrigger value="reportcard">Report Card</TabsTrigger>
          <TabsTrigger value="admitcard">Admit Card</TabsTrigger>
        </TabsList>

        {/* Grades Tab */}
        <TabsContent value="grades" className="space-y-4">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <Card className="bg-card border-border">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <BookOpen className="h-5 w-5" />
                  Subject-wise Performance
                </CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Subject</TableHead>
                      <TableHead>Marks</TableHead>
                      <TableHead>Grade</TableHead>
                      <TableHead>Teacher</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {grades.map((grade) => (
                      <TableRow key={grade.subject}>
                        <TableCell className="font-medium">{grade.subject}</TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <span>{grade.marks}/{grade.total}</span>
                            <Progress value={grade.marks} className="w-16 h-2" />
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge className={
                            grade.grade === "A+" ? "bg-green-500/20 text-green-400" :
                            grade.grade === "A" ? "bg-blue-500/20 text-blue-400" :
                            "bg-yellow-500/20 text-yellow-400"
                          }>
                            {grade.grade}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-sm text-muted-foreground">
                          {grade.teacher}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            <Card className="bg-card border-border">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <TrendingUp className="h-5 w-5" />
                  Performance Trend
                </CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={250}>
                  <LineChart data={performanceTrend}>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                    <XAxis dataKey="exam" stroke="hsl(var(--muted-foreground))" fontSize={12} />
                    <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} domain={[70, 100]} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "hsl(var(--card))",
                        border: "1px solid hsl(var(--border))",
                        borderRadius: "8px",
                      }}
                    />
                    <Line
                      type="monotone"
                      dataKey="percentage"
                      stroke="hsl(var(--primary))"
                      strokeWidth={2}
                      dot={{ fill: "hsl(var(--primary))", strokeWidth: 2 }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Attendance Tab */}
        <TabsContent value="attendance" className="space-y-4">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
            <Card className="bg-card border-border">
              <CardHeader>
                <CardTitle>Attendance Summary</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <CheckCircle className="h-5 w-5 text-green-400" />
                    <span>Present</span>
                  </div>
                  <span className="font-bold text-green-400">{attendanceData.present} days</span>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <XCircle className="h-5 w-5 text-red-400" />
                    <span>Absent</span>
                  </div>
                  <span className="font-bold text-red-400">{attendanceData.absent} days</span>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <AlertCircle className="h-5 w-5 text-yellow-400" />
                    <span>Late</span>
                  </div>
                  <span className="font-bold text-yellow-400">{attendanceData.late} days</span>
                </div>
                <div className="pt-4 border-t">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-muted-foreground">Overall Attendance</span>
                    <span className="font-bold">{attendanceData.percentage}%</span>
                  </div>
                  <Progress value={attendanceData.percentage} className="h-3" />
                </div>
              </CardContent>
            </Card>

            <Card className="lg:col-span-2 bg-card border-border">
              <CardHeader>
                <CardTitle>Monthly Attendance</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={250}>
                  <BarChart data={monthlyAttendance}>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                    <XAxis dataKey="month" stroke="hsl(var(--muted-foreground))" fontSize={12} />
                    <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "hsl(var(--card))",
                        border: "1px solid hsl(var(--border))",
                        borderRadius: "8px",
                      }}
                    />
                    <Bar dataKey="present" fill="hsl(142 76% 36%)" name="Present" />
                    <Bar dataKey="absent" fill="hsl(0 84% 60%)" name="Absent" />
                    <Bar dataKey="late" fill="hsl(45 93% 47%)" name="Late" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Fees Tab */}
        <TabsContent value="fees" className="space-y-4">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
            <Card className="bg-card border-border">
              <CardHeader>
                <CardTitle>Fee Summary</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Total Fee</span>
                  <span className="font-bold">₹{feeDetails.totalFee.toLocaleString()}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Paid Amount</span>
                  <span className="font-bold text-green-400">₹{feeDetails.paidAmount.toLocaleString()}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Pending Amount</span>
                  <span className="font-bold text-red-400">₹{feeDetails.pendingAmount.toLocaleString()}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground">Due Date</span>
                  <span className="font-bold">{feeDetails.dueDate}</span>
                </div>
                <div className="pt-4 border-t">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-muted-foreground">Payment Progress</span>
                    <span className="font-bold">{((feeDetails.paidAmount / feeDetails.totalFee) * 100).toFixed(0)}%</span>
                  </div>
                  <Progress value={(feeDetails.paidAmount / feeDetails.totalFee) * 100} className="h-3" />
                </div>
                <Button className="w-full mt-4">
                  <CreditCard className="h-4 w-4 mr-2" />
                  Pay Now
                </Button>
              </CardContent>
            </Card>

            <Card className="lg:col-span-2 bg-card border-border">
              <CardHeader>
                <CardTitle>Payment History</CardTitle>
              </CardHeader>
              <CardContent>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Receipt No</TableHead>
                      <TableHead>Date</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead>Mode</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Action</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {feeHistory.map((fee) => (
                      <TableRow key={fee.id}>
                        <TableCell className="font-medium">{fee.id}</TableCell>
                        <TableCell>{fee.date}</TableCell>
                        <TableCell>₹{fee.amount.toLocaleString()}</TableCell>
                        <TableCell>{fee.mode}</TableCell>
                        <TableCell>
                          <Badge className="bg-green-500/20 text-green-400">{fee.status}</Badge>
                        </TableCell>
                        <TableCell>
                          <Button size="sm" variant="ghost">
                            <Download className="h-4 w-4" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Report Card Tab */}
        <TabsContent value="reportcard" className="space-y-4">
          <Card className="bg-card border-border">
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Report Card - Final Term 2024
              </CardTitle>
              <Button variant="outline">
                <Download className="h-4 w-4 mr-2" />
                Download PDF
              </Button>
            </CardHeader>
            <CardContent>
              <div className="border rounded-lg p-6 space-y-6">
                {/* Student Info */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pb-4 border-b">
                  <div>
                    <p className="text-sm text-muted-foreground">Student Name</p>
                    <p className="font-medium">{studentInfo.name}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Class</p>
                    <p className="font-medium">{studentInfo.class}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Roll Number</p>
                    <p className="font-medium">{studentInfo.rollNo}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Academic Year</p>
                    <p className="font-medium">2023-24</p>
                  </div>
                </div>

                {/* Marks Table */}
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Subject</TableHead>
                      <TableHead className="text-center">Max Marks</TableHead>
                      <TableHead className="text-center">Marks Obtained</TableHead>
                      <TableHead className="text-center">Grade</TableHead>
                      <TableHead>Remarks</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {grades.map((grade) => (
                      <TableRow key={grade.subject}>
                        <TableCell className="font-medium">{grade.subject}</TableCell>
                        <TableCell className="text-center">{grade.total}</TableCell>
                        <TableCell className="text-center">{grade.marks}</TableCell>
                        <TableCell className="text-center">
                          <Badge className={
                            grade.grade === "A+" ? "bg-green-500/20 text-green-400" :
                            "bg-blue-500/20 text-blue-400"
                          }>
                            {grade.grade}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-muted-foreground">Excellent</TableCell>
                      </TableRow>
                    ))}
                    <TableRow className="font-bold bg-muted/50">
                      <TableCell>Total</TableCell>
                      <TableCell className="text-center">{totalPossible}</TableCell>
                      <TableCell className="text-center">{totalMarks}</TableCell>
                      <TableCell className="text-center">
                        <Badge className="bg-green-500/20 text-green-400">A+</Badge>
                      </TableCell>
                      <TableCell>Outstanding</TableCell>
                    </TableRow>
                  </TableBody>
                </Table>

                {/* Summary */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pt-4 border-t">
                  <div className="text-center p-4 bg-muted/50 rounded-lg">
                    <p className="text-sm text-muted-foreground">Percentage</p>
                    <p className="text-2xl font-bold text-primary">{overallPercentage}%</p>
                  </div>
                  <div className="text-center p-4 bg-muted/50 rounded-lg">
                    <p className="text-sm text-muted-foreground">Grade</p>
                    <p className="text-2xl font-bold text-green-400">A+</p>
                  </div>
                  <div className="text-center p-4 bg-muted/50 rounded-lg">
                    <p className="text-sm text-muted-foreground">Class Rank</p>
                    <p className="text-2xl font-bold text-blue-400">3rd</p>
                  </div>
                  <div className="text-center p-4 bg-muted/50 rounded-lg">
                    <p className="text-sm text-muted-foreground">Result</p>
                    <p className="text-2xl font-bold text-green-400">PASS</p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Admit Card Tab */}
        <TabsContent value="admitcard" className="space-y-4">
          <Card className="bg-card border-border">
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="flex items-center gap-2">
                <CreditCard className="h-5 w-5" />
                Admit Card - Final Term Examination 2024
              </CardTitle>
              <Button>
                <Printer className="h-4 w-4 mr-2" />
                Print Admit Card
              </Button>
            </CardHeader>
            <CardContent>
              <div className="border rounded-lg p-6 space-y-6 max-w-2xl mx-auto">
                {/* Header */}
                <div className="text-center pb-4 border-b">
                  <div className="flex items-center justify-center gap-3 mb-2">
                    <GraduationCap className="h-8 w-8 text-primary" />
                    <h2 className="text-2xl font-bold">EduManage School</h2>
                  </div>
                  <p className="text-muted-foreground">Final Term Examination 2024</p>
                </div>

                {/* Student Info */}
                <div className="flex items-start gap-6">
                  <div className="w-24 h-28 bg-muted rounded-lg flex items-center justify-center">
                    <User className="h-12 w-12 text-muted-foreground" />
                  </div>
                  <div className="flex-1 grid grid-cols-2 gap-4">
                    <div>
                      <p className="text-sm text-muted-foreground">Student Name</p>
                      <p className="font-medium">{studentInfo.name}</p>
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground">Roll Number</p>
                      <p className="font-medium">{studentInfo.rollNo}</p>
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground">Class & Section</p>
                      <p className="font-medium">{studentInfo.class}</p>
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground">Admission No</p>
                      <p className="font-medium">{studentInfo.admissionNo}</p>
                    </div>
                  </div>
                </div>

                {/* Exam Schedule */}
                <div>
                  <h3 className="font-semibold mb-3">Examination Schedule</h3>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Subject</TableHead>
                        <TableHead>Date</TableHead>
                        <TableHead>Time</TableHead>
                        <TableHead>Room</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {examSchedule.map((exam) => (
                        <TableRow key={exam.subject}>
                          <TableCell className="font-medium">{exam.subject}</TableCell>
                          <TableCell>{exam.date}</TableCell>
                          <TableCell>{exam.time}</TableCell>
                          <TableCell>{exam.room}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>

                {/* Instructions */}
                <div className="bg-muted/50 p-4 rounded-lg">
                  <h3 className="font-semibold mb-2">Instructions</h3>
                  <ul className="text-sm text-muted-foreground space-y-1">
                    <li>• Students must carry this admit card to the examination hall.</li>
                    <li>• Students should report 30 minutes before the exam starts.</li>
                    <li>• Electronic devices are not allowed in the examination hall.</li>
                    <li>• Students must bring their own stationery.</li>
                  </ul>
                </div>

                {/* Signatures */}
                <div className="flex justify-between pt-4 border-t">
                  <div className="text-center">
                    <div className="w-32 border-b border-dashed mb-2"></div>
                    <p className="text-sm text-muted-foreground">Class Teacher</p>
                  </div>
                  <div className="text-center">
                    <div className="w-32 border-b border-dashed mb-2"></div>
                    <p className="text-sm text-muted-foreground">Principal</p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}