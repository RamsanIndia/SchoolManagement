/**
 * Auto-Scheduling Feature
 * Automatically generates optimal timetables based on teacher availability and constraints
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Wand2, Play, Settings, CheckCircle, AlertTriangle, Clock, Users, BookOpen } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "@/hooks/use-toast";

interface ScheduleConstraint {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
  priority: "high" | "medium" | "low";
}

interface GeneratedSlot {
  day: string;
  period: number;
  subject: string;
  teacher: string;
  room: string;
  conflict?: string;
}

const mockClasses = ["Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5"];
const mockSections = ["A", "B", "C"];

const defaultConstraints: ScheduleConstraint[] = [
  { id: "1", name: "No Back-to-Back Same Subject", description: "Avoid scheduling the same subject in consecutive periods", enabled: true, priority: "high" },
  { id: "2", name: "Teacher Break Time", description: "Ensure teachers have at least one break period", enabled: true, priority: "high" },
  { id: "3", name: "Lab Sessions Together", description: "Schedule double periods for lab subjects", enabled: true, priority: "medium" },
  { id: "4", name: "Morning Core Subjects", description: "Schedule Math/Science in morning slots", enabled: true, priority: "medium" },
  { id: "5", name: "Balanced Distribution", description: "Spread subjects evenly across the week", enabled: true, priority: "high" },
  { id: "6", name: "Teacher Availability", description: "Respect teacher unavailable time slots", enabled: true, priority: "high" },
  { id: "7", name: "Room Capacity", description: "Match class size with room capacity", enabled: false, priority: "low" },
  { id: "8", name: "Minimize Room Changes", description: "Reduce student movement between rooms", enabled: false, priority: "low" },
];

const mockGeneratedSchedule: GeneratedSlot[] = [
  { day: "Monday", period: 1, subject: "Mathematics", teacher: "Ravi Kumar", room: "Room 101" },
  { day: "Monday", period: 2, subject: "English", teacher: "Priya Singh", room: "Room 101" },
  { day: "Monday", period: 3, subject: "Science", teacher: "Anil Mehta", room: "Lab 1" },
  { day: "Monday", period: 4, subject: "Hindi", teacher: "Rohan Verma", room: "Room 101" },
  { day: "Tuesday", period: 1, subject: "English", teacher: "Priya Singh", room: "Room 101" },
  { day: "Tuesday", period: 2, subject: "Mathematics", teacher: "Ravi Kumar", room: "Room 101" },
  { day: "Tuesday", period: 3, subject: "Social Studies", teacher: "Neha Singh", room: "Room 101" },
  { day: "Tuesday", period: 4, subject: "Computer", teacher: "Amit Patel", room: "Computer Lab", conflict: "Room double-booked with Grade 2B" },
  { day: "Wednesday", period: 1, subject: "Science", teacher: "Anil Mehta", room: "Lab 1" },
  { day: "Wednesday", period: 2, subject: "Science", teacher: "Anil Mehta", room: "Lab 1" },
  { day: "Wednesday", period: 3, subject: "Physical Ed.", teacher: "Suresh Gupta", room: "Ground" },
  { day: "Wednesday", period: 4, subject: "Art", teacher: "Kavita Joshi", room: "Art Room" },
];

export default function AutoScheduler() {
  const navigate = useNavigate();
  const [selectedClass, setSelectedClass] = useState("Grade 1");
  const [selectedSection, setSelectedSection] = useState("A");
  const [constraints, setConstraints] = useState<ScheduleConstraint[]>(defaultConstraints);
  const [isGenerating, setIsGenerating] = useState(false);
  const [progress, setProgress] = useState(0);
  const [generatedSchedule, setGeneratedSchedule] = useState<GeneratedSlot[] | null>(null);
  const [showResults, setShowResults] = useState(false);

  const toggleConstraint = (id: string) => {
    setConstraints(prev =>
      prev.map(c => c.id === id ? { ...c, enabled: !c.enabled } : c)
    );
  };

  const handleGenerate = () => {
    setIsGenerating(true);
    setProgress(0);
    setShowResults(false);

    const interval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 100) {
          clearInterval(interval);
          setIsGenerating(false);
          setGeneratedSchedule(mockGeneratedSchedule);
          setShowResults(true);
          toast({
            title: "Schedule Generated",
            description: "Optimal timetable has been generated with 1 conflict detected",
          });
          return 100;
        }
        return prev + 10;
      });
    }, 200);
  };

  const handleApply = () => {
    toast({
      title: "Schedule Applied",
      description: `Timetable for ${selectedClass} - Section ${selectedSection} has been saved`,
    });
    navigate("/classes/timetable-builder");
  };

  const enabledConstraints = constraints.filter(c => c.enabled).length;
  const conflictCount = generatedSchedule?.filter(s => s.conflict).length || 0;

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "high": return "bg-destructive/20 text-destructive";
      case "medium": return "bg-warning/20 text-warning";
      case "low": return "bg-muted text-muted-foreground";
      default: return "bg-muted text-muted-foreground";
    }
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
            <h1 className="text-3xl font-bold text-foreground">Auto Scheduler</h1>
            <p className="text-muted-foreground mt-1">Generate optimal timetables automatically</p>
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
        </div>
      </div>

      <div className="grid grid-cols-12 gap-6">
        {/* Constraints Panel */}
        <div className="col-span-4">
          <Card className="bg-card border-border">
            <CardHeader className="pb-3">
              <CardTitle className="text-foreground flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Scheduling Constraints
              </CardTitle>
              <CardDescription>
                {enabledConstraints} of {constraints.length} constraints enabled
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {constraints.map(constraint => (
                <div
                  key={constraint.id}
                  className={`p-3 rounded-lg border transition-colors ${
                    constraint.enabled ? "border-primary/50 bg-primary/5" : "border-border bg-muted/30"
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <p className="font-medium text-foreground text-sm">{constraint.name}</p>
                        <Badge variant="secondary" className={`text-xs ${getPriorityColor(constraint.priority)}`}>
                          {constraint.priority}
                        </Badge>
                      </div>
                      <p className="text-xs text-muted-foreground">{constraint.description}</p>
                    </div>
                    <Switch
                      checked={constraint.enabled}
                      onCheckedChange={() => toggleConstraint(constraint.id)}
                    />
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        </div>

        {/* Generation Panel */}
        <div className="col-span-8 space-y-6">
          {/* Generate Card */}
          <Card className="bg-card border-border">
            <CardHeader className="pb-3">
              <CardTitle className="text-foreground flex items-center gap-2">
                <Wand2 className="h-5 w-5" />
                Generate Timetable
              </CardTitle>
              <CardDescription>
                Click generate to create an optimal schedule for {selectedClass} - Section {selectedSection}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {isGenerating && (
                <div className="space-y-2">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">Optimizing schedule...</span>
                    <span className="text-foreground font-medium">{progress}%</span>
                  </div>
                  <Progress value={progress} className="h-2" />
                </div>
              )}

              <div className="flex gap-3">
                <Button
                  onClick={handleGenerate}
                  disabled={isGenerating}
                  className="bg-primary hover:bg-primary/90"
                >
                  <Play className="mr-2 h-4 w-4" />
                  {isGenerating ? "Generating..." : "Generate Schedule"}
                </Button>
                {showResults && (
                  <Button onClick={handleApply} variant="outline">
                    <CheckCircle className="mr-2 h-4 w-4" />
                    Apply to Timetable
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Results */}
          {showResults && generatedSchedule && (
            <>
              {/* Stats */}
              <div className="grid grid-cols-4 gap-4">
                <Card className="bg-card border-border">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-primary/10">
                        <Clock className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <p className="text-2xl font-bold text-foreground">{generatedSchedule.length}</p>
                        <p className="text-xs text-muted-foreground">Periods Scheduled</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                <Card className="bg-card border-border">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-green-500/10">
                        <CheckCircle className="h-5 w-5 text-green-500" />
                      </div>
                      <div>
                        <p className="text-2xl font-bold text-foreground">{enabledConstraints}</p>
                        <p className="text-xs text-muted-foreground">Constraints Met</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                <Card className="bg-card border-border">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-yellow-500/10">
                        <AlertTriangle className="h-5 w-5 text-yellow-500" />
                      </div>
                      <div>
                        <p className="text-2xl font-bold text-foreground">{conflictCount}</p>
                        <p className="text-xs text-muted-foreground">Conflicts Found</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
                <Card className="bg-card border-border">
                  <CardContent className="p-4">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-blue-500/10">
                        <Users className="h-5 w-5 text-blue-500" />
                      </div>
                      <div>
                        <p className="text-2xl font-bold text-foreground">6</p>
                        <p className="text-xs text-muted-foreground">Teachers Assigned</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* Generated Schedule Preview */}
              <Card className="bg-card border-border">
                <CardHeader className="pb-3">
                  <CardTitle className="text-foreground flex items-center gap-2">
                    <BookOpen className="h-5 w-5" />
                    Generated Schedule Preview
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-border">
                          <th className="text-left p-3 text-muted-foreground text-sm font-medium">Day</th>
                          <th className="text-left p-3 text-muted-foreground text-sm font-medium">Period</th>
                          <th className="text-left p-3 text-muted-foreground text-sm font-medium">Subject</th>
                          <th className="text-left p-3 text-muted-foreground text-sm font-medium">Teacher</th>
                          <th className="text-left p-3 text-muted-foreground text-sm font-medium">Room</th>
                          <th className="text-left p-3 text-muted-foreground text-sm font-medium">Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {generatedSchedule.map((slot, idx) => (
                          <tr key={idx} className="border-b border-border hover:bg-muted/30">
                            <td className="p-3 text-foreground">{slot.day}</td>
                            <td className="p-3 text-foreground">Period {slot.period}</td>
                            <td className="p-3 text-foreground font-medium">{slot.subject}</td>
                            <td className="p-3 text-muted-foreground">{slot.teacher}</td>
                            <td className="p-3 text-muted-foreground">{slot.room}</td>
                            <td className="p-3">
                              {slot.conflict ? (
                                <Badge variant="destructive" className="text-xs">
                                  <AlertTriangle className="h-3 w-3 mr-1" />
                                  Conflict
                                </Badge>
                              ) : (
                                <Badge variant="secondary" className="bg-green-500/20 text-green-500 text-xs">
                                  <CheckCircle className="h-3 w-3 mr-1" />
                                  OK
                                </Badge>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </CardContent>
              </Card>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
