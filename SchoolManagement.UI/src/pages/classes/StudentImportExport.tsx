/**
 * Bulk Student Import/Export with CSV Support
 */

import { useState, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { 
  ArrowLeft, Upload, Download, FileSpreadsheet, Check, X, 
  AlertTriangle, Users, FileText, Loader2 
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { toast } from "@/hooks/use-toast";

interface StudentPreview {
  id: number;
  name: string;
  rollNo: string;
  gender: string;
  parentName: string;
  mobile: string;
  email: string;
  status: "valid" | "error" | "warning";
  errors?: string[];
}

const mockClasses = ["Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5"];
const mockSections = ["A", "B", "C"];

const sampleCSVData = `Name,Roll No,Gender,Parent Name,Mobile,Email
Aarav Sharma,101,Male,Rajesh Sharma,9876543210,rajesh@email.com
Priya Patel,102,Female,Amit Patel,9876543211,amit@email.com
Rohan Verma,103,Male,Sunil Verma,9876543212,sunil@email.com
Ananya Singh,104,Female,Vikram Singh,9876543213,vikram@email.com
Arjun Kumar,105,Male,Raj Kumar,9876543214,raj@email.com`;

const mockPreviewData: StudentPreview[] = [
  { id: 1, name: "Aarav Sharma", rollNo: "101", gender: "Male", parentName: "Rajesh Sharma", mobile: "9876543210", email: "rajesh@email.com", status: "valid" },
  { id: 2, name: "Priya Patel", rollNo: "102", gender: "Female", parentName: "Amit Patel", mobile: "9876543211", email: "amit@email.com", status: "valid" },
  { id: 3, name: "Rohan Verma", rollNo: "103", gender: "Male", parentName: "Sunil Verma", mobile: "9876543212", email: "sunil@email.com", status: "valid" },
  { id: 4, name: "", rollNo: "104", gender: "Female", parentName: "Vikram Singh", mobile: "9876543213", email: "vikram@email.com", status: "error", errors: ["Name is required"] },
  { id: 5, name: "Arjun Kumar", rollNo: "105", gender: "Male", parentName: "Raj Kumar", mobile: "invalid", email: "raj@email.com", status: "warning", errors: ["Invalid mobile number format"] },
];

export default function StudentImportExport() {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedClass, setSelectedClass] = useState("Grade 1");
  const [selectedSection, setSelectedSection] = useState("A");
  const [isDragging, setIsDragging] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [previewData, setPreviewData] = useState<StudentPreview[]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [progress, setProgress] = useState(0);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const file = e.dataTransfer.files[0];
    if (file && file.name.endsWith(".csv")) {
      processFile(file);
    } else {
      toast({ title: "Error", description: "Please upload a CSV file", variant: "destructive" });
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      processFile(file);
    }
  };

  const processFile = (file: File) => {
    setUploadedFile(file);
    setIsProcessing(true);
    setProgress(0);
    
    // Simulate processing
    const interval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 100) {
          clearInterval(interval);
          setIsProcessing(false);
          setPreviewData(mockPreviewData);
          return 100;
        }
        return prev + 20;
      });
    }, 200);
  };

  const handleImport = () => {
    setShowConfirmDialog(true);
  };

  const confirmImport = () => {
    const validCount = previewData.filter(s => s.status === "valid").length;
    toast({ 
      title: "Import Successful", 
      description: `${validCount} students imported to ${selectedClass} - Section ${selectedSection}` 
    });
    setShowConfirmDialog(false);
    setPreviewData([]);
    setUploadedFile(null);
  };

  const handleExport = () => {
    // Create CSV content
    const csvContent = sampleCSVData;
    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `students_${selectedClass.replace(" ", "_")}_${selectedSection}.csv`;
    a.click();
    URL.revokeObjectURL(url);
    toast({ title: "Export Complete", description: "CSV file downloaded successfully" });
  };

  const downloadTemplate = () => {
    const template = "Name,Roll No,Gender,Parent Name,Mobile,Email\n";
    const blob = new Blob([template], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "student_import_template.csv";
    a.click();
    URL.revokeObjectURL(url);
    toast({ title: "Template Downloaded", description: "Fill in the template and upload" });
  };

  const validCount = previewData.filter(s => s.status === "valid").length;
  const errorCount = previewData.filter(s => s.status === "error").length;
  const warningCount = previewData.filter(s => s.status === "warning").length;

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate("/classes")}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold text-foreground">Student Import/Export</h1>
            <p className="text-muted-foreground mt-1">Bulk manage students with CSV files</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Select value={selectedClass} onValueChange={setSelectedClass}>
            <SelectTrigger className="w-[140px] bg-background">
              <SelectValue />
            </SelectTrigger>
            <SelectContent className="bg-popover">
              {mockClasses.map(c => (
                <SelectItem key={c} value={c}>{c}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={selectedSection} onValueChange={setSelectedSection}>
            <SelectTrigger className="w-[100px] bg-background">
              <SelectValue />
            </SelectTrigger>
            <SelectContent className="bg-popover">
              {mockSections.map(s => (
                <SelectItem key={s} value={s}>Section {s}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Action Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card className="bg-card border-border">
          <CardContent className="p-6">
            <div className="flex items-center gap-4">
              <div className="h-12 w-12 rounded-lg bg-primary/10 flex items-center justify-center">
                <Upload className="h-6 w-6 text-primary" />
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-foreground">Import Students</h3>
                <p className="text-sm text-muted-foreground">Upload CSV file</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border cursor-pointer hover:bg-muted/50 transition-colors" onClick={handleExport}>
          <CardContent className="p-6">
            <div className="flex items-center gap-4">
              <div className="h-12 w-12 rounded-lg bg-green-500/10 flex items-center justify-center">
                <Download className="h-6 w-6 text-green-500" />
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-foreground">Export Students</h3>
                <p className="text-sm text-muted-foreground">Download as CSV</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card className="bg-card border-border cursor-pointer hover:bg-muted/50 transition-colors" onClick={downloadTemplate}>
          <CardContent className="p-6">
            <div className="flex items-center gap-4">
              <div className="h-12 w-12 rounded-lg bg-blue-500/10 flex items-center justify-center">
                <FileText className="h-6 w-6 text-blue-500" />
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-foreground">Download Template</h3>
                <p className="text-sm text-muted-foreground">Get CSV template</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Upload Area */}
      <Card className="bg-card border-border">
        <CardHeader>
          <CardTitle className="text-foreground flex items-center gap-2">
            <FileSpreadsheet className="h-5 w-5" />
            Upload CSV File
          </CardTitle>
          <CardDescription>Drag and drop or click to upload student data</CardDescription>
        </CardHeader>
        <CardContent>
          <input
            ref={fileInputRef}
            type="file"
            accept=".csv"
            onChange={handleFileSelect}
            className="hidden"
          />
          <div
            onDragOver={handleDragOver}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            className={`border-2 border-dashed rounded-lg p-12 text-center cursor-pointer transition-all ${
              isDragging
                ? "border-primary bg-primary/5"
                : "border-border hover:border-muted-foreground/50 hover:bg-muted/30"
            }`}
          >
            {isProcessing ? (
              <div className="space-y-4">
                <Loader2 className="h-12 w-12 mx-auto text-primary animate-spin" />
                <div className="space-y-2">
                  <p className="text-foreground font-medium">Processing {uploadedFile?.name}...</p>
                  <Progress value={progress} className="w-64 mx-auto" />
                  <p className="text-sm text-muted-foreground">{progress}% complete</p>
                </div>
              </div>
            ) : uploadedFile ? (
              <div className="space-y-2">
                <FileSpreadsheet className="h-12 w-12 mx-auto text-green-500" />
                <p className="text-foreground font-medium">{uploadedFile.name}</p>
                <p className="text-sm text-muted-foreground">File uploaded successfully</p>
              </div>
            ) : (
              <>
                <Upload className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                <p className="text-foreground font-medium">Drop your CSV file here</p>
                <p className="text-sm text-muted-foreground mt-1">or click to browse</p>
              </>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Preview Table */}
      {previewData.length > 0 && (
        <Card className="bg-card border-border">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div>
                <CardTitle className="text-foreground flex items-center gap-2">
                  <Users className="h-5 w-5" />
                  Import Preview
                </CardTitle>
                <CardDescription>{previewData.length} records found</CardDescription>
              </div>
              <div className="flex items-center gap-4">
                <div className="flex items-center gap-2">
                  <Badge className="bg-green-500/10 text-green-500 border-green-500/30">
                    <Check className="h-3 w-3 mr-1" />
                    {validCount} Valid
                  </Badge>
                  <Badge className="bg-yellow-500/10 text-yellow-500 border-yellow-500/30">
                    <AlertTriangle className="h-3 w-3 mr-1" />
                    {warningCount} Warnings
                  </Badge>
                  <Badge className="bg-destructive/10 text-destructive border-destructive/30">
                    <X className="h-3 w-3 mr-1" />
                    {errorCount} Errors
                  </Badge>
                </div>
                <Button onClick={handleImport} disabled={validCount === 0}>
                  Import {validCount} Students
                </Button>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow className="border-border hover:bg-muted/50">
                  <TableHead className="text-muted-foreground w-[80px]">Status</TableHead>
                  <TableHead className="text-muted-foreground">Name</TableHead>
                  <TableHead className="text-muted-foreground">Roll No</TableHead>
                  <TableHead className="text-muted-foreground">Gender</TableHead>
                  <TableHead className="text-muted-foreground">Parent Name</TableHead>
                  <TableHead className="text-muted-foreground">Mobile</TableHead>
                  <TableHead className="text-muted-foreground">Email</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {previewData.map((student) => (
                  <TableRow 
                    key={student.id} 
                    className={`border-border hover:bg-muted/50 ${
                      student.status === "error" ? "bg-destructive/5" : 
                      student.status === "warning" ? "bg-yellow-500/5" : ""
                    }`}
                  >
                    <TableCell>
                      {student.status === "valid" && (
                        <Check className="h-5 w-5 text-green-500" />
                      )}
                      {student.status === "warning" && (
                        <AlertTriangle className="h-5 w-5 text-yellow-500" />
                      )}
                      {student.status === "error" && (
                        <X className="h-5 w-5 text-destructive" />
                      )}
                    </TableCell>
                    <TableCell className="text-foreground font-medium">
                      {student.name || <span className="text-destructive italic">Missing</span>}
                    </TableCell>
                    <TableCell className="text-foreground">{student.rollNo}</TableCell>
                    <TableCell className="text-foreground">{student.gender}</TableCell>
                    <TableCell className="text-foreground">{student.parentName}</TableCell>
                    <TableCell className={student.status === "warning" ? "text-yellow-500" : "text-foreground"}>
                      {student.mobile}
                    </TableCell>
                    <TableCell className="text-foreground">{student.email}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Confirm Dialog */}
      <Dialog open={showConfirmDialog} onOpenChange={setShowConfirmDialog}>
        <DialogContent className="bg-card border-border">
          <DialogHeader>
            <DialogTitle className="text-foreground">Confirm Import</DialogTitle>
            <DialogDescription>
              You are about to import {validCount} students to {selectedClass} - Section {selectedSection}.
              {errorCount > 0 && ` ${errorCount} records with errors will be skipped.`}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowConfirmDialog(false)}>Cancel</Button>
            <Button onClick={confirmImport}>Confirm Import</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
