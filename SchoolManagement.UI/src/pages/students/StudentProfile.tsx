import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import {
  ArrowLeft,
  Edit,
  Download,
  Mail,
  Phone,
  MapPin,
  Calendar,
  User,
  Users,
  FileText,
  CreditCard,
  Clock,
  GraduationCap,
  Bus,
  Heart,
} from "lucide-react";

// Mock student data
const mockStudent = {
  id: "ADM001",
  admissionNo: "ADM001",
  name: "Rohan Sharma",
  email: "rohan.sharma@student.edu",
  phone: "+91 9876543210",
  dob: "2010-05-15",
  gender: "Male",
  bloodGroup: "B+",
  class: "5",
  section: "A",
  rollNo: "12",
  status: "Active",
  photo: "",
  address: {
    street: "123 Main Street",
    city: "New Delhi",
    state: "Delhi",
    pincode: "110001",
  },
  parent: {
    fatherName: "Rajesh Sharma",
    fatherPhone: "+91 9876543210",
    fatherEmail: "rajesh.sharma@email.com",
    fatherOccupation: "Business",
    motherName: "Priya Sharma",
    motherPhone: "+91 9876543211",
    motherEmail: "priya.sharma@email.com",
    motherOccupation: "Teacher",
  },
  academic: {
    admissionDate: "2020-04-01",
    previousSchool: "ABC Primary School",
    previousClass: "4",
    achievements: ["Science Olympiad Gold", "Best Student Award 2023"],
  },
  medical: {
    allergies: "None",
    conditions: "None",
    emergencyContact: "+91 9876543212",
  },
  transport: {
    mode: "School Bus",
    route: "Route 5 - Sector 21",
    busNo: "DL-05-AB-1234",
    pickupPoint: "Sector 21 Main Gate",
  },
  documents: [
    { name: "Aadhar Card", status: "Uploaded", date: "2020-04-01" },
    { name: "Birth Certificate", status: "Uploaded", date: "2020-04-01" },
    { name: "Transfer Certificate", status: "Pending", date: "-" },
    { name: "Photo", status: "Uploaded", date: "2024-01-15" },
  ],
  attendance: {
    totalDays: 180,
    present: 165,
    absent: 10,
    late: 5,
    percentage: 91.67,
  },
  fees: {
    totalFee: 85000,
    paid: 60000,
    pending: 25000,
    lastPayment: "2024-02-15",
  },
};

