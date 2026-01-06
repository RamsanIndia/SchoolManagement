import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { ArrowLeft, Upload, User, Users, GraduationCap, Heart, Bus, FileText, Save, X } from "lucide-react";
import { toast } from "@/hooks/use-toast";

export default function StudentForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = !!id;

  const [activeTab, setActiveTab] = useState("personal");
  const [formData, setFormData] = useState({
    // Personal Details
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    dob: "",
    gender: "",
    bloodGroup: "",
    religion: "",
    category: "",
    nationality: "Indian",
    // Academic Details
    admissionNo: "",
    admissionDate: "",
    class: "",
    section: "",
    rollNo: "",
    previousSchool: "",
    previousClass: "",
    // Parent Details
    fatherName: "",
    fatherPhone: "",
    fatherEmail: "",
    fatherOccupation: "",
    motherName: "",
    motherPhone: "",
    motherEmail: "",
    motherOccupation: "",
    guardianName: "",
    guardianPhone: "",
    guardianRelation: "",
    // Address
    street: "",
    city: "",
    state: "",
    pincode: "",
    country: "India",
    // Medical Info
    allergies: "",
    medicalConditions: "",
    emergencyContact: "",
    doctorName: "",
    doctorPhone: "",
    // Transport
    transportMode: "",
    busRoute: "",
    pickupPoint: "",
    // Settings
    isActive: true,
    smsAlerts: true,
    emailAlerts: true,
  });

  const handleChange = (field: string, value: string | boolean) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = () => {
    toast({
      title: isEditing ? "Student Updated" : "Student Added",
      description: `Student ${formData.firstName} ${formData.lastName} has been ${isEditing ? "updated" : "added"} successfully.`,
    });
    navigate("/students");
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
            <h1 className="text-3xl font-bold">{isEditing ? "Edit Student" : "Add New Student"}</h1>
            <p className="text-muted-foreground">
              {isEditing ? "Update student information" : "Register a new student in the system"}
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate(-1)}>
            <X className="h-4 w-4 mr-2" />
            Cancel
          </Button>
          <Button onClick={handleSubmit}>
            <Save className="h-4 w-4 mr-2" />
            {isEditing ? "Update Student" : "Add Student"}
          </Button>
        </div>
      </div>

      {/* Photo Upload */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-center gap-6">
            <Avatar className="h-24 w-24">
              <AvatarImage src="" />
              <AvatarFallback className="text-2xl bg-primary/10 text-primary">
                {formData.firstName?.[0] || "S"}{formData.lastName?.[0] || "T"}
              </AvatarFallback>
            </Avatar>
            <div>
              <h3 className="font-medium mb-1">Student Photo</h3>
              <p className="text-sm text-muted-foreground mb-3">Upload a passport size photo (max 2MB)</p>
              <Button variant="outline" size="sm">
                <Upload className="h-4 w-4 mr-2" />
                Upload Photo
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Form Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="grid w-full grid-cols-6">
          <TabsTrigger value="personal" className="flex items-center gap-2">
            <User className="h-4 w-4" />
            Personal
          </TabsTrigger>
          <TabsTrigger value="academic" className="flex items-center gap-2">
            <GraduationCap className="h-4 w-4" />
            Academic
          </TabsTrigger>
          <TabsTrigger value="parent" className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            Parent/Guardian
          </TabsTrigger>
          <TabsTrigger value="medical" className="flex items-center gap-2">
            <Heart className="h-4 w-4" />
            Medical
          </TabsTrigger>
          <TabsTrigger value="transport" className="flex items-center gap-2">
            <Bus className="h-4 w-4" />
            Transport
          </TabsTrigger>
          <TabsTrigger value="documents" className="flex items-center gap-2">
            <FileText className="h-4 w-4" />
            Documents
          </TabsTrigger>
        </TabsList>

        <TabsContent value="personal">
          <Card>
            <CardHeader>
              <CardTitle>Personal Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">First Name *</Label>
                  <Input
                    id="firstName"
                    value={formData.firstName}
                    onChange={(e) => handleChange("firstName", e.target.value)}
                    placeholder="Enter first name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Last Name *</Label>
                  <Input
                    id="lastName"
                    value={formData.lastName}
                    onChange={(e) => handleChange("lastName", e.target.value)}
                    placeholder="Enter last name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="dob">Date of Birth *</Label>
                  <Input
                    id="dob"
                    type="date"
                    value={formData.dob}
                    onChange={(e) => handleChange("dob", e.target.value)}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="gender">Gender *</Label>
                  <Select value={formData.gender} onValueChange={(v) => handleChange("gender", v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select gender" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="male">Male</SelectItem>
                      <SelectItem value="female">Female</SelectItem>
                      <SelectItem value="other">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="bloodGroup">Blood Group</Label>
                  <Select value={formData.bloodGroup} onValueChange={(v) => handleChange("bloodGroup", v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select blood group" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="A+">A+</SelectItem>
                      <SelectItem value="A-">A-</SelectItem>
                      <SelectItem value="B+">B+</SelectItem>
                      <SelectItem value="B-">B-</SelectItem>
                      <SelectItem value="AB+">AB+</SelectItem>
                      <SelectItem value="AB-">AB-</SelectItem>
                      <SelectItem value="O+">O+</SelectItem>
                      <SelectItem value="O-">O-</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="religion">Religion</Label>
                  <Input
                    id="religion"
                    value={formData.religion}
                    onChange={(e) => handleChange("religion", e.target.value)}
                    placeholder="Enter religion"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="email">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    value={formData.email}
                    onChange={(e) => handleChange("email", e.target.value)}
                    placeholder="Enter email"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone">Phone Number</Label>
                  <Input
                    id="phone"
                    value={formData.phone}
                    onChange={(e) => handleChange("phone", e.target.value)}
                    placeholder="Enter phone number"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="nationality">Nationality</Label>
                  <Input
                    id="nationality"
                    value={formData.nationality}
                    onChange={(e) => handleChange("nationality", e.target.value)}
                    placeholder="Enter nationality"
                  />
                </div>
              </div>

              <div className="border-t pt-4">
                <h4 className="font-medium mb-4">Address</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2 md:col-span-2">
                    <Label htmlFor="street">Street Address</Label>
                    <Input
                      id="street"
                      value={formData.street}
                      onChange={(e) => handleChange("street", e.target.value)}
                      placeholder="Enter street address"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="city">City</Label>
                    <Input
                      id="city"
                      value={formData.city}
                      onChange={(e) => handleChange("city", e.target.value)}
                      placeholder="Enter city"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="state">State</Label>
                    <Input
                      id="state"
                      value={formData.state}
                      onChange={(e) => handleChange("state", e.target.value)}
                      placeholder="Enter state"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="pincode">PIN Code</Label>
                    <Input
                      id="pincode"
                      value={formData.pincode}
                      onChange={(e) => handleChange("pincode", e.target.value)}
                      placeholder="Enter PIN code"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="country">Country</Label>
                    <Input
                      id="country"
                      value={formData.country}
                      onChange={(e) => handleChange("country", e.target.value)}
                      placeholder="Enter country"
                    />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="academic">
          <Card>
            <CardHeader>
              <CardTitle>Academic Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="admissionNo">Admission Number *</Label>
                  <Input
                    id="admissionNo"
                    value={formData.admissionNo}
                    onChange={(e) => handleChange("admissionNo", e.target.value)}
                    placeholder="Enter admission number"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="admissionDate">Admission Date *</Label>
                  <Input
                    id="admissionDate"
                    type="date"
                    value={formData.admissionDate}
                    onChange={(e) => handleChange("admissionDate", e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="rollNo">Roll Number</Label>
                  <Input
                    id="rollNo"
                    value={formData.rollNo}
                    onChange={(e) => handleChange("rollNo", e.target.value)}
                    placeholder="Enter roll number"
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="class">Class *</Label>
                  <Select value={formData.class} onValueChange={(v) => handleChange("class", v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select class" />
                    </SelectTrigger>
                    <SelectContent>
                      {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map((c) => (
                        <SelectItem key={c} value={c.toString()}>Class {c}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="section">Section *</Label>
                  <Select value={formData.section} onValueChange={(v) => handleChange("section", v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select section" />
                    </SelectTrigger>
                    <SelectContent>
                      {["A", "B", "C", "D", "E"].map((s) => (
                        <SelectItem key={s} value={s}>Section {s}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="border-t pt-4">
                <h4 className="font-medium mb-4">Previous School Details</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="previousSchool">Previous School Name</Label>
                    <Input
                      id="previousSchool"
                      value={formData.previousSchool}
                      onChange={(e) => handleChange("previousSchool", e.target.value)}
                      placeholder="Enter previous school name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="previousClass">Previous Class</Label>
                    <Input
                      id="previousClass"
                      value={formData.previousClass}
                      onChange={(e) => handleChange("previousClass", e.target.value)}
                      placeholder="Enter previous class"
                    />
                  </div>
                </div>
              </div>

              <div className="border-t pt-4">
                <h4 className="font-medium mb-4">Account Settings</h4>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <Label>Active Status</Label>
                      <p className="text-sm text-muted-foreground">Student is currently active</p>
                    </div>
                    <Switch
                      checked={formData.isActive}
                      onCheckedChange={(v) => handleChange("isActive", v)}
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <Label>SMS Alerts</Label>
                      <p className="text-sm text-muted-foreground">Send SMS notifications</p>
                    </div>
                    <Switch
                      checked={formData.smsAlerts}
                      onCheckedChange={(v) => handleChange("smsAlerts", v)}
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <div>
                      <Label>Email Alerts</Label>
                      <p className="text-sm text-muted-foreground">Send email notifications</p>
                    </div>
                    <Switch
                      checked={formData.emailAlerts}
                      onCheckedChange={(v) => handleChange("emailAlerts", v)}
                    />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="parent">
          <Card>
            <CardHeader>
              <CardTitle>Parent / Guardian Details</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="border-b pb-6">
                <h4 className="font-medium mb-4">Father's Details</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="fatherName">Father's Name *</Label>
                    <Input
                      id="fatherName"
                      value={formData.fatherName}
                      onChange={(e) => handleChange("fatherName", e.target.value)}
                      placeholder="Enter father's name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="fatherPhone">Father's Phone *</Label>
                    <Input
                      id="fatherPhone"
                      value={formData.fatherPhone}
                      onChange={(e) => handleChange("fatherPhone", e.target.value)}
                      placeholder="Enter father's phone"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="fatherEmail">Father's Email</Label>
                    <Input
                      id="fatherEmail"
                      type="email"
                      value={formData.fatherEmail}
                      onChange={(e) => handleChange("fatherEmail", e.target.value)}
                      placeholder="Enter father's email"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="fatherOccupation">Father's Occupation</Label>
                    <Input
                      id="fatherOccupation"
                      value={formData.fatherOccupation}
                      onChange={(e) => handleChange("fatherOccupation", e.target.value)}
                      placeholder="Enter father's occupation"
                    />
                  </div>
                </div>
              </div>

              <div className="border-b pb-6">
                <h4 className="font-medium mb-4">Mother's Details</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="motherName">Mother's Name *</Label>
                    <Input
                      id="motherName"
                      value={formData.motherName}
                      onChange={(e) => handleChange("motherName", e.target.value)}
                      placeholder="Enter mother's name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="motherPhone">Mother's Phone</Label>
                    <Input
                      id="motherPhone"
                      value={formData.motherPhone}
                      onChange={(e) => handleChange("motherPhone", e.target.value)}
                      placeholder="Enter mother's phone"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="motherEmail">Mother's Email</Label>
                    <Input
                      id="motherEmail"
                      type="email"
                      value={formData.motherEmail}
                      onChange={(e) => handleChange("motherEmail", e.target.value)}
                      placeholder="Enter mother's email"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="motherOccupation">Mother's Occupation</Label>
                    <Input
                      id="motherOccupation"
                      value={formData.motherOccupation}
                      onChange={(e) => handleChange("motherOccupation", e.target.value)}
                      placeholder="Enter mother's occupation"
                    />
                  </div>
                </div>
              </div>

              <div>
                <h4 className="font-medium mb-4">Guardian Details (if different)</h4>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="guardianName">Guardian's Name</Label>
                    <Input
                      id="guardianName"
                      value={formData.guardianName}
                      onChange={(e) => handleChange("guardianName", e.target.value)}
                      placeholder="Enter guardian's name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="guardianPhone">Guardian's Phone</Label>
                    <Input
                      id="guardianPhone"
                      value={formData.guardianPhone}
                      onChange={(e) => handleChange("guardianPhone", e.target.value)}
                      placeholder="Enter guardian's phone"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="guardianRelation">Relation</Label>
                    <Input
                      id="guardianRelation"
                      value={formData.guardianRelation}
                      onChange={(e) => handleChange("guardianRelation", e.target.value)}
                      placeholder="e.g., Uncle, Aunt"
                    />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="medical">
          <Card>
            <CardHeader>
              <CardTitle>Medical Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="allergies">Allergies</Label>
                  <Textarea
                    id="allergies"
                    value={formData.allergies}
                    onChange={(e) => handleChange("allergies", e.target.value)}
                    placeholder="List any known allergies"
                    rows={3}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="medicalConditions">Medical Conditions</Label>
                  <Textarea
                    id="medicalConditions"
                    value={formData.medicalConditions}
                    onChange={(e) => handleChange("medicalConditions", e.target.value)}
                    placeholder="List any medical conditions"
                    rows={3}
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="emergencyContact">Emergency Contact</Label>
                  <Input
                    id="emergencyContact"
                    value={formData.emergencyContact}
                    onChange={(e) => handleChange("emergencyContact", e.target.value)}
                    placeholder="Emergency contact number"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="doctorName">Family Doctor</Label>
                  <Input
                    id="doctorName"
                    value={formData.doctorName}
                    onChange={(e) => handleChange("doctorName", e.target.value)}
                    placeholder="Doctor's name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="doctorPhone">Doctor's Phone</Label>
                  <Input
                    id="doctorPhone"
                    value={formData.doctorPhone}
                    onChange={(e) => handleChange("doctorPhone", e.target.value)}
                    placeholder="Doctor's phone number"
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="transport">
          <Card>
            <CardHeader>
              <CardTitle>Transport Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="transportMode">Transport Mode</Label>
                  <Select value={formData.transportMode} onValueChange={(v) => handleChange("transportMode", v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select mode" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="school_bus">School Bus</SelectItem>
                      <SelectItem value="private">Private Vehicle</SelectItem>
                      <SelectItem value="public">Public Transport</SelectItem>
                      <SelectItem value="walk">Walking</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="busRoute">Bus Route</Label>
                  <Select value={formData.busRoute} onValueChange={(v) => handleChange("busRoute", v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select route" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="route1">Route 1 - Sector 15</SelectItem>
                      <SelectItem value="route2">Route 2 - Sector 21</SelectItem>
                      <SelectItem value="route3">Route 3 - Sector 28</SelectItem>
                      <SelectItem value="route4">Route 4 - Sector 35</SelectItem>
                      <SelectItem value="route5">Route 5 - Sector 42</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="pickupPoint">Pickup Point</Label>
                  <Input
                    id="pickupPoint"
                    value={formData.pickupPoint}
                    onChange={(e) => handleChange("pickupPoint", e.target.value)}
                    placeholder="Enter pickup point"
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="documents">
          <Card>
            <CardHeader>
              <CardTitle>Document Upload</CardTitle>
            </CardHeader>
            <CardContent className="space-y-6">
              {[
                { name: "Aadhar Card", required: true },
                { name: "Birth Certificate", required: true },
                { name: "Transfer Certificate", required: false },
                { name: "Previous School Marksheet", required: false },
                { name: "Medical Certificate", required: false },
                { name: "Passport Size Photos", required: true },
              ].map((doc, index) => (
                <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <p className="font-medium">
                      {doc.name}
                      {doc.required && <span className="text-red-500 ml-1">*</span>}
                    </p>
                    <p className="text-sm text-muted-foreground">PDF, JPG, PNG (max 5MB)</p>
                  </div>
                  <Button variant="outline" size="sm">
                    <Upload className="h-4 w-4 mr-2" />
                    Upload
                  </Button>
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
