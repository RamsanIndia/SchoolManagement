import { useState } from "react";
import { Calendar } from "@/components/ui/calendar";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { CalendarIcon, Plus, MapPin } from "lucide-react";
import { format } from "date-fns";
import { useToast } from "@/hooks/use-toast";

interface CalendarEvent {
  id: string;
  title: string;
  date: Date;
  type: "holiday" | "exam" | "event" | "vacation";
  description?: string;
  location?: string;
}

const mockEvents: CalendarEvent[] = [
  { id: "1", title: "New Year's Day", date: new Date(2025, 0, 1), type: "holiday", description: "Public Holiday" },
  { id: "2", title: "Republic Day", date: new Date(2025, 0, 26), type: "holiday", description: "Public Holiday" },
  { id: "3", title: "Mid-Term Exams Begin", date: new Date(2025, 1, 15), type: "exam", location: "Main Building" },
  { id: "4", title: "Holi", date: new Date(2025, 2, 14), type: "holiday", description: "Festival Holiday" },
  { id: "5", title: "Spring Break", date: new Date(2025, 2, 20), type: "vacation", description: "One week break" },
  { id: "6", title: "Independence Day", date: new Date(2025, 7, 15), type: "holiday", description: "Public Holiday" },
  { id: "7", title: "Annual Sports Day", date: new Date(2025, 8, 5), type: "event", location: "Sports Ground" },
  { id: "8", title: "Diwali", date: new Date(2025, 9, 20), type: "holiday", description: "Festival Holiday" },
  { id: "9", title: "Final Exams", date: new Date(2025, 10, 15), type: "exam", location: "All Classrooms" },
  { id: "10", title: "Christmas", date: new Date(2025, 11, 25), type: "holiday", description: "Public Holiday" },
];