export default function StudentProfile() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [activeTab, setActiveTab] = useState("basic");

  const student = mockStudent; // In real app, fetch by id

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Student Profile</h1>
            <p className="text-muted-foreground">View and manage student information</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={() => navigate(`/students/edit/${id}`)}>
            <Edit className="h-4 w-4 mr-2" />
            Edit Profile
          </Button>
        </div>
      </div>

      {/* Profile Header Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-6 items-start">
            <Avatar className="h-32 w-32">
              <AvatarImage src={student.photo} />
              <AvatarFallback className="text-3xl bg-primary/10 text-primary">
                {student.name.split(" ").map((n) => n[0]).join("")}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1 space-y-4">
              <div className="flex items-start justify-between">
                <div>
                  <h2 className="text-2xl font-bold">{student.name}</h2>
                  <p className="text-muted-foreground">Admission No: {student.admissionNo}</p>
                </div>
                <Badge className={student.status === "Active" ? "bg-green-500/10 text-green-600" : "bg-red-500/10 text-red-600"}>
                  {student.status}
                </Badge>
              </div>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="flex items-center gap-2">
                  <GraduationCap className="h-4 w-4 text-muted-foreground" />
                  <span>Class {student.class}-{student.section}</span>
                </div>
                <div className="flex items-center gap-2">
                  <User className="h-4 w-4 text-muted-foreground" />
                  <span>Roll No: {student.rollNo}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Mail className="h-4 w-4 text-muted-foreground" />
                  <span className="truncate">{student.email}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Phone className="h-4 w-4 text-muted-foreground" />
                  <span>{student.phone}</span>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Quick Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-blue-500/10">
                <Clock className="h-6 w-6 text-blue-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Attendance</p>
                <p className="text-2xl font-bold">{student.attendance.percentage}%</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-green-500/10">
                <CreditCard className="h-6 w-6 text-green-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Fee Paid</p>
                <p className="text-2xl font-bold">₹{student.fees.paid.toLocaleString()}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-orange-500/10">
                <CreditCard className="h-6 w-6 text-orange-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Fee Pending</p>
                <p className="text-2xl font-bold">₹{student.fees.pending.toLocaleString()}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center gap-4">
              <div className="p-3 rounded-full bg-purple-500/10">
                <FileText className="h-6 w-6 text-purple-500" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Documents</p>
                <p className="text-2xl font-bold">3/4</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Detailed Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="basic">Basic Details</TabsTrigger>
          <TabsTrigger value="academic">Academic</TabsTrigger>
          <TabsTrigger value="parent">Parent Details</TabsTrigger>
          <TabsTrigger value="address">Address</TabsTrigger>
          <TabsTrigger value="documents">Documents</TabsTrigger>
          <TabsTrigger value="fees">Fee Summary</TabsTrigger>
        </TabsList>

        <TabsContent value="basic" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <User className="h-5 w-5" />
                Personal Information
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
                <div>
                  <p className="text-sm text-muted-foreground">Full Name</p>
                  <p className="font-medium">{student.name}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Date of Birth</p>
                  <p className="font-medium">{new Date(student.dob).toLocaleDateString()}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Gender</p>
                  <p className="font-medium">{student.gender}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Blood Group</p>
                  <p className="font-medium">{student.bloodGroup}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Email</p>
                  <p className="font-medium">{student.email}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Phone</p>
                  <p className="font-medium">{student.phone}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Heart className="h-5 w-5" />
                Medical Information
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
                <div>
                  <p className="text-sm text-muted-foreground">Allergies</p>
                  <p className="font-medium">{student.medical.allergies}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Medical Conditions</p>
                  <p className="font-medium">{student.medical.conditions}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Emergency Contact</p>
                  <p className="font-medium">{student.medical.emergencyContact}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Bus className="h-5 w-5" />
                Transport Information
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                <div>
                  <p className="text-sm text-muted-foreground">Transport Mode</p>
                  <p className="font-medium">{student.transport.mode}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Route</p>
                  <p className="font-medium">{student.transport.route}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Bus Number</p>
                  <p className="font-medium">{student.transport.busNo}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Pickup Point</p>
                  <p className="font-medium">{student.transport.pickupPoint}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="academic" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <GraduationCap className="h-5 w-5" />
                Academic Information
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
                <div>
                  <p className="text-sm text-muted-foreground">Admission Date</p>
                  <p className="font-medium">{new Date(student.academic.admissionDate).toLocaleDateString()}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Previous School</p>
                  <p className="font-medium">{student.academic.previousSchool}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Previous Class</p>
                  <p className="font-medium">{student.academic.previousClass}</p>
                </div>
              </div>
              <div className="mt-6">
                <p className="text-sm text-muted-foreground mb-2">Achievements</p>
                <div className="flex flex-wrap gap-2">
                  {student.academic.achievements.map((achievement, index) => (
                    <Badge key={index} variant="secondary">{achievement}</Badge>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Attendance Summary
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-muted-foreground">Overall Attendance</span>
                  <span className="font-bold text-lg">{student.attendance.percentage}%</span>
                </div>
                <Progress value={student.attendance.percentage} className="h-3" />
                <div className="grid grid-cols-4 gap-4 mt-4">
                  <div className="text-center p-4 rounded-lg bg-muted/50">
                    <p className="text-2xl font-bold">{student.attendance.totalDays}</p>
                    <p className="text-sm text-muted-foreground">Total Days</p>
                  </div>
                  <div className="text-center p-4 rounded-lg bg-green-500/10">
                    <p className="text-2xl font-bold text-green-600">{student.attendance.present}</p>
                    <p className="text-sm text-muted-foreground">Present</p>
                  </div>
                  <div className="text-center p-4 rounded-lg bg-red-500/10">
                    <p className="text-2xl font-bold text-red-600">{student.attendance.absent}</p>
                    <p className="text-sm text-muted-foreground">Absent</p>
                  </div>
                  <div className="text-center p-4 rounded-lg bg-orange-500/10">
                    <p className="text-2xl font-bold text-orange-600">{student.attendance.late}</p>
                    <p className="text-sm text-muted-foreground">Late</p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="parent" className="space-y-4">
          <div className="grid md:grid-cols-2 gap-4">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="h-5 w-5" />
                  Father's Details
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm text-muted-foreground">Name</p>
                  <p className="font-medium">{student.parent.fatherName}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Phone</p>
                  <p className="font-medium">{student.parent.fatherPhone}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Email</p>
                  <p className="font-medium">{student.parent.fatherEmail}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Occupation</p>
                  <p className="font-medium">{student.parent.fatherOccupation}</p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="h-5 w-5" />
                  Mother's Details
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm text-muted-foreground">Name</p>
                  <p className="font-medium">{student.parent.motherName}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Phone</p>
                  <p className="font-medium">{student.parent.motherPhone}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Email</p>
                  <p className="font-medium">{student.parent.motherEmail}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Occupation</p>
                  <p className="font-medium">{student.parent.motherOccupation}</p>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="address">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                Address & Contact
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                <div>
                  <p className="text-sm text-muted-foreground">Street Address</p>
                  <p className="font-medium">{student.address.street}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">City</p>
                  <p className="font-medium">{student.address.city}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">State</p>
                  <p className="font-medium">{student.address.state}</p>
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">PIN Code</p>
                  <p className="font-medium">{student.address.pincode}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="documents">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Documents
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {student.documents.map((doc, index) => (
                  <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                    <div className="flex items-center gap-4">
                      <div className="p-2 rounded-lg bg-muted">
                        <FileText className="h-5 w-5" />
                      </div>
                      <div>
                        <p className="font-medium">{doc.name}</p>
                        <p className="text-sm text-muted-foreground">Uploaded: {doc.date}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <Badge variant={doc.status === "Uploaded" ? "default" : "secondary"}>
                        {doc.status}
                      </Badge>
                      {doc.status === "Uploaded" && (
                        <Button variant="ghost" size="sm">
                          <Download className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="fees">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <CreditCard className="h-5 w-5" />
                Fee Summary
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className="p-4 rounded-lg bg-muted/50">
                    <p className="text-sm text-muted-foreground">Total Fee</p>
                    <p className="text-2xl font-bold">₹{student.fees.totalFee.toLocaleString()}</p>
                  </div>
                  <div className="p-4 rounded-lg bg-green-500/10">
                    <p className="text-sm text-muted-foreground">Paid Amount</p>
                    <p className="text-2xl font-bold text-green-600">₹{student.fees.paid.toLocaleString()}</p>
                  </div>
                  <div className="p-4 rounded-lg bg-red-500/10">
                    <p className="text-sm text-muted-foreground">Pending Amount</p>
                    <p className="text-2xl font-bold text-red-600">₹{student.fees.pending.toLocaleString()}</p>
                  </div>
                  <div className="p-4 rounded-lg bg-blue-500/10">
                    <p className="text-sm text-muted-foreground">Last Payment</p>
                    <p className="text-2xl font-bold text-blue-600">{new Date(student.fees.lastPayment).toLocaleDateString()}</p>
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Payment Progress</span>
                    <span>{Math.round((student.fees.paid / student.fees.totalFee) * 100)}%</span>
                  </div>
                  <Progress value={(student.fees.paid / student.fees.totalFee) * 100} className="h-3" />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
