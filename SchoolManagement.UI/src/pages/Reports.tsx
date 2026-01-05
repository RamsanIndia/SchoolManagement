import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { toast } from "@/hooks/use-toast";
import { 
  FileText, 
  Users, 
  DollarSign, 
  ClipboardCheck, 
  GraduationCap,
  Download,
  FileSpreadsheet,
  Calendar
} from "lucide-react";
import { Input } from "@/components/ui/input";

export default function Reports() {
  const [selectedReport, setSelectedReport] = useState<string>("");
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");
  const [format, setFormat] = useState<string>("pdf");

  const reportCategories = [
    {
      title: "Student Reports",
      icon: Users,
      reports: [
        { id: "student-list", name: "Student List Report", description: "Complete list of all students" },
        { id: "student-attendance", name: "Student Attendance Report", description: "Attendance summary by student" },
        { id: "student-performance", name: "Student Performance Report", description: "Academic performance analysis" },
      ]
    },
    {
      title: "Financial Reports",
      icon: DollarSign,
      reports: [
        { id: "fee-collection", name: "Fee Collection Report", description: "Fee payment summary" },
        { id: "outstanding-fees", name: "Outstanding Fees Report", description: "Pending fee payments" },
        { id: "expense-report", name: "Expense Report", description: "School expenses summary" },
      ]
    },
    {
      title: "Attendance Reports",
      icon: ClipboardCheck,
      reports: [
        { id: "daily-attendance", name: "Daily Attendance Report", description: "Day-wise attendance records" },
        { id: "class-attendance", name: "Class Attendance Report", description: "Class-wise attendance summary" },
        { id: "monthly-attendance", name: "Monthly Attendance Report", description: "Month-wise attendance statistics" },
      ]
    },
    {
      title: "Examination Reports",
      icon: GraduationCap,
      reports: [
        { id: "exam-results", name: "Exam Results Report", description: "Detailed examination results" },
        { id: "grade-distribution", name: "Grade Distribution Report", description: "Grade statistics and distribution" },
        { id: "subject-performance", name: "Subject Performance Report", description: "Subject-wise performance analysis" },
      ]
    },
  ];

  const handleGenerateReport = () => {
    if (!selectedReport) {
      toast({
        title: "Select Report",
        description: "Please select a report type to generate",
        variant: "destructive",
      });
      return;
    }

    toast({
      title: "Generating Report",
      description: `Your ${format.toUpperCase()} report is being generated...`,
    });

    // Simulate report generation
    setTimeout(() => {
      toast({
        title: "Report Ready",
        description: "Your report has been generated successfully",
      });
    }, 2000);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <FileText className="h-8 w-8 text-primary" />
        <div>
          <h1 className="text-3xl font-bold text-foreground">Reports</h1>
          <p className="text-muted-foreground">Generate and download comprehensive reports</p>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileSpreadsheet className="h-5 w-5" />
              Report Configuration
            </CardTitle>
            <CardDescription>Select report type and configure parameters</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="reportType">Report Type</Label>
              <Select value={selectedReport} onValueChange={setSelectedReport}>
                <SelectTrigger id="reportType">
                  <SelectValue placeholder="Select a report" />
                </SelectTrigger>
                <SelectContent>
                  {reportCategories.map((category) => (
                    <div key={category.title}>
                      <div className="px-2 py-1.5 text-sm font-semibold text-muted-foreground">
                        {category.title}
                      </div>
                      {category.reports.map((report) => (
                        <SelectItem key={report.id} value={report.id}>
                          {report.name}
                        </SelectItem>
                      ))}
                    </div>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="startDate">Start Date</Label>
                <Input
                  id="startDate"
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="endDate">End Date</Label>
                <Input
                  id="endDate"
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="format">Export Format</Label>
              <Select value={format} onValueChange={setFormat}>
                <SelectTrigger id="format">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="pdf">PDF Document</SelectItem>
                  <SelectItem value="excel">Excel Spreadsheet</SelectItem>
                  <SelectItem value="csv">CSV File</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <Button 
              onClick={handleGenerateReport} 
              className="w-full bg-gradient-primary"
            >
              <Download className="mr-2 h-4 w-4" />
              Generate Report
            </Button>
          </CardContent>
        </Card>

        <div className="space-y-4">
          {reportCategories.map((category) => (
            <Card key={category.title}>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-lg">
                  <category.icon className="h-5 w-5" />
                  {category.title}
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                {category.reports.map((report) => (
                  <div 
                    key={report.id}
                    className="flex items-start justify-between p-3 rounded-lg border hover:bg-muted/50 transition-colors cursor-pointer"
                    onClick={() => setSelectedReport(report.id)}
                  >
                    <div>
                      <p className="font-medium text-sm">{report.name}</p>
                      <p className="text-xs text-muted-foreground">{report.description}</p>
                    </div>
                    {selectedReport === report.id && (
                      <div className="h-2 w-2 rounded-full bg-primary" />
                    )}
                  </div>
                ))}
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
}
