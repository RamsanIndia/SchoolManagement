/**
 * Drag-and-Drop Timetable Builder
 * Allows teachers to arrange periods and subjects visually
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Save, RotateCcw, GripVertical, Clock, BookOpen } from "lucide-react";
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
import { toast } from "@/hooks/use-toast";

interface Subject {
  id: string;
  name: string;
  code: string;
  teacher: string;
  color: string;
}

interface TimetableSlot {
  id: string;
  day: string;
  period: number;
  subject: Subject | null;
}

const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
const periods = [1, 2, 3, 4, 5, 6, 7, 8];
const periodTimes = [
  "8:00 - 8:45",
  "8:45 - 9:30",
  "9:45 - 10:30",
  "10:30 - 11:15",
  "11:30 - 12:15",
  "12:15 - 1:00",
  "2:00 - 2:45",
  "2:45 - 3:30",
];

const mockSubjects: Subject[] = [
  { id: "1", name: "English", code: "ENG", teacher: "Priya Singh", color: "bg-blue-500" },
  { id: "2", name: "Mathematics", code: "MTH", teacher: "Ravi Kumar", color: "bg-green-500" },
  { id: "3", name: "Science", code: "SCI", teacher: "Anil Mehta", color: "bg-purple-500" },
  { id: "4", name: "Hindi", code: "HIN", teacher: "Rohan Verma", color: "bg-orange-500" },
  { id: "5", name: "Social Studies", code: "SOC", teacher: "Neha Singh", color: "bg-pink-500" },
  { id: "6", name: "Computer", code: "COM", teacher: "Amit Patel", color: "bg-cyan-500" },
  { id: "7", name: "Physical Ed.", code: "PE", teacher: "Suresh Gupta", color: "bg-yellow-500" },
  { id: "8", name: "Art", code: "ART", teacher: "Kavita Joshi", color: "bg-red-500" },
];

const mockClasses = ["Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5"];
const mockSections = ["A", "B", "C"];

// Generate initial empty timetable
const generateEmptyTimetable = (): TimetableSlot[] => {
  const slots: TimetableSlot[] = [];
  days.forEach(day => {
    periods.forEach(period => {
      slots.push({
        id: `${day}-${period}`,
        day,
        period,
        subject: null,
      });
    });
  });
  return slots;
};

export default function TimetableBuilder() {
  const navigate = useNavigate();
  const [selectedClass, setSelectedClass] = useState("Grade 1");
  const [selectedSection, setSelectedSection] = useState("A");
  const [timetable, setTimetable] = useState<TimetableSlot[]>(generateEmptyTimetable());
  const [draggedSubject, setDraggedSubject] = useState<Subject | null>(null);
  const [dragOverSlot, setDragOverSlot] = useState<string | null>(null);

  const handleDragStart = (subject: Subject) => {
    setDraggedSubject(subject);
  };

  const handleDragOver = (e: React.DragEvent, slotId: string) => {
    e.preventDefault();
    setDragOverSlot(slotId);
  };

  const handleDragLeave = () => {
    setDragOverSlot(null);
  };

  const handleDrop = (slotId: string) => {
    if (draggedSubject) {
      setTimetable(prev =>
        prev.map(slot =>
          slot.id === slotId ? { ...slot, subject: draggedSubject } : slot
        )
      );
    }
    setDraggedSubject(null);
    setDragOverSlot(null);
  };

  const handleSlotClick = (slotId: string) => {
    // Clear slot on click
    setTimetable(prev =>
      prev.map(slot =>
        slot.id === slotId ? { ...slot, subject: null } : slot
      )
    );
  };

  const handleReset = () => {
    setTimetable(generateEmptyTimetable());
    toast({ title: "Reset", description: "Timetable cleared" });
  };

  const handleSave = () => {
    const filledSlots = timetable.filter(s => s.subject).length;
    toast({ 
      title: "Saved", 
      description: `Timetable saved with ${filledSlots} periods assigned` 
    });
  };

  const getSlot = (day: string, period: number) => {
    return timetable.find(s => s.day === day && s.period === period);
  };

  // Calculate subject usage
  const subjectCount = mockSubjects.map(subject => ({
    ...subject,
    count: timetable.filter(s => s.subject?.id === subject.id).length,
  }));

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate("/classes")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Timetable Builder</h1>
            <p className="text-muted-foreground mt-1">Drag and drop subjects to create timetable</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Select value={selectedClass} onValueChange={setSelectedClass}>
            <SelectTrigger className="w-[140px] bg-background">
              <SelectValue />
            </SelectTrigger>
            <SelectContent className="bg-popover">
              {mockClasses.map(c => (
                <SelectItem key={c} value={c}>{c}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={selectedSection} onValueChange={setSelectedSection}>
            <SelectTrigger className="w-[100px] bg-background">
              <SelectValue />
            </SelectTrigger>
            <SelectContent className="bg-popover">
              {mockSections.map(s => (
                <SelectItem key={s} value={s}>Section {s}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button variant="outline" onClick={handleReset}>
            <RotateCcw className="mr-2 h-4 w-4" />
            Reset
          </Button>
          <Button onClick={handleSave} className="bg-primary hover:bg-primary/90">
            <Save className="mr-2 h-4 w-4" />
            Save Timetable
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-12 gap-6">
        {/* Subject Palette */}
        <div className="col-span-3">
          <Card className="bg-card border-border sticky top-6">
            <CardHeader className="pb-3">
              <CardTitle className="text-foreground flex items-center gap-2">
                <BookOpen className="h-5 w-5" />
                Subjects
              </CardTitle>
              <CardDescription>Drag subjects to the timetable grid</CardDescription>
            </CardHeader>
            <CardContent className="space-y-2">
              {subjectCount.map(subject => (
                <div
                  key={subject.id}
                  draggable
                  onDragStart={() => handleDragStart(subject)}
                  className={`p-3 rounded-lg cursor-grab active:cursor-grabbing border border-border bg-muted/50 hover:bg-muted transition-colors ${
                    draggedSubject?.id === subject.id ? "opacity-50 ring-2 ring-primary" : ""
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <GripVertical className="h-4 w-4 text-muted-foreground" />
                    <div className={`w-3 h-3 rounded-full ${subject.color}`} />
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-foreground text-sm truncate">{subject.name}</p>
                      <p className="text-xs text-muted-foreground">{subject.teacher}</p>
                    </div>
                    <Badge variant="secondary" className="text-xs">
                      {subject.count}
                    </Badge>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        </div>

        {/* Timetable Grid */}
        <div className="col-span-9">
          <Card className="bg-card border-border">
            <CardHeader className="pb-3">
              <CardTitle className="text-foreground flex items-center gap-2">
                <Clock className="h-5 w-5" />
                {selectedClass} - Section {selectedSection}
              </CardTitle>
              <CardDescription>Click on a filled slot to remove it</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full border-collapse">
                  <thead>
                    <tr>
                      <th className="p-2 text-left text-muted-foreground text-sm font-medium w-24">
                        Period
                      </th>
                      {days.map(day => (
                        <th key={day} className="p-2 text-center text-foreground text-sm font-medium">
                          {day}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {periods.map((period, idx) => (
                      <tr key={period}>
                        <td className="p-2 border-t border-border">
                          <div className="text-sm font-medium text-foreground">Period {period}</div>
                          <div className="text-xs text-muted-foreground">{periodTimes[idx]}</div>
                        </td>
                        {days.map(day => {
                          const slot = getSlot(day, period);
                          const slotId = `${day}-${period}`;
                          const isOver = dragOverSlot === slotId;
                          
                          return (
                            <td
                              key={slotId}
                              className="p-1 border-t border-border"
                              onDragOver={(e) => handleDragOver(e, slotId)}
                              onDragLeave={handleDragLeave}
                              onDrop={() => handleDrop(slotId)}
                            >
                              <div
                                onClick={() => slot?.subject && handleSlotClick(slotId)}
                                className={`h-20 rounded-lg border-2 border-dashed transition-all ${
                                  slot?.subject
                                    ? `${slot.subject.color} border-transparent cursor-pointer hover:opacity-80`
                                    : isOver
                                    ? "border-primary bg-primary/10"
                                    : "border-border bg-muted/30 hover:border-muted-foreground/50"
                                }`}
                              >
                                {slot?.subject ? (
                                  <div className="h-full p-2 flex flex-col justify-center text-white">
                                    <p className="font-semibold text-sm truncate">{slot.subject.name}</p>
                                    <p className="text-xs opacity-90 truncate">{slot.subject.teacher}</p>
                                    <p className="text-xs opacity-75">{slot.subject.code}</p>
                                  </div>
                                ) : (
                                  <div className="h-full flex items-center justify-center">
                                    <span className="text-xs text-muted-foreground">Drop here</span>
                                  </div>
                                )}
                              </div>
                            </td>
                          );
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
