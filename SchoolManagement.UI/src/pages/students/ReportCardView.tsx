import { useNavigate, useParams } from "react-router-dom";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, Download, Printer, Share2 } from "lucide-react";

const mockReportCard = {
  student: {
    name: "Rohan Sharma",
    rollNo: "5A-01",
    class: "5",
    section: "A",
    admissionNo: "ADM001",
    dob: "2010-05-15",
    fatherName: "Rajesh Sharma",
    motherName: "Priya Sharma",
  },
  exam: {
    name: "Term 1 Examination",
    year: "2024-25",
    date: "October 2024",
  },
  subjects: [
    { name: "English", maxMarks: 100, obtained: 85, grade: "A" },
    { name: "Hindi", maxMarks: 100, obtained: 78, grade: "B+" },
    { name: "Mathematics", maxMarks: 100, obtained: 92, grade: "A+" },
    { name: "Science", maxMarks: 100, obtained: 88, grade: "A" },
    { name: "Social Studies", maxMarks: 100, obtained: 82, grade: "A" },
    { name: "Computer Science", maxMarks: 50, obtained: 45, grade: "A+" },
  ],
  coScholastic: [
    { area: "Art & Craft", grade: "A" },
    { area: "Music", grade: "B" },
    { area: "Sports", grade: "A" },
    { area: "Discipline", grade: "A" },
  ],
  attendance: {
    totalDays: 120,
    present: 112,
    percentage: 93.3,
  },
  remarks: "Excellent performance. Keep up the good work!",
  rank: 3,
  totalStudents: 45,
};

