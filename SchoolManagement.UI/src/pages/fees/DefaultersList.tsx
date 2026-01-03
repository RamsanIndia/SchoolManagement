import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Search, Phone, Mail, AlertTriangle, Download } from "lucide-react";

interface Defaulter {
  id: number;
  name: string;
  admissionNo: string;
  class: string;
  section: string;
  phone: string;
  outstanding: number;
  lastPayment: string;
  daysOverdue: number;
}

const mockDefaulters: Defaulter[] = [
  {
    id: 1,
    name: "Rohan Sharma",
    admissionNo: "S10234",
    class: "9",
    section: "B",
    phone: "9876543210",
    outstanding: 4500,
    lastPayment: "2024-12-15",
    daysOverdue: 45,
  },
  {
    id: 2,
    name: "Kavya Jain",
    admissionNo: "S10567",
    class: "6",
    section: "A",
    phone: "9876543110",
    outstanding: 3000,
    lastPayment: "2024-11-28",
    daysOverdue: 62,
  },
  {
    id: 3,
    name: "Arjun Patel",
    admissionNo: "S10789",
    class: "8",
    section: "C",
    phone: "9876543220",
    outstanding: 6000,
    lastPayment: "2024-10-20",
    daysOverdue: 101,
  },
  {
    id: 4,
    name: "Priya Singh",
    admissionNo: "S10890",
    class: "10",
    section: "A",
    phone: "9876543330",
    outstanding: 2500,
    lastPayment: "2024-12-28",
    daysOverdue: 32,
  },
  {
    id: 5,
    name: "Vikram Mehta",
    admissionNo: "S11001",
    class: "7",
    section: "B",
    phone: "9876543440",
    outstanding: 5500,
    lastPayment: "2024-11-15",
    daysOverdue: 75,
  },
];

export default function DefaultersList() {
  const [defaulters, setDefaulters] = useState<Defaulter[]>(mockDefaulters);
  const [searchTerm, setSearchTerm] = useState("");
  const [classFilter, setClassFilter] = useState("all");

  const filteredDefaulters = defaulters.filter((defaulter) => {
    const matchesSearch =
      defaulter.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      defaulter.admissionNo.toLowerCase().includes(searchTerm.toLowerCase()) ||
      defaulter.phone.includes(searchTerm);

    const matchesClass = classFilter === "all" || defaulter.class === classFilter;

    return matchesSearch && matchesClass;
  });

  const totalOutstanding = filteredDefaulters.reduce((sum, d) => sum + d.outstanding, 0);

  const getSeverityBadge = (daysOverdue: number) => {
    if (daysOverdue < 30) return <Badge variant="secondary">Low</Badge>;
    if (daysOverdue < 60) return <Badge className="bg-orange-500">Medium</Badge>;
    return <Badge variant="destructive">High</Badge>;
  };

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Defaulters List</h1>
        <p className="text-muted-foreground">Students with outstanding fee payments</p>
      </div>

      {/* Stats Card */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Defaulters
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">{filteredDefaulters.length}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Outstanding
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold text-destructive">
              ₹{totalOutstanding.toLocaleString()}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Average Overdue
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">
              {filteredDefaulters.length > 0
                ? Math.round(
                    filteredDefaulters.reduce((sum, d) => sum + d.daysOverdue, 0) /
                      filteredDefaulters.length
                  )
                : 0}{" "}
              days
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by name, admission no, or phone..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={classFilter} onValueChange={setClassFilter}>
              <SelectTrigger className="w-full md:w-48">
                <SelectValue placeholder="Filter by class" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Classes</SelectItem>
                {Array.from({ length: 12 }, (_, i) => i + 1).map((cls) => (
                  <SelectItem key={cls} value={String(cls)}>
                    Class {cls}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button variant="outline">
              <Download className="mr-2 h-4 w-4" />
              Export
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Defaulters Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            Defaulters ({filteredDefaulters.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Student</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Contact</TableHead>
                <TableHead>Outstanding</TableHead>
                <TableHead>Last Payment</TableHead>
                <TableHead>Days Overdue</TableHead>
                <TableHead>Severity</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredDefaulters.map((defaulter) => (
                <TableRow key={defaulter.id}>
                  <TableCell>
                    <div>
                      <div className="font-medium">{defaulter.name}</div>
                      <div className="text-sm text-muted-foreground">{defaulter.admissionNo}</div>
                    </div>
                  </TableCell>
                  <TableCell>
                    {defaulter.class} - {defaulter.section}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1 text-sm">
                      <Phone className="h-3 w-3" />
                      {defaulter.phone}
                    </div>
                  </TableCell>
                  <TableCell className="font-bold text-destructive">
                    ₹{defaulter.outstanding.toLocaleString()}
                  </TableCell>
                  <TableCell>{defaulter.lastPayment}</TableCell>
                  <TableCell>
                    <span className="font-medium">{defaulter.daysOverdue} days</span>
                  </TableCell>
                  <TableCell>{getSeverityBadge(defaulter.daysOverdue)}</TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button variant="ghost" size="sm">
                        <Phone className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="sm">
                        <Mail className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