const AcademicCalendar = () => {
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(new Date());
  const [events, setEvents] = useState<CalendarEvent[]>(mockEvents);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const { toast } = useToast();

  const getEventTypeColor = (type: string) => {
    switch (type) {
      case "holiday": return "bg-red-500/10 text-red-700 dark:text-red-400 border-red-500/20";
      case "exam": return "bg-orange-500/10 text-orange-700 dark:text-orange-400 border-orange-500/20";
      case "event": return "bg-blue-500/10 text-blue-700 dark:text-blue-400 border-blue-500/20";
      case "vacation": return "bg-green-500/10 text-green-700 dark:text-green-400 border-green-500/20";
      default: return "bg-muted text-muted-foreground";
    }
  };

  const getEventsForDate = (date: Date) => {
    return events.filter(event => 
      event.date.toDateString() === date.toDateString()
    );
  };

  const upcomingEvents = events
    .filter(event => event.date >= new Date())
    .sort((a, b) => a.date.getTime() - b.date.getTime())
    .slice(0, 5);

  const handleAddEvent = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const newEvent: CalendarEvent = {
      id: String(events.length + 1),
      title: formData.get("title") as string,
      date: new Date(formData.get("date") as string),
      type: formData.get("type") as CalendarEvent["type"],
      description: formData.get("description") as string,
      location: formData.get("location") as string,
    };
    setEvents([...events, newEvent]);
    setIsAddDialogOpen(false);
    toast({
      title: "Event Added",
      description: "New event has been added to the calendar.",
    });
  };

  const modifiers = {
    holiday: events.filter(e => e.type === "holiday").map(e => e.date),
    exam: events.filter(e => e.type === "exam").map(e => e.date),
    event: events.filter(e => e.type === "event").map(e => e.date),
    vacation: events.filter(e => e.type === "vacation").map(e => e.date),
  };

  const modifiersStyles = {
    holiday: { backgroundColor: "hsl(var(--destructive) / 0.2)", color: "hsl(var(--destructive))" },
    exam: { backgroundColor: "hsl(var(--warning) / 0.2)", color: "hsl(var(--warning))" },
    event: { backgroundColor: "hsl(var(--primary) / 0.2)", color: "hsl(var(--primary))" },
    vacation: { backgroundColor: "hsl(var(--success) / 0.2)", color: "hsl(var(--success))" },
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Academic Calendar</h1>
          <p className="text-muted-foreground">View holidays, events, and important dates</p>
        </div>
        <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Add Event
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Add New Event</DialogTitle>
              <DialogDescription>Add a new event to the academic calendar</DialogDescription>
            </DialogHeader>
            <form onSubmit={handleAddEvent} className="space-y-4">
              <div>
                <Label htmlFor="title">Event Title</Label>
                <Input id="title" name="title" placeholder="Enter event title" required />
              </div>
              <div>
                <Label htmlFor="date">Date</Label>
                <Input id="date" name="date" type="date" required />
              </div>
              <div>
                <Label htmlFor="type">Event Type</Label>
                <Select name="type" required>
                  <SelectTrigger>
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="holiday">Public Holiday</SelectItem>
                    <SelectItem value="exam">Examination</SelectItem>
                    <SelectItem value="event">Event</SelectItem>
                    <SelectItem value="vacation">Vacation</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label htmlFor="location">Location (Optional)</Label>
                <Input id="location" name="location" placeholder="Event location" />
              </div>
              <div>
                <Label htmlFor="description">Description (Optional)</Label>
                <Textarea id="description" name="description" placeholder="Event description" />
              </div>
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                  Cancel
                </Button>
                <Button type="submit">Add Event</Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Calendar View</CardTitle>
            <CardDescription>
              Click on a date to view events
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex justify-center">
              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={setSelectedDate}
                className="rounded-md border"
                modifiers={modifiers}
                modifiersStyles={modifiersStyles}
              />
            </div>
            {selectedDate && (
              <div className="mt-6">
                <h3 className="font-semibold mb-3">
                  Events on {format(selectedDate, "MMMM dd, yyyy")}
                </h3>
                {getEventsForDate(selectedDate).length > 0 ? (
                  <div className="space-y-2">
                    {getEventsForDate(selectedDate).map((event) => (
                      <div key={event.id} className="flex items-start gap-3 p-3 border rounded-lg">
                        <CalendarIcon className="h-5 w-5 text-muted-foreground mt-0.5" />
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <p className="font-medium">{event.title}</p>
                            <Badge variant="outline" className={getEventTypeColor(event.type)}>
                              {event.type}
                            </Badge>
                          </div>
                          {event.description && (
                            <p className="text-sm text-muted-foreground">{event.description}</p>
                          )}
                          {event.location && (
                            <div className="flex items-center gap-1 text-sm text-muted-foreground mt-1">
                              <MapPin className="h-3 w-3" />
                              {event.location}
                            </div>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No events scheduled for this date.</p>
                )}
              </div>
            )}
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Legend</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <div className="flex items-center gap-2">
                <div className="w-4 h-4 rounded-full bg-red-500/20 border border-red-500/20" />
                <span className="text-sm">Public Holidays</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-4 h-4 rounded-full bg-orange-500/20 border border-orange-500/20" />
                <span className="text-sm">Examinations</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-4 h-4 rounded-full bg-blue-500/20 border border-blue-500/20" />
                <span className="text-sm">Events</span>
              </div>
              <div className="flex items-center gap-2">
                <div className="w-4 h-4 rounded-full bg-green-500/20 border border-green-500/20" />
                <span className="text-sm">Vacations</span>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Upcoming Events</CardTitle>
              <CardDescription>Next 5 scheduled events</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {upcomingEvents.map((event) => (
                  <div key={event.id} className="flex items-start gap-3 pb-3 border-b last:border-0">
                    <div className="text-center min-w-[48px]">
                      <p className="text-2xl font-bold">{format(event.date, "dd")}</p>
                      <p className="text-xs text-muted-foreground uppercase">{format(event.date, "MMM")}</p>
                    </div>
                    <div className="flex-1">
                      <p className="font-medium text-sm">{event.title}</p>
                      <Badge variant="outline" className={`${getEventTypeColor(event.type)} text-xs mt-1`}>
                        {event.type}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};

export default AcademicCalendar;
