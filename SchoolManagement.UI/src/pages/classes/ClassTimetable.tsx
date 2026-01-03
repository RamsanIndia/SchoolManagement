/**
 * Class Timetable Preview
 * Weekly timetable grid view for a class/section
 */

import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Calendar, Clock, Download, Printer } from "lucide-react";
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

interface TimetableSlot {
  subject: string;
  teacher: string;
  room: string;
  color: string;
}

interface DaySchedule {
  day: string;
  slots: (TimetableSlot | null)[];
}

const timeSlots = [
  "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", 
  "1:00 PM", "2:00 PM", "3:00 PM"
];

const subjectColors: Record<string, string> = {
  "Math": "bg-blue-500/20 border-blue-500/40 text-blue-600",
  "English": "bg-purple-500/20 border-purple-500/40 text-purple-600",
  "Science": "bg-green-500/20 border-green-500/40 text-green-600",
  "Hindi": "bg-orange-500/20 border-orange-500/40 text-orange-600",
  "Social Studies": "bg-pink-500/20 border-pink-500/40 text-pink-600",
  "Computer": "bg-cyan-500/20 border-cyan-500/40 text-cyan-600",
  "P.E.": "bg-yellow-500/20 border-yellow-500/40 text-yellow-600",
  "Art": "bg-rose-500/20 border-rose-500/40 text-rose-600",
  "Lunch": "bg-muted border-border text-muted-foreground",
};

const mockTimetable: DaySchedule[] = [
  {
    day: "Monday",
    slots: [
      { subject: "Math", teacher: "Ravi Kumar", room: "Room 101", color: subjectColors["Math"] },
      { subject: "English", teacher: "Priya Singh", room: "Room 102", color: subjectColors["English"] },
      { subject: "Science", teacher: "Anil Mehta", room: "Lab 1", color: subjectColors["Science"] },
      { subject: "Lunch", teacher: "-", room: "-", color: subjectColors["Lunch"] },
      { subject: "Hindi", teacher: "Rohan Verma", room: "Room 101", color: subjectColors["Hindi"] },
      { subject: "Social Studies", teacher: "Neha Singh", room: "Room 103", color: subjectColors["Social Studies"] },
      { subject: "P.E.", teacher: "Mohan Verma", room: "Ground", color: subjectColors["P.E."] },
    ],
  },
  {
    day: "Tuesday",
    slots: [
      { subject: "English", teacher: "Priya Singh", room: "Room 102", color: subjectColors["English"] },
      { subject: "Math", teacher: "Ravi Kumar", room: "Room 101", color: subjectColors["Math"] },
      { subject: "Hindi", teacher: "Rohan Verma", room: "Room 101", color: subjectColors["Hindi"] },
      { subject: "Lunch", teacher: "-", room: "-", color: subjectColors["Lunch"] },
      { subject: "Computer", teacher: "Amit Patel", room: "Lab 2", color: subjectColors["Computer"] },
      { subject: "Science", teacher: "Anil Mehta", room: "Lab 1", color: subjectColors["Science"] },
      { subject: "Art", teacher: "Kavita Joshi", room: "Art Room", color: subjectColors["Art"] },
    ],
  },
  {
    day: "Wednesday",
    slots: [
      { subject: "Science", teacher: "Anil Mehta", room: "Lab 1", color: subjectColors["Science"] },
      { subject: "Math", teacher: "Ravi Kumar", room: "Room 101", color: subjectColors["Math"] },
      { subject: "English", teacher: "Priya Singh", room: "Room 102", color: subjectColors["English"] },
      { subject: "Lunch", teacher: "-", room: "-", color: subjectColors["Lunch"] },
      { subject: "Social Studies", teacher: "Neha Singh", room: "Room 103", color: subjectColors["Social Studies"] },
      { subject: "Hindi", teacher: "Rohan Verma", room: "Room 101", color: subjectColors["Hindi"] },
      { subject: "P.E.", teacher: "Mohan Verma", room: "Ground", color: subjectColors["P.E."] },
    ],
  },
  {
    day: "Thursday",
    slots: [
      { subject: "Hindi", teacher: "Rohan Verma", room: "Room 101", color: subjectColors["Hindi"] },
      { subject: "Science", teacher: "Anil Mehta", room: "Lab 1", color: subjectColors["Science"] },
      { subject: "Math", teacher: "Ravi Kumar", room: "Room 101", color: subjectColors["Math"] },
      { subject: "Lunch", teacher: "-", room: "-", color: subjectColors["Lunch"] },
      { subject: "English", teacher: "Priya Singh", room: "Room 102", color: subjectColors["English"] },
      { subject: "Computer", teacher: "Amit Patel", room: "Lab 2", color: subjectColors["Computer"] },
      { subject: "Social Studies", teacher: "Neha Singh", room: "Room 103", color: subjectColors["Social Studies"] },
    ],
  },
  {
    day: "Friday",
    slots: [
      { subject: "Math", teacher: "Ravi Kumar", room: "Room 101", color: subjectColors["Math"] },
      { subject: "Hindi", teacher: "Rohan Verma", room: "Room 101", color: subjectColors["Hindi"] },
      { subject: "Science", teacher: "Anil Mehta", room: "Lab 1", color: subjectColors["Science"] },
      { subject: "Lunch", teacher: "-", room: "-", color: subjectColors["Lunch"] },
      { subject: "Art", teacher: "Kavita Joshi", room: "Art Room", color: subjectColors["Art"] },
      { subject: "English", teacher: "Priya Singh", room: "Room 102", color: subjectColors["English"] },
      { subject: "P.E.", teacher: "Mohan Verma", room: "Ground", color: subjectColors["P.E."] },
    ],
  },
  {
    day: "Saturday",
    slots: [
      { subject: "English", teacher: "Priya Singh", room: "Room 102", color: subjectColors["English"] },
      { subject: "Math", teacher: "Ravi Kumar", room: "Room 101", color: subjectColors["Math"] },
      { subject: "Social Studies", teacher: "Neha Singh", room: "Room 103", color: subjectColors["Social Studies"] },
      null,
      null,
      null,
      null,
    ],
  },
];