export default function ReportCardView() {
  const navigate = useNavigate();
  const { id } = useParams();

  const data = mockReportCard;
  const totalObtained = data.subjects.reduce((sum, s) => sum + s.obtained, 0);
  const totalMax = data.subjects.reduce((sum, s) => sum + s.maxMarks, 0);
  const percentage = ((totalObtained / totalMax) * 100).toFixed(1);

  const getOverallGrade = (pct: number): string => {
    if (pct >= 90) return "A+";
    if (pct >= 80) return "A";
    if (pct >= 70) return "B+";
    if (pct >= 60) return "B";
    if (pct >= 50) return "C";
    if (pct >= 40) return "D";
    return "F";
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between print:hidden">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">Report Card</h1>
            <p className="text-muted-foreground">{data.student.name} - {data.exam.name}</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Share2 className="h-4 w-4 mr-2" />
            Share
          </Button>
          <Button variant="outline" onClick={() => window.print()}>
            <Printer className="h-4 w-4 mr-2" />
            Print
          </Button>
          <Button>
            <Download className="h-4 w-4 mr-2" />
            Download PDF
          </Button>
        </div>
      </div>

      {/* Report Card */}
      <Card className="max-w-4xl mx-auto print:shadow-none print:border-2">
        <CardContent className="p-8">
          {/* Header */}
          <div className="text-center border-b-2 pb-6 mb-6">
            <div className="flex justify-center mb-4">
              <div className="w-20 h-20 rounded-full bg-primary/10 flex items-center justify-center">
                <span className="text-2xl font-bold text-primary">ABC</span>
              </div>
            </div>
            <h1 className="text-2xl font-bold">ABC International School</h1>
            <p className="text-muted-foreground">123 Education Street, New Delhi - 110001</p>
            <p className="text-muted-foreground">Phone: +91 11 2345 6789 | Email: info@abcschool.edu</p>
            <div className="mt-4 inline-block px-6 py-2 bg-primary/10 rounded-full">
              <h2 className="text-lg font-semibold text-primary">{data.exam.name} - {data.exam.year}</h2>
            </div>
          </div>

          {/* Student Info */}
          <div className="grid grid-cols-2 gap-6 mb-8 p-4 bg-muted/30 rounded-lg">
            <div className="space-y-2">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Student Name:</span>
                <span className="font-medium">{data.student.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Admission No:</span>
                <span className="font-medium">{data.student.admissionNo}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Class - Section:</span>
                <span className="font-medium">{data.student.class} - {data.student.section}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Roll No:</span>
                <span className="font-medium">{data.student.rollNo}</span>
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Date of Birth:</span>
                <span className="font-medium">{new Date(data.student.dob).toLocaleDateString()}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Father's Name:</span>
                <span className="font-medium">{data.student.fatherName}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Mother's Name:</span>
                <span className="font-medium">{data.student.motherName}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Attendance:</span>
                <span className="font-medium">{data.attendance.percentage}%</span>
              </div>
            </div>
          </div>

          {/* Scholastic Areas */}
          <div className="mb-8">
            <h3 className="text-lg font-semibold mb-4 border-b pb-2">Scholastic Areas</h3>
            <table className="w-full">
              <thead>
                <tr className="bg-muted/50">
                  <th className="px-4 py-3 text-left font-medium">Subject</th>
                  <th className="px-4 py-3 text-center font-medium">Max Marks</th>
                  <th className="px-4 py-3 text-center font-medium">Marks Obtained</th>
                  <th className="px-4 py-3 text-center font-medium">Grade</th>
                </tr>
              </thead>
              <tbody>
                {data.subjects.map((subject, idx) => (
                  <tr key={idx} className="border-b">
                    <td className="px-4 py-3">{subject.name}</td>
                    <td className="px-4 py-3 text-center">{subject.maxMarks}</td>
                    <td className="px-4 py-3 text-center font-medium">{subject.obtained}</td>
                    <td className="px-4 py-3 text-center">
                      <Badge variant="outline">{subject.grade}</Badge>
                    </td>
                  </tr>
                ))}
                <tr className="bg-muted/30 font-semibold">
                  <td className="px-4 py-3">Total</td>
                  <td className="px-4 py-3 text-center">{totalMax}</td>
                  <td className="px-4 py-3 text-center">{totalObtained}</td>
                  <td className="px-4 py-3 text-center">
                    <Badge className="bg-primary/10 text-primary">{getOverallGrade(parseFloat(percentage))}</Badge>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Co-Scholastic Areas */}
          <div className="mb-8">
            <h3 className="text-lg font-semibold mb-4 border-b pb-2">Co-Scholastic Areas</h3>
            <div className="grid grid-cols-4 gap-4">
              {data.coScholastic.map((item, idx) => (
                <div key={idx} className="p-4 border rounded-lg text-center">
                  <p className="text-sm text-muted-foreground mb-1">{item.area}</p>
                  <Badge variant="outline" className="text-lg">{item.grade}</Badge>
                </div>
              ))}
            </div>
          </div>

          {/* Summary */}
          <div className="grid grid-cols-3 gap-4 mb-8 p-4 bg-primary/5 rounded-lg">
            <div className="text-center">
              <p className="text-sm text-muted-foreground">Percentage</p>
              <p className="text-3xl font-bold text-primary">{percentage}%</p>
            </div>
            <div className="text-center">
              <p className="text-sm text-muted-foreground">Overall Grade</p>
              <p className="text-3xl font-bold text-primary">{getOverallGrade(parseFloat(percentage))}</p>
            </div>
            <div className="text-center">
              <p className="text-sm text-muted-foreground">Class Rank</p>
              <p className="text-3xl font-bold text-primary">{data.rank} / {data.totalStudents}</p>
            </div>
          </div>

          {/* Remarks */}
          <div className="mb-8 p-4 border rounded-lg">
            <h3 className="text-sm font-medium text-muted-foreground mb-2">Teacher's Remarks</h3>
            <p className="italic">{data.remarks}</p>
          </div>

          {/* Signatures */}
          <div className="grid grid-cols-3 gap-8 mt-12 pt-8 border-t">
            <div className="text-center">
              <div className="border-t border-foreground pt-2 mt-12">
                <p className="text-sm text-muted-foreground">Class Teacher</p>
              </div>
            </div>
            <div className="text-center">
              <div className="border-t border-foreground pt-2 mt-12">
                <p className="text-sm text-muted-foreground">Principal</p>
              </div>
            </div>
            <div className="text-center">
              <div className="border-t border-foreground pt-2 mt-12">
                <p className="text-sm text-muted-foreground">Parent/Guardian</p>
              </div>
            </div>
          </div>

          {/* Footer */}
          <div className="text-center mt-8 pt-4 border-t text-sm text-muted-foreground">
            <p>This is a computer generated report card. Printed on {new Date().toLocaleDateString()}</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
