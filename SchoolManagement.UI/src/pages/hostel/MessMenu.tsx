import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  Utensils, 
  Plus, 
  Edit,
  Calendar,
  Clock,
  Coffee,
  Sun,
  Sunset,
  Moon,
  AlertCircle,
  Leaf
} from "lucide-react";

const MessMenu = () => {
  const [selectedWeek, setSelectedWeek] = useState("current");

  const weeklyMenu = [
    {
      day: "Monday",
      meals: {
        breakfast: { items: "Idli, Sambar, Chutney, Tea/Coffee", time: "7:30 - 9:00 AM" },
        lunch: { items: "Rice, Dal Tadka, Aloo Gobi, Roti, Salad, Buttermilk", time: "12:30 - 2:00 PM" },
        snacks: { items: "Veg Sandwich, Tea", time: "5:00 - 6:00 PM" },
        dinner: { items: "Jeera Rice, Kadhi, Mixed Veg, Chapati, Sweet", time: "7:30 - 9:00 PM" }
      }
    },
    {
      day: "Tuesday",
      meals: {
        breakfast: { items: "Poha, Jalebi, Milk, Fruits", time: "7:30 - 9:00 AM" },
        lunch: { items: "Rice, Rajma, Bhindi Fry, Roti, Salad, Lassi", time: "12:30 - 2:00 PM" },
        snacks: { items: "Samosa, Green Chutney, Tea", time: "5:00 - 6:00 PM" },
        dinner: { items: "Pulao, Paneer Butter Masala, Raita, Chapati", time: "7:30 - 9:00 PM" }
      }
    },
    {
      day: "Wednesday",
      meals: {
        breakfast: { items: "Paratha, Curd, Pickle, Tea/Coffee", time: "7:30 - 9:00 AM" },
        lunch: { items: "Rice, Chole, Aloo Jeera, Roti, Salad, Chaas", time: "12:30 - 2:00 PM" },
        snacks: { items: "Bread Pakora, Tea", time: "5:00 - 6:00 PM" },
        dinner: { items: "Rice, Dal Makhani, Palak Paneer, Chapati", time: "7:30 - 9:00 PM" }
      }
    },
    {
      day: "Thursday",
      meals: {
        breakfast: { items: "Upma, Coconut Chutney, Banana, Tea/Coffee", time: "7:30 - 9:00 AM" },
        lunch: { items: "Rice, Sambhar, Veg Korma, Roti, Salad, Buttermilk", time: "12:30 - 2:00 PM" },
        snacks: { items: "Bhel Puri, Tea", time: "5:00 - 6:00 PM" },
        dinner: { items: "Biryani, Raita, Mirchi Ka Salan, Gulab Jamun", time: "7:30 - 9:00 PM" }
      }
    },
    {
      day: "Friday",
      meals: {
        breakfast: { items: "Chole Bhature, Lassi", time: "7:30 - 9:00 AM" },
        lunch: { items: "Rice, Moong Dal, Cabbage Sabzi, Roti, Salad", time: "12:30 - 2:00 PM" },
        snacks: { items: "Pav Bhaji, Tea", time: "5:00 - 6:00 PM" },
        dinner: { items: "Rice, Kadi Pakora, Aloo Matar, Chapati", time: "7:30 - 9:00 PM" }
      }
    },
    {
      day: "Saturday",
      meals: {
        breakfast: { items: "Dosa, Sambar, Chutney, Coffee", time: "7:30 - 9:00 AM" },
        lunch: { items: "Rice, Dal Fry, Baingan Bharta, Roti, Salad", time: "12:30 - 2:00 PM" },
        snacks: { items: "Spring Roll, Tea", time: "5:00 - 6:00 PM" },
        dinner: { items: "Puri, Chana Masala, Halwa", time: "7:30 - 9:00 PM" }
      }
    },
    {
      day: "Sunday",
      meals: {
        breakfast: { items: "Puri Bhaji, Kheer, Fruits", time: "8:00 - 10:00 AM" },
        lunch: { items: "Special Veg Biryani, Raita, Papad, Sweet", time: "12:30 - 2:30 PM" },
        snacks: { items: "Pasta, Cold Drink", time: "5:00 - 6:00 PM" },
        dinner: { items: "Rice, Paneer Tikka Masala, Dal, Chapati, Ice Cream", time: "7:30 - 9:00 PM" }
      }
    },
  ];

  const specialDiets = [
    { id: 1, student: "Rohan Sharma", room: "A-101", type: "Jain", notes: "No onion, garlic, root vegetables" },
    { id: 2, student: "Priya Patel", room: "C-102", type: "Diabetic", notes: "Low sugar, controlled portions" },
    { id: 3, student: "Aisha Khan", room: "C-105", type: "Gluten-Free", notes: "No wheat products" },
    { id: 4, student: "Vikram Singh", room: "A-203", type: "High Protein", notes: "Extra protein portions for sports" },
  ];

  const mealFeedback = [
    { id: 1, date: "2024-01-15", meal: "Lunch", rating: 4.2, comments: 28, topFeedback: "Dal was excellent" },
    { id: 2, date: "2024-01-15", meal: "Dinner", rating: 3.8, comments: 15, topFeedback: "Need more variety" },
    { id: 3, date: "2024-01-14", meal: "Breakfast", rating: 4.5, comments: 22, topFeedback: "Loved the parathas" },
    { id: 4, date: "2024-01-14", meal: "Lunch", rating: 4.0, comments: 18, topFeedback: "Good quality" },
  ];

  const getMealIcon = (meal: string) => {
    switch (meal) {
      case "breakfast": return <Coffee className="h-4 w-4" />;
      case "lunch": return <Sun className="h-4 w-4" />;
      case "snacks": return <Sunset className="h-4 w-4" />;
      case "dinner": return <Moon className="h-4 w-4" />;
      default: return <Utensils className="h-4 w-4" />;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Mess Menu</h1>
          <p className="text-muted-foreground">Manage weekly menu and special dietary requirements</p>
        </div>
        <div className="flex gap-2">
          <Select value={selectedWeek} onValueChange={setSelectedWeek}>
            <SelectTrigger className="w-48">
              <Calendar className="h-4 w-4 mr-2" />
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="current">This Week (Jan 15-21)</SelectItem>
              <SelectItem value="next">Next Week (Jan 22-28)</SelectItem>
              <SelectItem value="prev">Previous Week (Jan 8-14)</SelectItem>
            </SelectContent>
          </Select>
          <Dialog>
            <DialogTrigger asChild>
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Update Menu
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl">
              <DialogHeader>
                <DialogTitle>Update Menu</DialogTitle>
              </DialogHeader>
              <div className="space-y-4 pt-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Day</Label>
                    <Select>
                      <SelectTrigger>
                        <SelectValue placeholder="Select day" />
                      </SelectTrigger>
                      <SelectContent>
                        {["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].map(day => (
                          <SelectItem key={day} value={day.toLowerCase()}>{day}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Meal</Label>
                    <Select>
                      <SelectTrigger>
                        <SelectValue placeholder="Select meal" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="breakfast">Breakfast</SelectItem>
                        <SelectItem value="lunch">Lunch</SelectItem>
                        <SelectItem value="snacks">Snacks</SelectItem>
                        <SelectItem value="dinner">Dinner</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="space-y-2">
                  <Label>Menu Items</Label>
                  <Textarea placeholder="Enter menu items separated by commas..." rows={3} />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Start Time</Label>
                    <Input type="time" />
                  </div>
                  <div className="space-y-2">
                    <Label>End Time</Label>
                    <Input type="time" />
                  </div>
                </div>
                <Button className="w-full">Update Menu</Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <Tabs defaultValue="weekly" className="space-y-4">
        <TabsList>
          <TabsTrigger value="weekly">
            <Calendar className="h-4 w-4 mr-2" />
            Weekly Menu
          </TabsTrigger>
          <TabsTrigger value="special">
            <Leaf className="h-4 w-4 mr-2" />
            Special Diets
          </TabsTrigger>
          <TabsTrigger value="feedback">
            <AlertCircle className="h-4 w-4 mr-2" />
            Feedback
          </TabsTrigger>
        </TabsList>

        <TabsContent value="weekly">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            {weeklyMenu.map((dayMenu, index) => (
              <Card key={index}>
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">{dayMenu.day}</CardTitle>
                    <Button variant="ghost" size="icon">
                      <Edit className="h-4 w-4" />
                    </Button>
                  </div>
                </CardHeader>
                <CardContent className="space-y-3">
                  {Object.entries(dayMenu.meals).map(([mealType, meal]) => (
                    <div key={mealType} className="flex items-start gap-3 p-3 rounded-lg bg-muted/50">
                      <div className={`p-2 rounded-full ${
                        mealType === 'breakfast' ? 'bg-amber-100 text-amber-600' :
                        mealType === 'lunch' ? 'bg-orange-100 text-orange-600' :
                        mealType === 'snacks' ? 'bg-purple-100 text-purple-600' :
                        'bg-blue-100 text-blue-600'
                      }`}>
                        {getMealIcon(mealType)}
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center justify-between">
                          <span className="font-medium capitalize">{mealType}</span>
                          <span className="text-xs text-muted-foreground flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            {meal.time}
                          </span>
                        </div>
                        <p className="text-sm text-muted-foreground mt-1">{meal.items}</p>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="special">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Special Dietary Requirements</CardTitle>
                <Dialog>
                  <DialogTrigger asChild>
                    <Button>
                      <Plus className="h-4 w-4 mr-2" />
                      Add Special Diet
                    </Button>
                  </DialogTrigger>
                  <DialogContent>
                    <DialogHeader>
                      <DialogTitle>Add Special Dietary Requirement</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 pt-4">
                      <div className="space-y-2">
                        <Label>Student</Label>
                        <Select>
                          <SelectTrigger>
                            <SelectValue placeholder="Select student" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="s1">Amit Kumar - A-102</SelectItem>
                            <SelectItem value="s2">Neha Singh - C-105</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Diet Type</Label>
                        <Select>
                          <SelectTrigger>
                            <SelectValue placeholder="Select diet type" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="jain">Jain</SelectItem>
                            <SelectItem value="vegan">Vegan</SelectItem>
                            <SelectItem value="gluten-free">Gluten-Free</SelectItem>
                            <SelectItem value="diabetic">Diabetic</SelectItem>
                            <SelectItem value="high-protein">High Protein</SelectItem>
                            <SelectItem value="low-sodium">Low Sodium</SelectItem>
                            <SelectItem value="other">Other</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-2">
                        <Label>Special Notes</Label>
                        <Textarea placeholder="Enter specific requirements..." rows={3} />
                      </div>
                      <Button className="w-full">Add Requirement</Button>
                    </div>
                  </DialogContent>
                </Dialog>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Student</TableHead>
                    <TableHead>Room</TableHead>
                    <TableHead>Diet Type</TableHead>
                    <TableHead>Special Notes</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {specialDiets.map((diet) => (
                    <TableRow key={diet.id}>
                      <TableCell className="font-medium">{diet.student}</TableCell>
                      <TableCell>{diet.room}</TableCell>
                      <TableCell>
                        <Badge variant="outline" className="capitalize">{diet.type}</Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground">{diet.notes}</TableCell>
                      <TableCell>
                        <Button variant="ghost" size="icon">
                          <Edit className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="feedback">
          <Card>
            <CardHeader>
              <CardTitle>Recent Meal Feedback</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Date</TableHead>
                    <TableHead>Meal</TableHead>
                    <TableHead>Rating</TableHead>
                    <TableHead>Comments</TableHead>
                    <TableHead>Top Feedback</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {mealFeedback.map((feedback) => (
                    <TableRow key={feedback.id}>
                      <TableCell>{feedback.date}</TableCell>
                      <TableCell>{feedback.meal}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1">
                          <span className={`font-medium ${
                            feedback.rating >= 4 ? 'text-green-600' :
                            feedback.rating >= 3 ? 'text-amber-600' :
                            'text-red-600'
                          }`}>
                            {feedback.rating}
                          </span>
                          <span className="text-muted-foreground">/5</span>
                        </div>
                      </TableCell>
                      <TableCell>{feedback.comments} comments</TableCell>
                      <TableCell className="text-muted-foreground">{feedback.topFeedback}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default MessMenu;
