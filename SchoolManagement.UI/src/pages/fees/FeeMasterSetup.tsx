import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Plus, Edit, Trash2 } from "lucide-react";
import { toast } from "@/hooks/use-toast";

interface FeeHead {
  id: number;
  category: string;
  head: string;
  amount: number;
  frequency: string;
  optional: boolean;
}

const mockFeeHeads: FeeHead[] = [
  { id: 1, category: "Tuition", head: "Tuition Fee", amount: 1500, frequency: "Monthly", optional: false },
  { id: 2, category: "Transport", head: "Bus Fee", amount: 800, frequency: "Monthly", optional: true },
  { id: 3, category: "Hostel", head: "Hostel Fee", amount: 5000, frequency: "Monthly", optional: true },
  { id: 4, category: "Tuition", head: "Computer Fee", amount: 300, frequency: "Monthly", optional: false },
  { id: 5, category: "Activities", head: "Sports Fee", amount: 500, frequency: "Quarterly", optional: true },
  { id: 6, category: "Tuition", head: "Library Fee", amount: 200, frequency: "Annual", optional: false },
];

const categories = ["Tuition", "Transport", "Hostel", "Activities", "Examination", "Other"];
const frequencies = ["Monthly", "Quarterly", "Half-Yearly", "Annual"];

export default function FeeMasterSetup() {
  const [feeHeads, setFeeHeads] = useState<FeeHead[]>(mockFeeHeads);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [formData, setFormData] = useState({
    category: "",
    head: "",
    amount: "",
    frequency: "Monthly",
    optional: false,
  });

  const handleSubmit = () => {
    const newFeeHead: FeeHead = {
      id: feeHeads.length + 1,
      category: formData.category,
      head: formData.head,
      amount: Number(formData.amount),
      frequency: formData.frequency,
      optional: formData.optional,
    };

    setFeeHeads([...feeHeads, newFeeHead]);
    setIsDialogOpen(false);
    setFormData({ category: "", head: "", amount: "", frequency: "Monthly", optional: false });
    toast({ title: "Success", description: "Fee head created successfully" });
  };

  const handleDelete = (id: number) => {
    setFeeHeads(feeHeads.filter((head) => head.id !== id));
    toast({ title: "Success", description: "Fee head deleted successfully" });
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Fee Master Setup</h1>
          <p className="text-muted-foreground">Configure fee categories and heads</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              Add Fee Head
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Create Fee Head</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="category">Category</Label>
                <Select
                  value={formData.category}
                  onValueChange={(value) => setFormData({ ...formData, category: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select category" />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map((cat) => (
                      <SelectItem key={cat} value={cat}>
                        {cat}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="head">Fee Head Name</Label>
                <Input
                  id="head"
                  value={formData.head}
                  onChange={(e) => setFormData({ ...formData, head: e.target.value })}
                  placeholder="Enter fee head name"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="amount">Default Amount (₹)</Label>
                <Input
                  id="amount"
                  type="number"
                  value={formData.amount}
                  onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                  placeholder="0"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="frequency">Frequency</Label>
                <Select
                  value={formData.frequency}
                  onValueChange={(value) => setFormData({ ...formData, frequency: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {frequencies.map((freq) => (
                      <SelectItem key={freq} value={freq}>
                        {freq}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="flex items-center space-x-2">
                <Switch
                  id="optional"
                  checked={formData.optional}
                  onCheckedChange={(checked) => setFormData({ ...formData, optional: checked })}
                />
                <Label htmlFor="optional">Optional Fee</Label>
              </div>

              <Button onClick={handleSubmit} className="w-full">
                Create Fee Head
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Fee Heads</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Category</TableHead>
                <TableHead>Fee Head</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Frequency</TableHead>
                <TableHead>Type</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {feeHeads.map((head) => (
                <TableRow key={head.id}>
                  <TableCell>
                    <Badge variant="outline">{head.category}</Badge>
                  </TableCell>
                  <TableCell className="font-medium">{head.head}</TableCell>
                  <TableCell>₹{head.amount.toLocaleString()}</TableCell>
                  <TableCell>{head.frequency}</TableCell>
                  <TableCell>
                    <Badge variant={head.optional ? "secondary" : "default"}>
                      {head.optional ? "Optional" : "Mandatory"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button variant="ghost" size="sm">
                      <Edit className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => handleDelete(head.id)}
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
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
