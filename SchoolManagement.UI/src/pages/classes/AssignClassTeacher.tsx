/**
 * Assign Class Teacher Screen
 * Assign a teacher to a specific section with availability preview
 */

import { useState } from "react";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { ArrowLeft, AlertTriangle, CheckCircle, Clock, Calendar, User } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { toast } from "@/hooks/use-toast";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";

interface TeacherSchedule {
  day: string;
  periods: { time: string; subject: string; class: string; available: boolean }[];
}

interface Teacher {
  id: string;
  name: string;
  subject: string;
  currentClasses: string[];
  schedule: TeacherSchedule[];
}

const mockTeachers: Teacher[] = [
  {
    id: "1",
    name: "Anita Sharma",
    subject: "Mathematics",
    currentClasses: ["Grade 1-A"],
    schedule: [
      { day: "Monday", periods: [
        { time: "9:00 AM", subject: "Math", class: "Grade 1-A", available: false },
        { time: "10:00 AM", subject: "Math", class: "Grade 2-B", available: false },
        { time: "11:00 AM", subject: "-", class: "-", available: true },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
      { day: "Tuesday", periods: [
        { time: "9:00 AM", subject: "-", class: "-", available: true },
        { time: "10:00 AM", subject: "Math", class: "Grade 1-A", available: false },
        { time: "11:00 AM", subject: "Math", class: "Grade 3-A", available: false },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
    ],
  },
  {
    id: "2",
    name: "Ravi Kumar",
    subject: "Science",
    currentClasses: ["Grade 2-A", "Grade 3-B"],
    schedule: [
      { day: "Monday", periods: [
        { time: "9:00 AM", subject: "Science", class: "Grade 2-A", available: false },
        { time: "10:00 AM", subject: "-", class: "-", available: true },
        { time: "11:00 AM", subject: "Science", class: "Grade 3-B", available: false },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
      { day: "Tuesday", periods: [
        { time: "9:00 AM", subject: "Science", class: "Grade 2-A", available: false },
        { time: "10:00 AM", subject: "-", class: "-", available: true },
        { time: "11:00 AM", subject: "-", class: "-", available: true },
        { time: "12:00 PM", subject: "Science", class: "Grade 3-B", available: false },
      ]},
    ],
  },
  {
    id: "3",
    name: "Priya Singh",
    subject: "English",
    currentClasses: ["Grade 4-A"],
    schedule: [
      { day: "Monday", periods: [
        { time: "9:00 AM", subject: "-", class: "-", available: true },
        { time: "10:00 AM", subject: "English", class: "Grade 4-A", available: false },
        { time: "11:00 AM", subject: "-", class: "-", available: true },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
      { day: "Tuesday", periods: [
        { time: "9:00 AM", subject: "English", class: "Grade 4-A", available: false },
        { time: "10:00 AM", subject: "-", class: "-", available: true },
        { time: "11:00 AM", subject: "-", class: "-", available: true },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
    ],
  },
  {
    id: "4",
    name: "Rohan Verma",
    subject: "Hindi",
    currentClasses: [],
    schedule: [
      { day: "Monday", periods: [
        { time: "9:00 AM", subject: "-", class: "-", available: true },
        { time: "10:00 AM", subject: "-", class: "-", available: true },
        { time: "11:00 AM", subject: "-", class: "-", available: true },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
      { day: "Tuesday", periods: [
        { time: "9:00 AM", subject: "-", class: "-", available: true },
        { time: "10:00 AM", subject: "-", class: "-", available: true },
        { time: "11:00 AM", subject: "-", class: "-", available: true },
        { time: "12:00 PM", subject: "-", class: "-", available: true },
      ]},
    ],
  },
];

const mockSections = ["A", "B", "C"];

const classNames: Record<string, string> = {
  "1": "Grade 1",
  "2": "Grade 2",
  "3": "Grade 3",
  "4": "Grade 4",
  "5": "Grade 5",
};

export default function AssignClassTeacher() {
  const { classId } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const className = classNames[classId || "1"] || "Grade 1";
  const initialSection = searchParams.get("section") || "";
  
  const [selectedTeacher, setSelectedTeacher] = useState("");
  const [selectedSection, setSelectedSection] = useState(initialSection);
  const [hasClash, setHasClash] = useState(false);
  
  const selectedTeacherData = mockTeachers.find(t => t.id === selectedTeacher);

  const handleTeacherChange = (teacherId: string) => {
    setSelectedTeacher(teacherId);
    const teacher = mockTeachers.find(t => t.id === teacherId);
    // Simulate clash detection
    if (teacher && teacher.currentClasses.length >= 2) {
      setHasClash(true);
    } else {
      setHasClash(false);
    }
  };

  const handleSave = () => {
    if (!selectedTeacher || !selectedSection) {
      toast({ title: "Error", description: "Please select both teacher and section", variant: "destructive" });
      return;
    }
    toast({ 
      title: "Success", 
      description: `${selectedTeacherData?.name} assigned to ${className} Section ${selectedSection}` 
    });
    navigate(`/classes/${classId}/sections`);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate(`/classes/${classId}/sections`)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold text-foreground">Assign Class Teacher</h1>
          <p className="text-muted-foreground mt-1">{className}</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Assignment Form */}
        <Card className="bg-card border-border">
          <CardHeader>
            <CardTitle className="text-foreground">Teacher Assignment</CardTitle>
            <CardDescription>Select a teacher and section for assignment</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="teacher">Select Teacher *</Label>
              <Select value={selectedTeacher} onValueChange={handleTeacherChange}>
                <SelectTrigger className="bg-background">
                  <SelectValue placeholder="Choose a teacher" />
                </SelectTrigger>
                <SelectContent className="bg-popover">
                  {mockTeachers.map(teacher => (
                    <SelectItem key={teacher.id} value={teacher.id}>
                      <div className="flex items-center gap-2">
                        <User className="h-4 w-4" />
                        <span>{teacher.name}</span>
                        <Badge variant="outline" className="ml-2 text-xs">{teacher.subject}</Badge>
                      </div>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="section">Select Section *</Label>
              <Select value={selectedSection} onValueChange={setSelectedSection}>
                <SelectTrigger className="bg-background">
                  <SelectValue placeholder="Choose a section" />
                </SelectTrigger>
                <SelectContent className="bg-popover">
                  {mockSections.map(section => (
                    <SelectItem key={section} value={section}>Section {section}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {hasClash && (
              <Alert variant="destructive">
                <AlertTriangle className="h-4 w-4" />
                <AlertTitle>Schedule Clash Detected</AlertTitle>
                <AlertDescription>
                  This teacher is already assigned to multiple classes. Please review their schedule before proceeding.
                </AlertDescription>
              </Alert>
            )}

            {selectedTeacherData && !hasClash && (
              <Alert className="border-green-500/30 bg-green-500/10">
                <CheckCircle className="h-4 w-4 text-green-500" />
                <AlertTitle className="text-green-600">Available</AlertTitle>
                <AlertDescription className="text-green-600/80">
                  This teacher is available for assignment.
                </AlertDescription>
              </Alert>
            )}

            <div className="flex gap-3 pt-4">
              <Button variant="outline" onClick={() => navigate(`/classes/${classId}/sections`)} className="flex-1">
                Cancel
              </Button>
              <Button onClick={handleSave} className="flex-1 bg-primary hover:bg-primary/90">
                Save Assignment
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Teacher Schedule Preview */}
        <Card className="bg-card border-border">
          <CardHeader>
            <CardTitle className="text-foreground flex items-center gap-2">
              <Calendar className="h-5 w-5" />
              Teacher Timetable Preview
            </CardTitle>
            <CardDescription>
              {selectedTeacherData ? `${selectedTeacherData.name}'s schedule` : "Select a teacher to view schedule"}
            </CardDescription>
          </CardHeader>
          <CardContent>
            {selectedTeacherData ? (
              <div className="space-y-4">
                {/* Current Assignments */}
                <div className="space-y-2">
                  <Label className="text-muted-foreground">Current Class Assignments</Label>
                  <div className="flex flex-wrap gap-2">
                    {selectedTeacherData.currentClasses.length > 0 ? (
                      selectedTeacherData.currentClasses.map((cls, idx) => (
                        <Badge key={idx} variant="secondary">{cls}</Badge>
                      ))
                    ) : (
                      <span className="text-sm text-muted-foreground">No current assignments</span>
                    )}
                  </div>
                </div>

                {/* Schedule Grid */}
                <div className="space-y-3 mt-4">
                  {selectedTeacherData.schedule.map((day, dayIdx) => (
                    <div key={dayIdx} className="space-y-2">
                      <h4 className="font-medium text-sm text-foreground">{day.day}</h4>
                      <div className="grid grid-cols-4 gap-2">
                        {day.periods.map((period, periodIdx) => (
                          <div
                            key={periodIdx}
                            className={`p-2 rounded-lg text-xs border ${
                              period.available 
                                ? "bg-green-500/10 border-green-500/30 text-green-600" 
                                : "bg-muted border-border text-muted-foreground"
                            }`}
                          >
                            <div className="flex items-center gap-1">
                              <Clock className="h-3 w-3" />
                              {period.time}
                            </div>
                            {!period.available && (
                              <div className="mt-1">
                                <div className="font-medium">{period.subject}</div>
                                <div className="text-xs opacity-70">{period.class}</div>
                              </div>
                            )}
                            {period.available && (
                              <div className="mt-1 font-medium">Available</div>
                            )}
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-12 text-muted-foreground">
                <Calendar className="h-12 w-12 mb-4 opacity-30" />
                <p>Select a teacher to view their schedule</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
