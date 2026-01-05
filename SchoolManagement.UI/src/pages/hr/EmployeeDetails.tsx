import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { 
  ArrowLeft, Mail, Phone, MapPin, Calendar, Building, 
  Briefcase, DollarSign, Clock, FileText, TrendingUp 
} from "lucide-react";
import { Employee } from "@/types/navigation.types";
import EmployeeAttendanceTab from "@/components/hr/EmployeeAttendanceTab";
import EmployeeLeavesTab from "@/components/hr/EmployeeLeavesTab";
import EmployeePayrollTab from "@/components/hr/EmployeePayrollTab";
import EmployeePerformanceTab from "@/components/hr/EmployeePerformanceTab";

/**
 * Employee Details Page
 * Shows comprehensive employee information with tabs for:
 * - Attendance
 * - Leaves
 * - Payroll
 * - Performance Reviews
 */
export default function EmployeeDetails() {
  const { id } = useParams<{ id: string }>();
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Mock data - replace with actual API call
    setTimeout(() => {
      setEmployee({
        id: id || "1",
        employeeId: "EMP001",
        name: "John Doe",
        email: "john.doe@school.edu",
        phone: "+1234567890",
        departmentId: "dept-1",
        department: "Mathematics",
        designationId: "des-1",
        designation: "Senior Teacher",
        position: "Senior Teacher",
        salary: 65000,
        joinDate: "2020-01-15",
        status: "active",
        address: "123 Main St, City, State 12345",
        qualifications: "M.Sc Mathematics, B.Ed",
        dateOfBirth: "1985-05-20",
        emergencyContact: "+1234567891",
        bankAccount: "1234567890",
        taxId: "TAX123456",
        createdAt: "2020-01-15T00:00:00Z",
        updatedAt: "2024-01-15T00:00:00Z",
      });
      setLoading(false);
    }, 500);
  }, [id]);

  if (loading) {
    return <div className="flex items-center justify-center h-96">Loading...</div>;
  }

  if (!employee) {
    return <div className="flex items-center justify-center h-96">Employee not found</div>;
  }

  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase();
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case "active": return "bg-status-present";
      case "inactive": return "bg-status-absent";
      case "on-leave": return "bg-amber-500";
      default: return "bg-muted";
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link to="/hr/employees">
            <Button variant="outline" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Employee Details</h1>
            <p className="text-muted-foreground">Comprehensive employee information</p>
          </div>
        </div>
        <Button variant="outline">Edit Employee</Button>
      </div>

      {/* Employee Info Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-6">
            <Avatar className="h-24 w-24">
              <AvatarFallback className="text-2xl">{getInitials(employee.name)}</AvatarFallback>
            </Avatar>
            
            <div className="flex-1 space-y-4">
              <div className="flex items-start justify-between">
                <div>
                  <h2 className="text-2xl font-bold">{employee.name}</h2>
                  <p className="text-muted-foreground">{employee.position}</p>
                </div>
                <Badge className={getStatusColor(employee.status)}>
                  {employee.status}
                </Badge>
              </div>

              <div className="grid md:grid-cols-2 gap-4">
                <div className="flex items-center gap-2 text-sm">
                  <Mail className="h-4 w-4 text-muted-foreground" />
                  <span>{employee.email}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Phone className="h-4 w-4 text-muted-foreground" />
                  <span>{employee.phone}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Building className="h-4 w-4 text-muted-foreground" />
                  <span>{employee.department}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Briefcase className="h-4 w-4 text-muted-foreground" />
                  <span>{employee.designation}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span>Joined: {new Date(employee.joinDate).toLocaleDateString()}</span>
                </div>
                <div className="flex items-center gap-2 text-sm">
                  <DollarSign className="h-4 w-4 text-muted-foreground" />
                  <span>Salary: ${employee.salary.toLocaleString()}</span>
                </div>
                <div className="flex items-center gap-2 text-sm col-span-2">
                  <MapPin className="h-4 w-4 text-muted-foreground" />
                  <span>{employee.address}</span>
                </div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Tabs for related data */}
      <Tabs defaultValue="attendance" className="space-y-4">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="attendance" className="flex items-center gap-2">
            <Clock className="h-4 w-4" />
            Attendance
          </TabsTrigger>
          <TabsTrigger value="leaves" className="flex items-center gap-2">
            <Calendar className="h-4 w-4" />
            Leaves
          </TabsTrigger>
          <TabsTrigger value="payroll" className="flex items-center gap-2">
            <DollarSign className="h-4 w-4" />
            Payroll
          </TabsTrigger>
          <TabsTrigger value="performance" className="flex items-center gap-2">
            <TrendingUp className="h-4 w-4" />
            Performance
          </TabsTrigger>
        </TabsList>

        <TabsContent value="attendance">
          <EmployeeAttendanceTab employeeId={employee.id} />
        </TabsContent>

        <TabsContent value="leaves">
          <EmployeeLeavesTab employeeId={employee.id} />
        </TabsContent>

        <TabsContent value="payroll">
          <EmployeePayrollTab employeeId={employee.id} />
        </TabsContent>

        <TabsContent value="performance">
          <EmployeePerformanceTab employeeId={employee.id} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
