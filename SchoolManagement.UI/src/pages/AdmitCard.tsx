import { useState } from 'react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Download, Printer, Eye, Search, ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';

interface ExamSchedule {
  subject: string;
  date: string;
  time: string;
  duration: string;
}

interface AdmitCardData {
  id: string;
  studentName: string;
  rollNumber: string;
  class: string;
  examName: string;
  examCenter: string;
  examSchedule: ExamSchedule[];
  status: 'generated' | 'downloaded' | 'printed';
  studentPhoto?: string;
}

const examSchedule: ExamSchedule[] = [
  { subject: 'Mathematics', date: '2024-03-15', time: '09:00 AM', duration: '3 hours' },
  { subject: 'English', date: '2024-03-16', time: '09:00 AM', duration: '3 hours' },
  { subject: 'Science', date: '2024-03-17', time: '09:00 AM', duration: '3 hours' },
  { subject: 'Social Studies', date: '2024-03-18', time: '09:00 AM', duration: '3 hours' },
  { subject: 'Computer Science', date: '2024-03-19', time: '09:00 AM', duration: '2 hours' },
];

const mockAdmitCards: AdmitCardData[] = [
  {
    id: '1',
    studentName: 'Emma Rodriguez',
    rollNumber: 'STU2024001',
    class: 'Grade 10-A',
    examName: 'Mid-Term Examination 2024',
    examCenter: 'Main Campus - Block A',
    examSchedule,
    status: 'downloaded',
  },
  {
    id: '2',
    studentName: 'Michael Chen',
    rollNumber: 'STU2024002',
    class: 'Grade 10-A',
    examName: 'Mid-Term Examination 2024',
    examCenter: 'Main Campus - Block A',
    examSchedule,
    status: 'generated',
  },
  {
    id: '3',
    studentName: 'Sarah Johnson',
    rollNumber: 'STU2024003',
    class: 'Grade 10-B',
    examName: 'Mid-Term Examination 2024',
    examCenter: 'Main Campus - Block B',
    examSchedule,
    status: 'printed',
  },
  {
    id: '4',
    studentName: 'David Park',
    rollNumber: 'STU2024004',
    class: 'Grade 10-B',
    examName: 'Mid-Term Examination 2024',
    examCenter: 'Main Campus - Block B',
    examSchedule,
    status: 'generated',
  },
  {
    id: '5',
    studentName: 'Lisa Thompson',
    rollNumber: 'STU2024005',
    class: 'Grade 10-C',
    examName: 'Mid-Term Examination 2024',
    examCenter: 'Main Campus - Block C',
    examSchedule,
    status: 'downloaded',
  },
];

type SortField = 'rollNumber' | 'studentName' | 'class' | 'examCenter' | 'status';
type SortOrder = 'asc' | 'desc' | null;

