import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import {
  ArrowRight,
  Users,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Settings,
  GraduationCap,
} from "lucide-react";

const classStats = {
  "5-A": { total: 45, passed: 42, failed: 2, absent: 1 },
  "5-B": { total: 43, passed: 40, failed: 2, absent: 1 },
  "6-A": { total: 48, passed: 45, failed: 2, absent: 1 },
  "6-B": { total: 46, passed: 43, failed: 2, absent: 1 },
};

export default function PromotionSetup() {
  const navigate = useNavigate();
  const [fromYear, setFromYear] = useState("2023-24");
  const [toYear, setToYear] = useState("2024-25");
  const [selectedClass, setSelectedClass] = useState("");
  const [autoPromotion, setAutoPromotion] = useState(true);
  const [minAttendance, setMinAttendance] = useState("75");
  const [minMarks, setMinMarks] = useState("35");

  const stats = selectedClass ? classStats[selectedClass as keyof typeof classStats] : null;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Student Promotion</h1>
          <p className="text-muted-foreground">Promote students to the next academic year</p>
        </div>
      </div>

      {/* Academic Year Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <GraduationCap className="h-5 w-5" />
            Academic Year Configuration
          </CardTitle>
          <CardDescription>Select the academic years for promotion</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4">
            <div className="flex-1 space-y-2">
              <Label>From Academic Year</Label>
              <Select value={fromYear} onValueChange={setFromYear}>
                <SelectTrigger>
                  <SelectValue placeholder="Select year" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="2022-23">2022-23</SelectItem>
                  <SelectItem value="2023-24">2023-24</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <ArrowRight className="h-6 w-6 text-muted-foreground mt-6" />
            <div className="flex-1 space-y-2">
              <Label>To Academic Year</Label>
              <Select value={toYear} onValueChange={setToYear}>
                <SelectTrigger>
                  <SelectValue placeholder="Select year" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="2023-24">2023-24</SelectItem>
                  <SelectItem value="2024-25">2024-25</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Class Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" />
            Select Class & Section
          </CardTitle>
          <CardDescription>Choose the class to promote students from</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {Object.keys(classStats).map((cls) => (
              <Card
                key={cls}
                className={`cursor-pointer transition-all hover:border-primary ${
                  selectedClass === cls ? "border-primary bg-primary/5" : ""
                }`}
                onClick={() => setSelectedClass(cls)}
              >
                <CardContent className="pt-4">
                  <div className="text-center">
                    <p className="font-bold text-lg">Class {cls}</p>
                    <p className="text-sm text-muted-foreground">
                      {classStats[cls as keyof typeof classStats].total} students
                    </p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Stats Display */}
      {stats && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <div className="p-3 rounded-full bg-blue-500/10">
                  <Users className="h-6 w-6 text-blue-500" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Total Students</p>
                  <p className="text-2xl font-bold">{stats.total}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <div className="p-3 rounded-full bg-green-500/10">
                  <CheckCircle className="h-6 w-6 text-green-500" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Passed</p>
                  <p className="text-2xl font-bold text-green-600">{stats.passed}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <div className="p-3 rounded-full bg-red-500/10">
                  <XCircle className="h-6 w-6 text-red-500" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Failed</p>
                  <p className="text-2xl font-bold text-red-600">{stats.failed}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4">
                <div className="p-3 rounded-full bg-orange-500/10">
                  <AlertTriangle className="h-6 w-6 text-orange-500" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground">Special Cases</p>
                  <p className="text-2xl font-bold text-orange-600">{stats.absent}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Promotion Rules */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Promotion Rules
          </CardTitle>
          <CardDescription>Configure automatic promotion criteria</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <Label className="text-base">Auto Promotion</Label>
              <p className="text-sm text-muted-foreground">
                Automatically promote students meeting all criteria
              </p>
            </div>
            <Switch checked={autoPromotion} onCheckedChange={setAutoPromotion} />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2">
              <Label>Minimum Attendance (%)</Label>
              <Input
                type="number"
                value={minAttendance}
                onChange={(e) => setMinAttendance(e.target.value)}
                placeholder="75"
              />
              <p className="text-xs text-muted-foreground">
                Students below this attendance will require manual review
              </p>
            </div>
            <div className="space-y-2">
              <Label>Minimum Passing Marks (%)</Label>
              <Input
                type="number"
                value={minMarks}
                onChange={(e) => setMinMarks(e.target.value)}
                placeholder="35"
              />
              <p className="text-xs text-muted-foreground">
                Minimum percentage required in each subject
              </p>
            </div>
          </div>

          {stats && (
            <div className="border-t pt-4">
              <h4 className="font-medium mb-3">Promotion Preview</h4>
              <div className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span>Eligible for Promotion</span>
                  <span className="font-medium">{stats.passed} / {stats.total}</span>
                </div>
                <Progress value={(stats.passed / stats.total) * 100} className="h-2" />
                <div className="flex gap-4 text-sm">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-green-500" />
                    <span>Promoted ({stats.passed})</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-red-500" />
                    <span>Failed ({stats.failed})</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-orange-500" />
                    <span>Review ({stats.absent})</span>
                  </div>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-end gap-4">
        <Button variant="outline">Save as Draft</Button>
        <Button
          onClick={() => navigate("/students/promotion/mapping")}
          disabled={!selectedClass}
        >
          Proceed to Student Mapping
          <ArrowRight className="h-4 w-4 ml-2" />
        </Button>
      </div>
    </div>
  );
}