const classNames: Record<string, string> = {
  "1": "Grade 1",
  "2": "Grade 2",
  "3": "Grade 3",
  "4": "Grade 4",
  "5": "Grade 5",
};

const sections = ["A", "B", "C"];

export default function ClassTimetable() {
  const { classId } = useParams();
  const navigate = useNavigate();
  const className = classNames[classId || "1"] || "Grade 1";
  
  const [selectedSection, setSelectedSection] = useState("A");
  const [timetable] = useState<DaySchedule[]>(mockTimetable);

  const handlePrint = () => {
    toast({ title: "Printing", description: "Preparing timetable for print..." });
    window.print();
  };

  const handleDownload = () => {
    toast({ title: "Download", description: "Downloading timetable as PDF..." });
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate("/classes")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Class Timetable</h1>
            <p className="text-muted-foreground mt-1">{className}</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="outline" onClick={handlePrint}>
            <Printer className="mr-2 h-4 w-4" />
            Print
          </Button>
          <Button variant="outline" onClick={handleDownload}>
            <Download className="mr-2 h-4 w-4" />
            Download
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="bg-card border-border">
        <CardContent className="p-4">
          <div className="flex flex-col md:flex-row gap-4 items-end">
            <div className="space-y-2 flex-1">
              <Label>Class</Label>
              <Select defaultValue={classId || "1"} disabled>
                <SelectTrigger className="bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-popover">
                  {Object.entries(classNames).map(([id, name]) => (
                    <SelectItem key={id} value={id}>{name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-2 flex-1">
              <Label>Section</Label>
              <Select value={selectedSection} onValueChange={setSelectedSection}>
                <SelectTrigger className="bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="bg-popover">
                  {sections.map(section => (
                    <SelectItem key={section} value={section}>Section {section}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Timetable Grid */}
      <Card className="bg-card border-border overflow-hidden">
        <CardHeader>
          <CardTitle className="text-foreground flex items-center gap-2">
            <Calendar className="h-5 w-5" />
            Weekly Schedule - {className} (Section {selectedSection})
          </CardTitle>
          <CardDescription>Click on any slot to view details</CardDescription>
        </CardHeader>
        <CardContent className="p-0 overflow-x-auto">
          <table className="w-full min-w-[800px]">
            <thead>
              <tr className="border-b border-border bg-muted/30">
                <th className="p-3 text-left text-muted-foreground font-medium w-28">
                  <div className="flex items-center gap-2">
                    <Clock className="h-4 w-4" />
                    Time
                  </div>
                </th>
                {timetable.map((day) => (
                  <th key={day.day} className="p-3 text-center text-foreground font-medium">
                    {day.day}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {timeSlots.map((time, timeIdx) => (
                <tr key={time} className="border-b border-border">
                  <td className="p-3 text-muted-foreground font-medium bg-muted/20">
                    {time}
                  </td>
                  {timetable.map((day) => {
                    const slot = day.slots[timeIdx];
                    return (
                      <td key={`${day.day}-${time}`} className="p-2">
                        {slot ? (
                          <div className={`p-3 rounded-lg border ${slot.color} transition-transform hover:scale-105 cursor-pointer`}>
                            <div className="font-semibold text-sm">{slot.subject}</div>
                            {slot.subject !== "Lunch" && (
                              <>
                                <div className="text-xs mt-1 opacity-80">{slot.teacher}</div>
                                <div className="text-xs opacity-60">{slot.room}</div>
                              </>
                            )}
                          </div>
                        ) : (
                          <div className="p-3 rounded-lg border border-dashed border-border text-center text-muted-foreground text-xs">
                            -
                          </div>
                        )}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Legend */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle className="text-foreground text-sm">Subject Legend</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-3">
            {Object.entries(subjectColors).map(([subject, color]) => (
              <Badge key={subject} className={`${color} border`}>
                {subject}
              </Badge>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
