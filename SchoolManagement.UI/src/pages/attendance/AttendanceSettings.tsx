import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Settings, Clock, AlertTriangle } from "lucide-react";
import { useToast } from "@/hooks/use-toast";

export default function AttendanceSettings() {
  const [lateAfter, setLateAfter] = useState("08:30");
  const [autoMarkLate, setAutoMarkLate] = useState(true);
  const [convertToAbsent, setConvertToAbsent] = useState(false);
  const [convertAfterMinutes, setConvertAfterMinutes] = useState(30);
  const { toast } = useToast();

  const handleSave = () => {
    toast({
      title: "Settings Saved",
      description: "Attendance settings have been updated successfully.",
    });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Attendance Settings</h1>
        <p className="text-muted-foreground">Configure late arrival and attendance marking rules</p>
      </div>

      {/* Late Marking Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Late Arrival Settings
          </CardTitle>
          <CardDescription>
            Define when students should be marked as late based on arrival time
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="late-time">Mark Late After Time</Label>
            <Input
              id="late-time"
              type="time"
              value={lateAfter}
              onChange={(e) => setLateAfter(e.target.value)}
              className="max-w-xs"
            />
            <p className="text-sm text-muted-foreground">
              Students arriving after this time will be marked as late
            </p>
          </div>

          <div className="flex items-center justify-between py-3 border-t">
            <div className="space-y-1">
              <Label htmlFor="auto-late">Auto-mark Late</Label>
              <p className="text-sm text-muted-foreground">
                Automatically mark students as late based on check-in time
              </p>
            </div>
            <Switch
              id="auto-late"
              checked={autoMarkLate}
              onCheckedChange={setAutoMarkLate}
            />
          </div>
        </CardContent>
      </Card>

      {/* Absent Conversion Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5" />
            Late to Absent Conversion
          </CardTitle>
          <CardDescription>
            Optionally convert late status to absent after a certain time period
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center justify-between py-3">
            <div className="space-y-1">
              <Label htmlFor="convert-absent">Enable Late to Absent Conversion</Label>
              <p className="text-sm text-muted-foreground">
                Convert late arrivals to absent if they arrive too late
              </p>
            </div>
            <Switch
              id="convert-absent"
              checked={convertToAbsent}
              onCheckedChange={setConvertToAbsent}
            />
          </div>

          {convertToAbsent && (
            <div className="space-y-2 pl-6 border-l-2">
              <Label htmlFor="convert-minutes">Convert After Minutes</Label>
              <Input
                id="convert-minutes"
                type="number"
                value={convertAfterMinutes}
                onChange={(e) => setConvertAfterMinutes(parseInt(e.target.value))}
                className="max-w-xs"
                min="1"
                max="120"
              />
              <p className="text-sm text-muted-foreground">
                Late students will be marked absent if they arrive {convertAfterMinutes} minutes after the late threshold
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Working Hours Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Working Hours
          </CardTitle>
          <CardDescription>
            Set standard school timing for attendance calculations
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid gap-6 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="school-start">School Start Time</Label>
              <Input
                id="school-start"
                type="time"
                defaultValue="08:00"
                className="max-w-xs"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="school-end">School End Time</Label>
              <Input
                id="school-end"
                type="time"
                defaultValue="15:30"
                className="max-w-xs"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Current Configuration Summary */}
      <Card className="border-2 bg-muted/20">
        <CardHeader>
          <CardTitle>Current Configuration Summary</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div className="flex justify-between items-center py-2 border-b">
              <span className="font-medium">Late After:</span>
              <span className="text-muted-foreground">{lateAfter} AM</span>
            </div>
            <div className="flex justify-between items-center py-2 border-b">
              <span className="font-medium">Auto-mark Late:</span>
              <span className="text-muted-foreground">{autoMarkLate ? "Enabled" : "Disabled"}</span>
            </div>
            <div className="flex justify-between items-center py-2 border-b">
              <span className="font-medium">Late to Absent Conversion:</span>
              <span className="text-muted-foreground">{convertToAbsent ? "Enabled" : "Disabled"}</span>
            </div>
            {convertToAbsent && (
              <div className="flex justify-between items-center py-2">
                <span className="font-medium">Conversion Time:</span>
                <span className="text-muted-foreground">{convertAfterMinutes} minutes</span>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Save Button */}
      <div className="flex justify-end">
        <Button onClick={handleSave} size="lg" className="bg-gradient-primary">
          Save All Settings
        </Button>
      </div>
    </div>
  );
}
