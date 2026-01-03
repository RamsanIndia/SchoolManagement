import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  FileText,
  Plus,
  Edit,
  Trash2,
  Eye,
  Copy,
  GripVertical,
  CheckCircle,
  Settings,
} from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface Template {
  id: string;
  name: string;
  type: "CBSE" | "ICSE" | "Custom";
  description: string;
  isDefault: boolean;
  sections: string[];
  lastModified: string;
}

const mockTemplates: Template[] = [
  {
    id: "1",
    name: "CBSE Standard Template",
    type: "CBSE",
    description: "Standard CBSE report card format with scholastic and co-scholastic areas",
    isDefault: true,
    sections: ["Student Info", "Scholastic Areas", "Co-Scholastic Areas", "Attendance", "Remarks"],
    lastModified: "2024-01-15",
  },
  {
    id: "2",
    name: "ICSE Format",
    type: "ICSE",
    description: "ICSE board report card format with detailed subject breakdown",
    isDefault: false,
    sections: ["Student Info", "Subject Marks", "Grade Summary", "Teacher Comments"],
    lastModified: "2024-01-10",
  },
  {
    id: "3",
    name: "Custom Elementary",
    type: "Custom",
    description: "Custom template for elementary classes with simplified grading",
    isDefault: false,
    sections: ["Student Info", "Subjects", "Skills Assessment", "Parent Feedback"],
    lastModified: "2024-01-05",
  },
];

const templateSections = [
  { id: "student_info", name: "Student Information", required: true },
  { id: "subject_marks", name: "Subject-wise Marks" },
  { id: "grade_summary", name: "Grade Summary" },
  { id: "attendance", name: "Attendance Summary" },
  { id: "co_scholastic", name: "Co-Scholastic Areas" },
  { id: "remarks", name: "Teacher Remarks" },
  { id: "signature", name: "Signature Section" },
];

