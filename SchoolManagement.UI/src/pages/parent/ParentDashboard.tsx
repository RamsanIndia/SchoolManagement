import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  User,
  Calendar,
  BookOpen,
  CreditCard,
  Bell,
  FileText,
  Clock,
  MessageSquare,
  Download,
  GraduationCap,
  CheckCircle,
  AlertTriangle,
  TrendingUp,
} from "lucide-react";

const studentData = {
  name: "Rohan Sharma",
  class: "5-A",
  rollNo: "12",
  photo: "",
  admissionNo: "ADM001",
};

const attendanceData = {
  present: 165,
  absent: 10,
  late: 5,
  total: 180,
  percentage: 91.67,
  monthlyData: [
    { month: "Aug", percentage: 95 },
    { month: "Sep", percentage: 90 },
    { month: "Oct", percentage: 88 },
    { month: "Nov", percentage: 93 },
  ],
};

const assignments = [
  { id: "1", title: "Chapter 2 Exercises", subject: "Math", dueDate: "Dec 12", status: "pending" },
  { id: "2", title: "Poem Summary", subject: "English", dueDate: "Dec 15", status: "submitted" },
  { id: "3", title: "Science Project", subject: "Science", dueDate: "Dec 20", status: "pending" },
];

const feeData = {
  totalFee: 85000,
  paid: 60000,
  pending: 25000,
  nextDueDate: "2024-12-31",
  lastPayment: "2024-10-15",
};

const notices = [
  { id: "1", title: "Parent-Teacher Meeting", date: "Dec 15", type: "event" },
  { id: "2", title: "Winter Vacation Schedule", date: "Dec 10", type: "notice" },
  { id: "3", title: "Annual Day Celebration", date: "Dec 20", type: "event" },
];

const examSchedule = [
  { subject: "Mathematics", date: "Jan 10", time: "9:00 AM" },
  { subject: "English", date: "Jan 12", time: "9:00 AM" },
  { subject: "Science", date: "Jan 14", time: "9:00 AM" },
];

const recentGrades = [
  { subject: "Mathematics", exam: "Unit Test 3", marks: 45, total: 50, grade: "A" },
  { subject: "English", exam: "Unit Test 3", marks: 42, total: 50, grade: "A" },
  { subject: "Science", exam: "Unit Test 3", marks: 48, total: 50, grade: "A+" },
];