export default function AdmitCard() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCard, setSelectedCard] = useState<AdmitCardData | null>(null);
  const [admitCards, setAdmitCards] = useState<AdmitCardData[]>(mockAdmitCards);
  const [sortField, setSortField] = useState<SortField | null>(null);
  const [sortOrder, setSortOrder] = useState<SortOrder>(null);
  const { toast } = useToast();

  const handleSort = (field: SortField) => {
    let newOrder: SortOrder = 'asc';
    
    if (sortField === field) {
      if (sortOrder === 'asc') {
        newOrder = 'desc';
      } else if (sortOrder === 'desc') {
        newOrder = null;
        setSortField(null);
        setSortOrder(null);
        return;
      }
    }
    
    setSortField(field);
    setSortOrder(newOrder);
  };

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) {
      return <ArrowUpDown className="ml-2 h-4 w-4" />;
    }
    if (sortOrder === 'asc') {
      return <ArrowUp className="ml-2 h-4 w-4" />;
    }
    return <ArrowDown className="ml-2 h-4 w-4" />;
  };

  let filteredCards = admitCards.filter(
    (card) =>
      card.studentName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      card.rollNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      card.class.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Apply sorting
  if (sortField && sortOrder) {
    filteredCards = [...filteredCards].sort((a, b) => {
      let aValue = a[sortField];
      let bValue = b[sortField];

      if (typeof aValue === 'string' && typeof bValue === 'string') {
        aValue = aValue.toLowerCase();
        bValue = bValue.toLowerCase();
      }

      if (aValue < bValue) {
        return sortOrder === 'asc' ? -1 : 1;
      }
      if (aValue > bValue) {
        return sortOrder === 'asc' ? 1 : -1;
      }
      return 0;
    });
  }

  const handleView = (card: AdmitCardData) => {
    setSelectedCard(card);
  };

  const handlePrint = (card: AdmitCardData) => {
    setSelectedCard(card);
    setTimeout(() => {
      window.print();
      setAdmitCards((prev) =>
        prev.map((c) => (c.id === card.id ? { ...c, status: 'printed' } : c))
      );
      toast({
        title: 'Print',
        description: 'Opening print dialog...',
      });
    }, 100);
  };

  const handleDownload = (card: AdmitCardData) => {
    setAdmitCards((prev) =>
      prev.map((c) => (c.id === card.id ? { ...c, status: 'downloaded' } : c))
    );
    toast({
      title: 'Download',
      description: `Admit card for ${card.studentName} downloaded successfully`,
    });
  };

  const getStatusBadge = (status: AdmitCardData['status']) => {
    const variants = {
      generated: 'secondary',
      downloaded: 'default',
      printed: 'outline',
    } as const;

    return (
      <Badge variant={variants[status]} className="capitalize">
        {status}
      </Badge>
    );
  };

  return (
    <div className="p-8">
      <div className="mb-8 print:hidden">
        <h1 className="text-3xl font-bold mb-2">Admit Cards</h1>
        <p className="text-muted-foreground">
          View, print, and download examination admit cards for all students
        </p>
      </div>

      <Card className="p-6 print:hidden">
        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by name, roll number, or class..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>
                  <Button
                    variant="ghost"
                    onClick={() => handleSort('rollNumber')}
                    className="font-medium hover:bg-transparent p-0"
                  >
                    Roll Number
                    {getSortIcon('rollNumber')}
                  </Button>
                </TableHead>
                <TableHead>
                  <Button
                    variant="ghost"
                    onClick={() => handleSort('studentName')}
                    className="font-medium hover:bg-transparent p-0"
                  >
                    Student Name
                    {getSortIcon('studentName')}
                  </Button>
                </TableHead>
                <TableHead>
                  <Button
                    variant="ghost"
                    onClick={() => handleSort('class')}
                    className="font-medium hover:bg-transparent p-0"
                  >
                    Class
                    {getSortIcon('class')}
                  </Button>
                </TableHead>
                <TableHead>
                  <Button
                    variant="ghost"
                    onClick={() => handleSort('examCenter')}
                    className="font-medium hover:bg-transparent p-0"
                  >
                    Exam Center
                    {getSortIcon('examCenter')}
                  </Button>
                </TableHead>
                <TableHead>
                  <Button
                    variant="ghost"
                    onClick={() => handleSort('status')}
                    className="font-medium hover:bg-transparent p-0"
                  >
                    Status
                    {getSortIcon('status')}
                  </Button>
                </TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredCards.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                    No admit cards found
                  </TableCell>
                </TableRow>
              ) : (
                filteredCards.map((card) => (
                  <TableRow key={card.id}>
                    <TableCell className="font-medium">{card.rollNumber}</TableCell>
                    <TableCell>{card.studentName}</TableCell>
                    <TableCell>{card.class}</TableCell>
                    <TableCell>{card.examCenter}</TableCell>
                    <TableCell>{getStatusBadge(card.status)}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleView(card)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handlePrint(card)}
                        >
                          <Printer className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleDownload(card)}
                        >
                          <Download className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>
      </Card>

      {/* Admit Card Details Dialog */}
      <Dialog open={!!selectedCard} onOpenChange={() => setSelectedCard(null)}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
          <DialogHeader className="print:hidden">
            <DialogTitle>Admit Card Details</DialogTitle>
          </DialogHeader>

          {selectedCard && (
            <div className="print:p-8">
              <Card className="p-8 bg-background border-none print:border print:shadow-none">
                {/* Header */}
                <div className="text-center mb-6 border-b pb-6">
                  <h2 className="text-2xl font-bold mb-1">ABC International School</h2>
                  <p className="text-sm text-muted-foreground">
                    123 Education Street, City - 123456
                  </p>
                  <h3 className="text-xl font-semibold mt-4">{selectedCard.examName}</h3>
                  <p className="text-sm font-medium mt-1">ADMIT CARD</p>
                </div>

                {/* Student Information */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                  <div className="md:col-span-2 space-y-3">
                    <div className="grid grid-cols-2 gap-x-4 gap-y-3">
                      <div>
                        <p className="text-sm text-muted-foreground">Student Name</p>
                        <p className="font-medium">{selectedCard.studentName}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Roll Number</p>
                        <p className="font-medium">{selectedCard.rollNumber}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Class</p>
                        <p className="font-medium">{selectedCard.class}</p>
                      </div>
                      <div>
                        <p className="text-sm text-muted-foreground">Exam Center</p>
                        <p className="font-medium">{selectedCard.examCenter}</p>
                      </div>
                    </div>
                  </div>
                  <div className="flex justify-center md:justify-end">
                    <div className="w-32 h-32 border-2 border-border rounded flex items-center justify-center bg-muted">
                      <p className="text-xs text-muted-foreground text-center">
                        Student
                        <br />
                        Photo
                      </p>
                    </div>
                  </div>
                </div>

                {/* Exam Schedule */}
                <div className="mb-6">
                  <h4 className="font-semibold mb-3 text-lg">Examination Schedule</h4>
                  <div className="border rounded-lg overflow-hidden">
                    <table className="w-full">
                      <thead className="bg-muted">
                        <tr>
                          <th className="px-4 py-3 text-left text-sm font-medium">Subject</th>
                          <th className="px-4 py-3 text-left text-sm font-medium">Date</th>
                          <th className="px-4 py-3 text-left text-sm font-medium">Time</th>
                          <th className="px-4 py-3 text-left text-sm font-medium">Duration</th>
                        </tr>
                      </thead>
                      <tbody>
                        {selectedCard.examSchedule.map((exam, index) => (
                          <tr key={index} className="border-t">
                            <td className="px-4 py-3 text-sm">{exam.subject}</td>
                            <td className="px-4 py-3 text-sm">
                              {new Date(exam.date).toLocaleDateString('en-US', {
                                day: '2-digit',
                                month: 'short',
                                year: 'numeric',
                              })}
                            </td>
                            <td className="px-4 py-3 text-sm">{exam.time}</td>
                            <td className="px-4 py-3 text-sm">{exam.duration}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Instructions */}
                <div className="border-t pt-6">
                  <h4 className="font-semibold mb-3">Important Instructions</h4>
                  <ul className="text-sm space-y-2 text-muted-foreground">
                    <li>• Bring this admit card to the examination center.</li>
                    <li>• Report to the examination center 30 minutes before the exam starts.</li>
                    <li>• Carry a valid ID proof along with the admit card.</li>
                    <li>• Electronic devices are strictly prohibited in the examination hall.</li>
                    <li>• Follow all instructions given by the examination supervisors.</li>
                  </ul>
                </div>

                {/* Footer */}
                <div className="mt-8 pt-6 border-t flex justify-between items-end">
                  <div>
                    <p className="text-sm text-muted-foreground mb-1">Issue Date</p>
                    <p className="text-sm font-medium">
                      {new Date().toLocaleDateString('en-US', {
                        day: '2-digit',
                        month: 'short',
                        year: 'numeric',
                      })}
                    </p>
                  </div>
                  <div className="text-right">
                    <div className="border-t border-foreground pt-1 w-40">
                      <p className="text-xs text-muted-foreground">Principal's Signature</p>
                    </div>
                  </div>
                </div>
              </Card>

              <div className="flex justify-end gap-2 mt-4 print:hidden">
                <Button onClick={() => handlePrint(selectedCard)} variant="outline">
                  <Printer className="mr-2 h-4 w-4" />
                  Print
                </Button>
                <Button onClick={() => handleDownload(selectedCard)} variant="outline">
                  <Download className="mr-2 h-4 w-4" />
                  Download
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