export default function ReportCardTemplates() {
  const navigate = useNavigate();
  const [templates, setTemplates] = useState<Template[]>(mockTemplates);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [selectedTemplate, setSelectedTemplate] = useState<Template | null>(null);
  const [newTemplateName, setNewTemplateName] = useState("");
  const [newTemplateType, setNewTemplateType] = useState<Template["type"]>("Custom");

  const handleCreateTemplate = () => {
    const newTemplate: Template = {
      id: (templates.length + 1).toString(),
      name: newTemplateName,
      type: newTemplateType,
      description: "Custom template",
      isDefault: false,
      sections: ["Student Info", "Subjects", "Remarks"],
      lastModified: new Date().toISOString().split("T")[0],
    };
    setTemplates([...templates, newTemplate]);
    setShowCreateDialog(false);
    setNewTemplateName("");
    toast({
      title: "Template Created",
      description: `${newTemplateName} has been created successfully.`,
    });
  };

  const handleSetDefault = (id: string) => {
    setTemplates((prev) =>
      prev.map((t) => ({ ...t, isDefault: t.id === id }))
    );
    toast({
      title: "Default Template Updated",
      description: "The default template has been changed.",
    });
  };

  const handleDeleteTemplate = (id: string) => {
    setTemplates((prev) => prev.filter((t) => t.id !== id));
    toast({
      title: "Template Deleted",
      description: "The template has been removed.",
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Report Card Templates</h1>
          <p className="text-muted-foreground">Create and manage report card templates</p>
        </div>
        <Button onClick={() => setShowCreateDialog(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Create Template
        </Button>
      </div>

      {/* Template Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {templates.map((template) => (
          <Card key={template.id} className={template.isDefault ? "border-primary" : ""}>
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="flex items-center gap-3">
                  <div className="p-2 rounded-lg bg-primary/10">
                    <FileText className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <CardTitle className="text-lg">{template.name}</CardTitle>
                    <div className="flex items-center gap-2 mt-1">
                      <Badge variant="outline">{template.type}</Badge>
                      {template.isDefault && (
                        <Badge className="bg-green-500/10 text-green-600">
                          <CheckCircle className="h-3 w-3 mr-1" />
                          Default
                        </Badge>
                      )}
                    </div>
                  </div>
                </div>
              </div>
              <CardDescription className="mt-2">{template.description}</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div>
                  <p className="text-sm font-medium mb-2">Sections</p>
                  <div className="flex flex-wrap gap-1">
                    {template.sections.map((section, idx) => (
                      <Badge key={idx} variant="secondary" className="text-xs">
                        {section}
                      </Badge>
                    ))}
                  </div>
                </div>
                <div className="text-xs text-muted-foreground">
                  Last modified: {template.lastModified}
                </div>
                <div className="flex gap-2 pt-2 border-t">
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1"
                    onClick={() => navigate(`/students/report-cards/templates/${template.id}`)}
                  >
                    <Eye className="h-4 w-4 mr-1" />
                    Preview
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    className="flex-1"
                    onClick={() => setSelectedTemplate(template)}
                  >
                    <Edit className="h-4 w-4 mr-1" />
                    Edit
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleSetDefault(template.id)}
                    disabled={template.isDefault}
                  >
                    <Settings className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Template Editor Dialog */}
      {selectedTemplate && (
        <Dialog open={!!selectedTemplate} onOpenChange={() => setSelectedTemplate(null)}>
          <DialogContent className="max-w-3xl max-h-[80vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Edit Template: {selectedTemplate.name}</DialogTitle>
              <DialogDescription>
                Drag and drop sections to rearrange. Toggle sections to include/exclude.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-6 py-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Template Name</Label>
                  <Input defaultValue={selectedTemplate.name} />
                </div>
                <div className="space-y-2">
                  <Label>Template Type</Label>
                  <Input defaultValue={selectedTemplate.type} disabled />
                </div>
              </div>

              <div>
                <Label className="mb-3 block">Template Sections</Label>
                <div className="space-y-2">
                  {templateSections.map((section) => (
                    <div
                      key={section.id}
                      className="flex items-center justify-between p-3 border rounded-lg"
                    >
                      <div className="flex items-center gap-3">
                        <GripVertical className="h-4 w-4 text-muted-foreground cursor-grab" />
                        <span className="font-medium">{section.name}</span>
                        {section.required && (
                          <Badge variant="outline" className="text-xs">Required</Badge>
                        )}
                      </div>
                      <Switch defaultChecked={selectedTemplate.sections.includes(section.name) || section.required} />
                    </div>
                  ))}
                </div>
              </div>

              <div className="space-y-2">
                <Label>Header Settings</Label>
                <div className="grid grid-cols-2 gap-4">
                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <span className="text-sm">Show School Logo</span>
                    <Switch defaultChecked />
                  </div>
                  <div className="flex items-center justify-between p-3 border rounded-lg">
                    <span className="text-sm">Show Student Photo</span>
                    <Switch defaultChecked />
                  </div>
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setSelectedTemplate(null)}>
                Cancel
              </Button>
              <Button onClick={() => {
                toast({ title: "Template Saved", description: "Your changes have been saved." });
                setSelectedTemplate(null);
              }}>
                Save Changes
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}

      {/* Create Template Dialog */}
      <Dialog open={showCreateDialog} onOpenChange={setShowCreateDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create New Template</DialogTitle>
            <DialogDescription>
              Create a new report card template from scratch or copy an existing one.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Template Name</Label>
              <Input
                value={newTemplateName}
                onChange={(e) => setNewTemplateName(e.target.value)}
                placeholder="Enter template name"
              />
            </div>
            <div className="space-y-2">
              <Label>Base Template</Label>
              <div className="grid grid-cols-3 gap-2">
                {(["CBSE", "ICSE", "Custom"] as const).map((type) => (
                  <Card
                    key={type}
                    className={`cursor-pointer transition-all ${
                      newTemplateType === type ? "border-primary bg-primary/5" : ""
                    }`}
                    onClick={() => setNewTemplateType(type)}
                  >
                    <CardContent className="pt-4 text-center">
                      <p className="font-medium">{type}</p>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowCreateDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleCreateTemplate} disabled={!newTemplateName}>
              Create Template
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
