import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { 
  Search, 
  Plus, 
  Building2, 
  BedDouble, 
  Users,
  Edit,
  Trash2,
  UserPlus,
  Filter
} from "lucide-react";

const RoomAllocation = () => {
  const [selectedBlock, setSelectedBlock] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");

  const blocks = [
    { id: "A", name: "Block A", type: "Boys", floors: 3, totalRooms: 60 },
    { id: "B", name: "Block B", type: "Boys", floors: 3, totalRooms: 60 },
    { id: "C", name: "Block C", type: "Girls", floors: 2, totalRooms: 50 },
    { id: "D", name: "Block D", type: "Girls", floors: 2, totalRooms: 50 },
  ];

  const rooms = [
    { id: 1, number: "A-101", block: "A", floor: 1, capacity: 4, occupied: 4, type: "Standard", status: "full" },
    { id: 2, number: "A-102", block: "A", floor: 1, capacity: 4, occupied: 3, type: "Standard", status: "available" },
    { id: 3, number: "A-103", block: "A", floor: 1, capacity: 2, occupied: 0, type: "Premium", status: "vacant" },
    { id: 4, number: "A-104", block: "A", floor: 1, capacity: 4, occupied: 4, type: "Standard", status: "full" },
    { id: 5, number: "B-201", block: "B", floor: 2, capacity: 4, occupied: 2, type: "Standard", status: "available" },
    { id: 6, number: "B-202", block: "B", floor: 2, capacity: 4, occupied: 0, type: "Standard", status: "maintenance" },
    { id: 7, number: "C-101", block: "C", floor: 1, capacity: 3, occupied: 3, type: "Standard", status: "full" },
    { id: 8, number: "D-102", block: "D", floor: 1, capacity: 2, occupied: 1, type: "Premium", status: "available" },
  ];

  const students = [
    { id: 1, name: "Rohan Sharma", admNo: "ADM001", class: "10-A", room: "A-101", bed: 1, checkIn: "2024-06-15" },
    { id: 2, name: "Vikram Singh", admNo: "ADM015", class: "10-B", room: "A-101", bed: 2, checkIn: "2024-06-15" },
    { id: 3, name: "Arun Kumar", admNo: "ADM022", class: "9-A", room: "A-101", bed: 3, checkIn: "2024-06-16" },
    { id: 4, name: "Raj Patel", admNo: "ADM031", class: "9-B", room: "A-101", bed: 4, checkIn: "2024-06-16" },
    { id: 5, name: "Kabir Khan", admNo: "ADM045", class: "11-A", room: "A-102", bed: 1, checkIn: "2024-06-15" },
    { id: 6, name: "Arjun Verma", admNo: "ADM052", class: "11-B", room: "A-102", bed: 2, checkIn: "2024-06-17" },
    { id: 7, name: "Priya Sharma", admNo: "ADM068", class: "10-A", room: "C-101", bed: 1, checkIn: "2024-06-15" },
    { id: 8, name: "Aisha Verma", admNo: "ADM071", class: "10-B", room: "C-101", bed: 2, checkIn: "2024-06-15" },
  ];

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "full":
        return <Badge variant="secondary">Full</Badge>;
      case "available":
        return <Badge className="bg-green-500 hover:bg-green-600">Available</Badge>;
      case "vacant":
        return <Badge variant="outline" className="text-blue-600 border-blue-300">Vacant</Badge>;
      case "maintenance":
        return <Badge variant="destructive">Maintenance</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  const filteredRooms = rooms.filter(room => 
    (selectedBlock === "all" || room.block === selectedBlock) &&
    room.number.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Room Allocation</h1>
          <p className="text-muted-foreground">Manage hostel rooms and student allocations</p>
        </div>
        <div className="flex gap-2">
          <Dialog>
            <DialogTrigger asChild>
              <Button variant="outline">
                <Building2 className="h-4 w-4 mr-2" />
                Add Room
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Add New Room</DialogTitle>
              </DialogHeader>
              <div className="space-y-4 pt-4">
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Block</Label>
                    <Select>
                      <SelectTrigger>
                        <SelectValue placeholder="Select block" />
                      </SelectTrigger>
                      <SelectContent>
                        {blocks.map(block => (
                          <SelectItem key={block.id} value={block.id}>{block.name}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Room Number</Label>
                    <Input placeholder="e.g., A-105" />
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label>Floor</Label>
                    <Input type="number" placeholder="Floor number" />
                  </div>
                  <div className="space-y-2">
                    <Label>Capacity</Label>
                    <Input type="number" placeholder="Number of beds" />
                  </div>
                </div>
                <div className="space-y-2">
                  <Label>Room Type</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="standard">Standard</SelectItem>
                      <SelectItem value="premium">Premium</SelectItem>
                      <SelectItem value="deluxe">Deluxe</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <Button className="w-full">Add Room</Button>
              </div>
            </DialogContent>
          </Dialog>
          <Dialog>
            <DialogTrigger asChild>
              <Button>
                <UserPlus className="h-4 w-4 mr-2" />
                Allocate Student
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Allocate Student to Room</DialogTitle>
              </DialogHeader>
              <div className="space-y-4 pt-4">
                <div className="space-y-2">
                  <Label>Student</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select student" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="s1">Amit Kumar (ADM089)</SelectItem>
                      <SelectItem value="s2">Neha Singh (ADM092)</SelectItem>
                      <SelectItem value="s3">Rahul Jain (ADM095)</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Room</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select room" />
                    </SelectTrigger>
                    <SelectContent>
                      {rooms.filter(r => r.status === "available" || r.status === "vacant").map(room => (
                        <SelectItem key={room.id} value={room.number}>
                          {room.number} ({room.capacity - room.occupied} beds available)
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Bed Number</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select bed" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">Bed 1</SelectItem>
                      <SelectItem value="2">Bed 2</SelectItem>
                      <SelectItem value="3">Bed 3</SelectItem>
                      <SelectItem value="4">Bed 4</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label>Check-in Date</Label>
                  <Input type="date" />
                </div>
                <Button className="w-full">Allocate Room</Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Block Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {blocks.map((block) => (
          <Card key={block.id} className="cursor-pointer hover:shadow-md transition-shadow" onClick={() => setSelectedBlock(block.id)}>
            <CardContent className="p-4">
              <div className="flex items-center justify-between mb-2">
                <span className="font-semibold">{block.name}</span>
                <Badge variant={block.type === "Boys" ? "default" : "secondary"}>{block.type}</Badge>
              </div>
              <div className="text-sm text-muted-foreground">
                <p>{block.floors} Floors • {block.totalRooms} Rooms</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Tabs defaultValue="rooms" className="space-y-4">
        <TabsList>
          <TabsTrigger value="rooms">
            <Building2 className="h-4 w-4 mr-2" />
            Rooms
          </TabsTrigger>
          <TabsTrigger value="students">
            <Users className="h-4 w-4 mr-2" />
            Allocated Students
          </TabsTrigger>
        </TabsList>

        <TabsContent value="rooms">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Room Directory</CardTitle>
                <div className="flex gap-2">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input 
                      placeholder="Search rooms..." 
                      className="pl-9 w-64"
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                    />
                  </div>
                  <Select value={selectedBlock} onValueChange={setSelectedBlock}>
                    <SelectTrigger className="w-40">
                      <Filter className="h-4 w-4 mr-2" />
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">All Blocks</SelectItem>
                      {blocks.map(block => (
                        <SelectItem key={block.id} value={block.id}>{block.name}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Room No.</TableHead>
                    <TableHead>Block</TableHead>
                    <TableHead>Floor</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Capacity</TableHead>
                    <TableHead>Occupied</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredRooms.map((room) => (
                    <TableRow key={room.id}>
                      <TableCell className="font-medium">{room.number}</TableCell>
                      <TableCell>Block {room.block}</TableCell>
                      <TableCell>Floor {room.floor}</TableCell>
                      <TableCell>{room.type}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1">
                          <BedDouble className="h-4 w-4 text-muted-foreground" />
                          {room.capacity}
                        </div>
                      </TableCell>
                      <TableCell>{room.occupied}/{room.capacity}</TableCell>
                      <TableCell>{getStatusBadge(room.status)}</TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          <Button variant="ghost" size="icon">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon">
                            <UserPlus className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="students">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>Allocated Students</CardTitle>
                <div className="relative">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input placeholder="Search students..." className="pl-9 w-64" />
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Adm. No.</TableHead>
                    <TableHead>Student Name</TableHead>
                    <TableHead>Class</TableHead>
                    <TableHead>Room</TableHead>
                    <TableHead>Bed</TableHead>
                    <TableHead>Check-in Date</TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {students.map((student) => (
                    <TableRow key={student.id}>
                      <TableCell className="font-medium">{student.admNo}</TableCell>
                      <TableCell>{student.name}</TableCell>
                      <TableCell>{student.class}</TableCell>
                      <TableCell>{student.room}</TableCell>
                      <TableCell>Bed {student.bed}</TableCell>
                      <TableCell>{student.checkIn}</TableCell>
                      <TableCell>
                        <div className="flex gap-1">
                          <Button variant="ghost" size="icon">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" className="text-destructive">
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
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

export default RoomAllocation;
