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
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { CalendarIcon, Plus, X } from "lucide-react";
import { format } from "date-fns";
import { toast } from "@/hooks/use-toast";
import { cn } from "@/lib/utils";

interface FeeItem {
  head: string;
  amount: string;
  frequency: string;
  dueDate: Date | undefined;
}

const classes = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"];
const sessions = ["2023-24", "2024-25", "2025-26"];
const feeHeads = ["Tuition Fee", "Computer Fee", "Library Fee", "Sports Fee", "Bus Fee", "Hostel Fee"];
const frequencies = ["Monthly", "Quarterly", "Half-Yearly", "Annual"];

export default function FeeStructureSetup() {
  const [selectedClass, setSelectedClass] = useState("");
  const [selectedSession, setSelectedSession] = useState("2024-25");
  const [feeItems, setFeeItems] = useState<FeeItem[]>([
    { head: "", amount: "", frequency: "Monthly", dueDate: undefined },
  ]);

  const addFeeItem = () => {
    setFeeItems([...feeItems, { head: "", amount: "", frequency: "Monthly", dueDate: undefined }]);
  };

  const removeFeeItem = (index: number) => {
    setFeeItems(feeItems.filter((_, i) => i !== index));
  };

  const updateFeeItem = (index: number, field: keyof FeeItem, value: any) => {
    const updated = [...feeItems];
    updated[index] = { ...updated[index], [field]: value };
    setFeeItems(updated);
  };

  const handleSave = () => {
    if (!selectedClass) {
      toast({ title: "Error", description: "Please select a class", variant: "destructive" });
      return;
    }

    const incomplete = feeItems.some((item) => !item.head || !item.amount || !item.dueDate);
    if (incomplete) {
      toast({
        title: "Error",
        description: "Please fill all fee structure details",
        variant: "destructive",
      });
      return;
    }

    toast({ title: "Success", description: "Fee structure saved successfully" });
  };

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Fee Structure Setup</h1>
        <p className="text-muted-foreground">Assign fee structure to classes</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Configure Fee Structure</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Class and Session Selection */}
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label>Class</Label>
              <Select value={selectedClass} onValueChange={setSelectedClass}>
                <SelectTrigger>
                  <SelectValue placeholder="Select class" />
                </SelectTrigger>
                <SelectContent>
                  {classes.map((cls) => (
                    <SelectItem key={cls} value={cls}>
                      Class {cls}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Session</Label>
              <Select value={selectedSession} onValueChange={setSelectedSession}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {sessions.map((session) => (
                    <SelectItem key={session} value={session}>
                      {session}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Fee Items */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label className="text-base">Fee Items</Label>
              <Button onClick={addFeeItem} size="sm" variant="outline">
                <Plus className="h-4 w-4 mr-2" />
                Add Item
              </Button>
            </div>

            {feeItems.map((item, index) => (
              <Card key={index}>
                <CardContent className="pt-6">
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                    <div className="space-y-2">
                      <Label>Fee Head</Label>
                      <Select
                        value={item.head}
                        onValueChange={(value) => updateFeeItem(index, "head", value)}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select fee head" />
                        </SelectTrigger>
                        <SelectContent>
                          {feeHeads.map((head) => (
                            <SelectItem key={head} value={head}>
                              {head}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label>Amount (₹)</Label>
                      <Input
                        type="number"
                        value={item.amount}
                        onChange={(e) => updateFeeItem(index, "amount", e.target.value)}
                        placeholder="0"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label>Frequency</Label>
                      <Select
                        value={item.frequency}
                        onValueChange={(value) => updateFeeItem(index, "frequency", value)}
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

                    <div className="space-y-2">
                      <Label>Due Date</Label>
                      <div className="flex gap-2">
                        <Popover>
                          <PopoverTrigger asChild>
                            <Button
                              variant="outline"
                              className={cn(
                                "flex-1 justify-start text-left font-normal",
                                !item.dueDate && "text-muted-foreground"
                              )}
                            >
                              <CalendarIcon className="mr-2 h-4 w-4" />
                              {item.dueDate ? format(item.dueDate, "PPP") : "Pick date"}
                            </Button>
                          </PopoverTrigger>
                          <PopoverContent className="w-auto p-0">
                            <Calendar
                              mode="single"
                              selected={item.dueDate}
                              onSelect={(date) => updateFeeItem(index, "dueDate", date)}
                              initialFocus
                              className="pointer-events-auto"
                            />
                          </PopoverContent>
                        </Popover>
                        {feeItems.length > 1 && (
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => removeFeeItem(index)}
                          >
                            <X className="h-4 w-4 text-destructive" />
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          <Button onClick={handleSave} size="lg" className="w-full">
            Save Fee Structure
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