export default function ParentDashboard() {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState("overview");

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Parent Portal</h1>
          <p className="text-muted-foreground">Welcome back! Here's an overview of your child's progress.</p>
        </div>
        <Button onClick={() => navigate("/parent/chat")}>
          <MessageSquare className="h-4 w-4 mr-2" />
          Message Teacher
        </Button>
      </div>

      {/* Student Card */}
      <Card className="bg-gradient-to-r from-primary/10 to-primary/5">
        <CardContent className="pt-6">
          <div className="flex items-center gap-6">
            <Avatar className="h-20 w-20">
              <AvatarImage src={studentData.photo} />
              <AvatarFallback className="text-2xl bg-primary text-primary-foreground">
                {studentData.name.split(" ").map((n) => n[0]).join("")}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h2 className="text-2xl font-bold">{studentData.name}</h2>
              <div className="flex gap-4 text-muted-foreground mt-1">
                <span>Class {studentData.class}</span>
                <span>•</span>
                <span>Roll No: {studentData.rollNo}</span>
                <span>•</span>
                <span>Admission No: {studentData.admissionNo}</span>
              </div>
            </div>
            <Button variant="outline" onClick={() => navigate(`/students/profile/${studentData.admissionNo}`)}>
              <User className="h-4 w-4 mr-2" />
              View Full Profile
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Quick Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-green-500/10">
                <CheckCircle className="h-6 w-6 text-green-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Attendance</p>
                <p className="text-2xl font-bold">{attendanceData.percentage}%</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-blue-500/10">
                <TrendingUp className="h-6 w-6 text-blue-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Overall Grade</p>
                <p className="text-2xl font-bold">A</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-orange-500/10">
                <BookOpen className="h-6 w-6 text-orange-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Pending Tasks</p>
                <p className="text-2xl font-bold">2</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-red-500/10">
                <CreditCard className="h-6 w-6 text-red-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Fee Due</p>
                <p className="text-2xl font-bold">₹{feeData.pending.toLocaleString()}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Main Content Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="attendance">Attendance</TabsTrigger>
          <TabsTrigger value="assignments">Assignments</TabsTrigger>
          <TabsTrigger value="fees">Fees</TabsTrigger>
          <TabsTrigger value="downloads">Downloads</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid md:grid-cols-2 gap-4">
            {/* Recent Grades */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <GraduationCap className="h-5 w-5" />
                  Recent Grades
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {recentGrades.map((grade, idx) => (
                    <div key={idx} className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                      <div>
                        <p className="font-medium">{grade.subject}</p>
                        <p className="text-sm text-muted-foreground">{grade.exam}</p>
                      </div>
                      <div className="text-right">
                        <p className="font-bold">{grade.marks}/{grade.total}</p>
                        <Badge variant="outline">{grade.grade}</Badge>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Upcoming Exams */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Calendar className="h-5 w-5" />
                  Exam Schedule
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {examSchedule.map((exam, idx) => (
                    <div key={idx} className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                      <div className="flex items-center gap-3">
                        <div className="p-2 rounded-lg bg-primary/10">
                          <BookOpen className="h-4 w-4 text-primary" />
                        </div>
                        <div>
                          <p className="font-medium">{exam.subject}</p>
                          <p className="text-sm text-muted-foreground">{exam.time}</p>
                        </div>
                      </div>
                      <Badge>{exam.date}</Badge>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Notices */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Bell className="h-5 w-5" />
                  Notices & Circulars
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {notices.map((notice) => (
                    <div key={notice.id} className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
                      <div className="flex items-center gap-3">
                        <div className={`p-2 rounded-lg ${notice.type === "event" ? "bg-blue-500/10" : "bg-orange-500/10"}`}>
                          {notice.type === "event" ? (
                            <Calendar className="h-4 w-4 text-blue-500" />
                          ) : (
                            <FileText className="h-4 w-4 text-orange-500" />
                          )}
                        </div>
                        <div>
                          <p className="font-medium">{notice.title}</p>
                          <p className="text-sm text-muted-foreground">{notice.date}</p>
                        </div>
                      </div>
                      <Button variant="ghost" size="sm">View</Button>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Leave Apply */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Clock className="h-5 w-5" />
                  Leave Application
                </CardTitle>
                <CardDescription>Apply for leave or view past applications</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <Button className="w-full" onClick={() => navigate("/parent/leave-apply")}>
                    Apply for Leave
                  </Button>
                  <div className="p-4 border rounded-lg">
                    <div className="flex justify-between items-center">
                      <div>
                        <p className="font-medium">Last Application</p>
                        <p className="text-sm text-muted-foreground">Dec 02-04, 2024 - Fever</p>
                      </div>
                      <Badge className="bg-orange-500/10 text-orange-600">Pending</Badge>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="attendance">
          <Card>
            <CardHeader>
              <CardTitle>Attendance Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-4 gap-4">
                <div className="text-center p-4 rounded-lg bg-muted/50">
                  <p className="text-3xl font-bold">{attendanceData.total}</p>
                  <p className="text-sm text-muted-foreground">Total Days</p>
                </div>
                <div className="text-center p-4 rounded-lg bg-green-500/10">
                  <p className="text-3xl font-bold text-green-600">{attendanceData.present}</p>
                  <p className="text-sm text-muted-foreground">Present</p>
                </div>
                <div className="text-center p-4 rounded-lg bg-red-500/10">
                  <p className="text-3xl font-bold text-red-600">{attendanceData.absent}</p>
                  <p className="text-sm text-muted-foreground">Absent</p>
                </div>
                <div className="text-center p-4 rounded-lg bg-orange-500/10">
                  <p className="text-3xl font-bold text-orange-600">{attendanceData.late}</p>
                  <p className="text-sm text-muted-foreground">Late</p>
                </div>
              </div>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span>Overall Attendance</span>
                  <span className="font-bold">{attendanceData.percentage}%</span>
                </div>
                <Progress value={attendanceData.percentage} className="h-3" />
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="assignments">
          <Card>
            <CardHeader>
              <CardTitle>Homework & Assignments</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {assignments.map((assignment) => (
                  <div key={assignment.id} className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-4">
                      <div className={`p-3 rounded-lg ${assignment.status === "submitted" ? "bg-green-500/10" : "bg-orange-500/10"}`}>
                        <BookOpen className={`h-5 w-5 ${assignment.status === "submitted" ? "text-green-500" : "text-orange-500"}`} />
                      </div>
                      <div>
                        <p className="font-medium">{assignment.title}</p>
                        <p className="text-sm text-muted-foreground">{assignment.subject} • Due: {assignment.dueDate}</p>
                      </div>
                    </div>
                    <Badge className={assignment.status === "submitted" ? "bg-green-500/10 text-green-600" : "bg-orange-500/10 text-orange-600"}>
                      {assignment.status === "submitted" ? "Submitted" : "Pending"}
                    </Badge>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="fees">
          <Card>
            <CardHeader>
              <CardTitle>Fee Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-3 gap-4">
                <div className="p-4 rounded-lg bg-muted/50 text-center">
                  <p className="text-sm text-muted-foreground">Total Fee</p>
                  <p className="text-2xl font-bold">₹{feeData.totalFee.toLocaleString()}</p>
                </div>
                <div className="p-4 rounded-lg bg-green-500/10 text-center">
                  <p className="text-sm text-muted-foreground">Paid</p>
                  <p className="text-2xl font-bold text-green-600">₹{feeData.paid.toLocaleString()}</p>
                </div>
                <div className="p-4 rounded-lg bg-red-500/10 text-center">
                  <p className="text-sm text-muted-foreground">Pending</p>
                  <p className="text-2xl font-bold text-red-600">₹{feeData.pending.toLocaleString()}</p>
                </div>
              </div>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span>Payment Progress</span>
                  <span>{Math.round((feeData.paid / feeData.totalFee) * 100)}%</span>
                </div>
                <Progress value={(feeData.paid / feeData.totalFee) * 100} className="h-3" />
              </div>
              <div className="flex items-center justify-between p-4 border rounded-lg">
                <div className="flex items-center gap-3">
                  <AlertTriangle className="h-5 w-5 text-orange-500" />
                  <div>
                    <p className="font-medium">Next Due Date</p>
                    <p className="text-sm text-muted-foreground">{new Date(feeData.nextDueDate).toLocaleDateString()}</p>
                  </div>
                </div>
                <Button>Pay Now</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="downloads">
          <Card>
            <CardHeader>
              <CardTitle>Downloadable Documents</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid md:grid-cols-2 gap-4">
                {[
                  { name: "Report Card - Term 1", date: "Oct 2024", type: "report" },
                  { name: "Admit Card - Term 2 Exam", date: "Dec 2024", type: "admit" },
                  { name: "Fee Receipt - Q3", date: "Oct 2024", type: "receipt" },
                  { name: "Character Certificate", date: "Nov 2024", type: "certificate" },
                ].map((doc, idx) => (
                  <div key={idx} className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-primary/10">
                        <FileText className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <p className="font-medium">{doc.name}</p>
                        <p className="text-sm text-muted-foreground">{doc.date}</p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      <Download className="h-4 w-4 mr-1" />
                      Download
                    </Button>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
