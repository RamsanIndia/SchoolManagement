import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";

const mockMonthlyData = [
  { date: "2026-01-01", present: 780, absent: 60, late: 10, holiday: false },
  { date: "2026-01-02", present: 770, absent: 70, late: 10, holiday: false },
  { date: "2026-01-03", present: 790, absent: 50, late: 10, holiday: false },
  { date: "2026-01-04", present: 760, absent: 80, late: 10, holiday: false },
  { date: "2026-01-05", present: 0, absent: 0, late: 0, holiday: true },
  { date: "2026-01-06", present: 785, absent: 55, late: 10, holiday: false },
  { date: "2026-01-07", present: 780, absent: 60, late: 10, holiday: false },
  { date: "2026-01-08", present: 775, absent: 65, late: 10, holiday: false },
  { date: "2026-01-09", present: 790, absent: 50, late: 10, holiday: false },
  { date: "2026-01-10", present: 765, absent: 75, late: 10, holiday: false },
];

export default function MonthlyCalendar() {
  const [currentDate, setCurrentDate] = useState(new Date(2026, 0, 1));
  const [selectedDate, setSelectedDate] = useState<typeof mockMonthlyData[0] | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const getDaysInMonth = (date: Date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    return { daysInMonth, startingDayOfWeek };
  };

  const getAttendanceForDate = (day: number) => {
    const dateStr = `${currentDate.getFullYear()}-${String(currentDate.getMonth() + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    return mockMonthlyData.find(d => d.date === dateStr);
  };

  const { daysInMonth, startingDayOfWeek } = getDaysInMonth(currentDate);

  const previousMonth = () => {
    setCurrentDate(new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1));
  };

  const nextMonth = () => {
    setCurrentDate(new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1));
  };

  const handleDateClick = (day: number) => {
    const data = getAttendanceForDate(day);
    if (data) {
      setSelectedDate(data);
      setIsDialogOpen(true);
    }
  };

  const getStatusDot = (data: typeof mockMonthlyData[0] | undefined) => {
    if (!data) return null;
    if (data.holiday) return <div className="w-2 h-2 rounded-full bg-yellow-500 mx-auto" />;
    
    const total = data.present + data.absent + data.late;
    const presentPercentage = (data.present / total) * 100;
    
    if (presentPercentage >= 90) return <div className="w-2 h-2 rounded-full bg-status-present mx-auto" />;
    if (presentPercentage >= 75) return <div className="w-2 h-2 rounded-full bg-status-late mx-auto" />;
    return <div className="w-2 h-2 rounded-full bg-status-absent mx-auto" />;
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Monthly Attendance Calendar</h1>
        <p className="text-muted-foreground">Visual representation of daily attendance trends</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>
              {currentDate.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}
            </CardTitle>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={previousMonth}>
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button variant="outline" size="sm" onClick={nextMonth}>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {/* Legend */}
          <div className="flex gap-6 mb-6 flex-wrap">
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-status-present" />
              <span className="text-sm">Present (&gt;90%)</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-status-late" />
              <span className="text-sm">Late (75-90%)</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-status-absent" />
              <span className="text-sm">Absent (&lt;75%)</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-yellow-500" />
              <span className="text-sm">Holiday</span>
            </div>
          </div>

          {/* Calendar Grid */}
          <div className="grid grid-cols-7 gap-2">
            {/* Day headers */}
            {['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map((day) => (
              <div key={day} className="text-center font-semibold text-sm py-2 text-muted-foreground">
                {day}
              </div>
            ))}

            {/* Empty cells for days before month starts */}
            {Array.from({ length: startingDayOfWeek }).map((_, index) => (
              <div key={`empty-${index}`} className="aspect-square" />
            ))}

            {/* Days of the month */}
            {Array.from({ length: daysInMonth }).map((_, index) => {
              const day = index + 1;
              const data = getAttendanceForDate(day);
              return (
                <div
                  key={day}
                  onClick={() => handleDateClick(day)}
                  className="aspect-square border rounded-lg p-2 hover:bg-muted/50 cursor-pointer transition-colors flex flex-col items-center justify-center"
                >
                  <span className="text-sm font-medium mb-1">{day}</span>
                  {getStatusDot(data)}
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Detail Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Attendance Details</DialogTitle>
          </DialogHeader>
          {selectedDate && (
            <div className="space-y-4">
              <div className="text-center py-4">
                <p className="text-2xl font-bold">
                  {new Date(selectedDate.date).toLocaleDateString('en-US', { 
                    weekday: 'long',
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric'
                  })}
                </p>
              </div>

              {selectedDate.holiday ? (
                <div className="text-center py-8">
                  <Badge className="bg-yellow-500 text-white text-lg px-4 py-2">Holiday</Badge>
                </div>
              ) : (
                <div className="grid gap-4">
                  <div className="grid grid-cols-2 gap-4">
                    <Card>
                      <CardContent className="pt-6">
                        <div className="text-center">
                          <p className="text-sm text-muted-foreground mb-2">Present</p>
                          <p className="text-3xl font-bold text-status-present">{selectedDate.present}</p>
                        </div>
                      </CardContent>
                    </Card>
                    <Card>
                      <CardContent className="pt-6">
                        <div className="text-center">
                          <p className="text-sm text-muted-foreground mb-2">Absent</p>
                          <p className="text-3xl font-bold text-status-absent">{selectedDate.absent}</p>
                        </div>
                      </CardContent>
                    </Card>
                  </div>
                  <Card>
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <p className="text-sm text-muted-foreground mb-2">Late</p>
                        <p className="text-3xl font-bold text-status-late">{selectedDate.late}</p>
                      </div>
                    </CardContent>
                  </Card>
                  <Card className="bg-gradient-primary">
                    <CardContent className="pt-6">
                      <div className="text-center">
                        <p className="text-sm text-white/80 mb-2">Attendance Rate</p>
                        <p className="text-3xl font-bold text-white">
                          {(((selectedDate.present + selectedDate.late) / (selectedDate.present + selectedDate.absent + selectedDate.late)) * 100).toFixed(1)}%
                        </p>
                      </div>
                    </CardContent>
                  </Card>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
