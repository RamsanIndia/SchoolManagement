import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { PerformanceReview, PerformanceGoal } from "@/types/navigation.types";
import { useToast } from "@/hooks/use-toast";
import { 
  TrendingUp, Search, Plus, Award, Target, Star,
  ArrowUpDown, ArrowUp, ArrowDown, Eye, Calendar
} from "lucide-react";

export default function PerformanceManagement() {
  const [reviews, setReviews] = useState<PerformanceReview[]>([]);
  const [goals, setGoals] = useState<PerformanceGoal[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [viewMode, setViewMode] = useState<"reviews" | "goals">("reviews");
  const [sortColumn, setSortColumn] = useState<string>("");
  const [sortDirection, setSortDirection] = useState<"asc" | "desc">("asc");
  const { toast } = useToast();

  useEffect(() => {
    loadPerformanceData();
  }, []);

  const loadPerformanceData = () => {
    // Mock performance reviews
    const mockReviews: PerformanceReview[] = [
      {
        id: "1",
        employeeId: "EMP001",
        employeeName: "John Doe",
        reviewerId: "MGR001",
        reviewerName: "Sarah Manager",
        reviewPeriod: "2023-Q4",
        reviewType: "quarterly",
        reviewDate: "2024-01-15",
        ratings: [
          { category: "Job Knowledge", rating: 4.5, maxRating: 5, comments: "Excellent technical expertise" },
          { category: "Quality of Work", rating: 4, maxRating: 5, comments: "Consistently high quality" },
          { category: "Communication", rating: 4.5, maxRating: 5, comments: "Great team collaboration" },
          { category: "Initiative", rating: 3.5, maxRating: 5, comments: "Could be more proactive" },
          { category: "Leadership", rating: 4, maxRating: 5, comments: "Shows leadership potential" },
        ],
        overallRating: 4.1,
        strengths: ["Strong technical skills", "Good team player", "Reliable and punctual"],
        areasForImprovement: ["Take more initiative", "Improve time management", "Public speaking"],
        goals: ["Lead a project team", "Mentor junior staff", "Complete leadership training"],
        trainingNeeds: ["Leadership training", "Project management", "Advanced communication skills"],
        managerComments: "John has shown excellent performance this quarter with strong technical contributions. He would benefit from taking on more leadership responsibilities.",
        status: "reviewed",
        nextReviewDate: "2024-04-15",
        createdAt: "2024-01-10T10:00:00Z",
        updatedAt: "2024-01-15T14:00:00Z",
      },
      {
        id: "2",
        employeeId: "EMP002",
        employeeName: "Jane Smith",
        reviewerId: "MGR001",
        reviewerName: "Sarah Manager",
        reviewPeriod: "2023-Q4",
        reviewType: "quarterly",
        reviewDate: "2024-01-20",
        ratings: [
          { category: "Job Knowledge", rating: 5, maxRating: 5, comments: "Subject matter expert" },
          { category: "Quality of Work", rating: 5, maxRating: 5, comments: "Exceptional quality" },
          { category: "Communication", rating: 5, maxRating: 5, comments: "Outstanding communicator" },
          { category: "Initiative", rating: 4.5, maxRating: 5, comments: "Very proactive" },
          { category: "Leadership", rating: 5, maxRating: 5, comments: "Natural leader" },
        ],
        overallRating: 4.9,
        strengths: ["Exceptional technical knowledge", "Outstanding leadership", "Excellent communication"],
        areasForImprovement: ["Work-life balance", "Delegation skills"],
        goals: ["Lead department initiative", "Develop succession plan"],
        trainingNeeds: ["Executive leadership program"],
        managerComments: "Jane continues to excel in all areas. She is ready for more senior responsibilities.",
        status: "reviewed",
        nextReviewDate: "2024-04-20",
        createdAt: "2024-01-15T10:00:00Z",
        updatedAt: "2024-01-20T14:00:00Z",
      },
      {
        id: "3",
        employeeId: "EMP003",
        employeeName: "Michael Brown",
        reviewerId: "MGR002",
        reviewerName: "Tom Director",
        reviewPeriod: "2023-Q4",
        reviewType: "quarterly",
        reviewDate: "2024-01-18",
        ratings: [
          { category: "Job Knowledge", rating: 3.5, maxRating: 5, comments: "Needs more technical depth" },
          { category: "Quality of Work", rating: 4, maxRating: 5, comments: "Good quality output" },
          { category: "Communication", rating: 3, maxRating: 5, comments: "Needs improvement" },
          { category: "Initiative", rating: 4, maxRating: 5, comments: "Shows good initiative" },
          { category: "Leadership", rating: 3, maxRating: 5, comments: "Developing leadership skills" },
        ],
        overallRating: 3.5,
        strengths: ["Good work ethic", "Willing to learn", "Team player"],
        areasForImprovement: ["Technical skills", "Communication", "Confidence"],
        goals: ["Complete technical certification", "Improve presentation skills"],
        trainingNeeds: ["Technical training", "Communication workshop"],
        managerComments: "Michael is progressing well but needs to focus on building technical depth and communication skills.",
        status: "reviewed",
        nextReviewDate: "2024-04-18",
        createdAt: "2024-01-12T10:00:00Z",
        updatedAt: "2024-01-18T14:00:00Z",
      },
      {
        id: "4",
        employeeId: "EMP004",
        employeeName: "Emily Davis",
        reviewerId: "MGR001",
        reviewerName: "Sarah Manager",
        reviewPeriod: "2023-Q4",
        reviewType: "quarterly",
        reviewDate: "",
        ratings: [],
        overallRating: 0,
        strengths: [],
        areasForImprovement: [],
        goals: [],
        trainingNeeds: [],
        managerComments: "",
        status: "draft",
        createdAt: "2024-01-25T10:00:00Z",
        updatedAt: "2024-01-25T10:00:00Z",
      },
    ];

    // Mock performance goals
    const mockGoals: PerformanceGoal[] = [
      {
        id: "g1",
        employeeId: "EMP001",
        title: "Complete Advanced Technical Certification",
        description: "Complete the advanced certification program in core technology stack",
        category: "individual",
        startDate: "2024-01-01",
        targetDate: "2024-06-30",
        status: "in-progress",
        progress: 65,
        metrics: ["Course completion", "Certification exam", "Apply knowledge to projects"],
        createdAt: "2024-01-01T00:00:00Z",
        updatedAt: "2024-01-15T00:00:00Z",
      },
      {
        id: "g2",
        employeeId: "EMP001",
        title: "Lead Department Project",
        description: "Successfully lead the Q2 department initiative from planning to delivery",
        category: "individual",
        startDate: "2024-02-01",
        targetDate: "2024-06-30",
        status: "not-started",
        progress: 0,
        metrics: ["Project planning", "Team coordination", "On-time delivery", "Budget adherence"],
        createdAt: "2024-01-15T00:00:00Z",
        updatedAt: "2024-01-15T00:00:00Z",
      },
      {
        id: "g3",
        employeeId: "EMP002",
        title: "Develop Succession Plan",
        description: "Create and implement a succession plan for the department",
        category: "organizational",
        startDate: "2024-01-15",
        targetDate: "2024-12-31",
        status: "in-progress",
        progress: 30,
        metrics: ["Identify key positions", "Develop training programs", "Mentor successors"],
        createdAt: "2024-01-15T00:00:00Z",
        updatedAt: "2024-01-20T00:00:00Z",
      },
      {
        id: "g4",
        employeeId: "EMP003",
        title: "Improve Communication Skills",
        description: "Complete communication workshop and deliver 5 presentations",
        category: "individual",
        startDate: "2024-01-01",
        targetDate: "2024-06-30",
        status: "in-progress",
        progress: 40,
        metrics: ["Workshop completion", "Presentation count", "Feedback scores"],
        createdAt: "2024-01-01T00:00:00Z",
        updatedAt: "2024-01-18T00:00:00Z",
      },
    ];

    setReviews(mockReviews);
    setGoals(mockGoals);
  };

  const handleSort = (column: string) => {
    if (sortColumn === column) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortColumn(column);
      setSortDirection("asc");
    }
  };

  const getSortIcon = (column: string) => {
    if (sortColumn !== column) return <ArrowUpDown className="h-4 w-4 ml-1" />;
    return sortDirection === "asc" ? <ArrowUp className="h-4 w-4 ml-1" /> : <ArrowDown className="h-4 w-4 ml-1" />;
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      draft: "bg-muted",
      submitted: "bg-blue-500",
      reviewed: "bg-status-present",
      acknowledged: "bg-purple-500",
      "not-started": "bg-muted",
      "in-progress": "bg-blue-500",
      completed: "bg-status-present",
      cancelled: "bg-status-absent",
    };
    return <Badge className={styles[status] || "bg-muted"}>{status}</Badge>;
  };

  const getRatingColor = (rating: number, maxRating: number) => {
    const percentage = (rating / maxRating) * 100;
    if (percentage >= 80) return "text-status-present";
    if (percentage >= 60) return "text-blue-500";
    if (percentage >= 40) return "text-amber-500";
    return "text-status-absent";
  };

  const filteredReviews = reviews
    .filter(review => {
      const matchesSearch = review.employeeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          review.employeeId.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter === "all" || review.status === statusFilter;
      return matchesSearch && matchesStatus;
    })
    .sort((a, b) => {
      if (!sortColumn) return 0;
      const aValue: any = a[sortColumn as keyof typeof a];
      const bValue: any = b[sortColumn as keyof typeof b];
      if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
      if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });

  const filteredGoals = goals
    .filter(goal => {
      const matchesSearch = goal.title.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter === "all" || goal.status === statusFilter;
      return matchesSearch && matchesStatus;
    });

  const reviewStats = {
    total: reviews.length,
    completed: reviews.filter(r => r.status === "reviewed" || r.status === "acknowledged").length,
    pending: reviews.filter(r => r.status === "draft" || r.status === "submitted").length,
    avgRating: reviews.filter(r => r.overallRating > 0).reduce((sum, r) => sum + r.overallRating, 0) / reviews.filter(r => r.overallRating > 0).length || 0,
  };

  const goalStats = {
    total: goals.length,
    completed: goals.filter(g => g.status === "completed").length,
    inProgress: goals.filter(g => g.status === "in-progress").length,
    avgProgress: goals.reduce((sum, g) => sum + g.progress, 0) / goals.length || 0,
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Performance Management</h1>
          <p className="text-muted-foreground">Track employee performance and goals</p>
        </div>
        <div className="flex gap-2">
          <Button
            variant={viewMode === "reviews" ? "default" : "outline"}
            onClick={() => setViewMode("reviews")}
          >
            <Award className="mr-2 h-4 w-4" />
            Reviews
          </Button>
          <Button
            variant={viewMode === "goals" ? "default" : "outline"}
            onClick={() => setViewMode("goals")}
          >
            <Target className="mr-2 h-4 w-4" />
            Goals
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      {viewMode === "reviews" ? (
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Award className="h-4 w-4" />
                Total Reviews
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{reviewStats.total}</div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <TrendingUp className="h-4 w-4" />
                Completed
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-status-present">{reviewStats.completed}</div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Pending
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-amber-500">{reviewStats.pending}</div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Star className="h-4 w-4" />
                Avg Rating
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{reviewStats.avgRating.toFixed(1)}/5</div>
            </CardContent>
          </Card>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Target className="h-4 w-4" />
                Total Goals
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{goalStats.total}</div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <TrendingUp className="h-4 w-4" />
                In Progress
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-blue-500">{goalStats.inProgress}</div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                <Award className="h-4 w-4" />
                Completed
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-status-present">{goalStats.completed}</div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Avg Progress
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{goalStats.avgProgress.toFixed(0)}%</div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Main Content */}
      <Card>
        <CardHeader>
          <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              {viewMode === "reviews" ? (
                <>
                  <Award className="h-5 w-5" />
                  Performance Reviews
                </>
              ) : (
                <>
                  <Target className="h-5 w-5" />
                  Performance Goals
                </>
              )}
            </CardTitle>
            <div className="flex flex-wrap gap-2 items-center w-full md:w-auto">
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-[150px]">
                  <SelectValue placeholder="Filter by status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  {viewMode === "reviews" ? (
                    <>
                      <SelectItem value="draft">Draft</SelectItem>
                      <SelectItem value="submitted">Submitted</SelectItem>
                      <SelectItem value="reviewed">Reviewed</SelectItem>
                      <SelectItem value="acknowledged">Acknowledged</SelectItem>
                    </>
                  ) : (
                    <>
                      <SelectItem value="not-started">Not Started</SelectItem>
                      <SelectItem value="in-progress">In Progress</SelectItem>
                      <SelectItem value="completed">Completed</SelectItem>
                      <SelectItem value="cancelled">Cancelled</SelectItem>
                    </>
                  )}
                </SelectContent>
              </Select>

              <div className="flex items-center gap-2">
                <Search className="h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder={viewMode === "reviews" ? "Search reviews..." : "Search goals..."}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-[200px]"
                />
              </div>

              <Button className="bg-gradient-primary">
                <Plus className="mr-2 h-4 w-4" />
                {viewMode === "reviews" ? "New Review" : "New Goal"}
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {viewMode === "reviews" ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead 
                      className="cursor-pointer select-none hover:bg-muted/50"
                      onClick={() => handleSort("employeeName")}
                    >
                      <div className="flex items-center">
                        Employee
                        {getSortIcon("employeeName")}
                      </div>
                    </TableHead>
                    <TableHead 
                      className="cursor-pointer select-none hover:bg-muted/50"
                      onClick={() => handleSort("reviewPeriod")}
                    >
                      <div className="flex items-center">
                        Period
                        {getSortIcon("reviewPeriod")}
                      </div>
                    </TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead 
                      className="cursor-pointer select-none hover:bg-muted/50"
                      onClick={() => handleSort("overallRating")}
                    >
                      <div className="flex items-center">
                        Rating
                        {getSortIcon("overallRating")}
                      </div>
                    </TableHead>
                    <TableHead>Reviewer</TableHead>
                    <TableHead 
                      className="cursor-pointer select-none hover:bg-muted/50"
                      onClick={() => handleSort("status")}
                    >
                      <div className="flex items-center">
                        Status
                        {getSortIcon("status")}
                      </div>
                    </TableHead>
                    <TableHead>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredReviews.map((review) => (
                    <TableRow key={review.id}>
                      <TableCell>
                        <div>
                          <div className="font-medium">{review.employeeName}</div>
                          <div className="text-sm text-muted-foreground">{review.employeeId}</div>
                        </div>
                      </TableCell>
                      <TableCell>{review.reviewPeriod}</TableCell>
                      <TableCell className="capitalize">{review.reviewType}</TableCell>
                      <TableCell>
                        {review.overallRating > 0 ? (
                          <div className="flex items-center gap-2">
                            <span className={`text-lg font-bold ${getRatingColor(review.overallRating, 5)}`}>
                              {review.overallRating.toFixed(1)}
                            </span>
                            <span className="text-sm text-muted-foreground">/5</span>
                          </div>
                        ) : (
                          <span className="text-muted-foreground">-</span>
                        )}
                      </TableCell>
                      <TableCell>{review.reviewerName}</TableCell>
                      <TableCell>{getStatusBadge(review.status)}</TableCell>
                      <TableCell>
                        <Button variant="outline" size="sm">
                          <Eye className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="space-y-4">
              {filteredGoals.map((goal) => (
                <Card key={goal.id}>
                  <CardContent className="pt-6">
                    <div className="flex justify-between items-start mb-4">
                      <div className="flex-1">
                        <h4 className="font-semibold text-lg">{goal.title}</h4>
                        <p className="text-sm text-muted-foreground mt-1">{goal.description}</p>
                        <div className="flex gap-2 mt-2">
                          <Badge variant="outline" className="capitalize">{goal.category}</Badge>
                          {getStatusBadge(goal.status)}
                        </div>
                      </div>
                      <Button variant="outline" size="sm">
                        <Eye className="h-4 w-4" />
                      </Button>
                    </div>

                    <div className="space-y-2 mb-4">
                      <div className="flex justify-between text-sm">
                        <span className="text-muted-foreground">Progress</span>
                        <span className="font-medium">{goal.progress}%</span>
                      </div>
                      <Progress value={goal.progress} />
                    </div>

                    <div className="flex justify-between text-sm text-muted-foreground">
                      <span>Start: {new Date(goal.startDate).toLocaleDateString()}</span>
                      <span>Target: {new Date(goal.targetDate).toLocaleDateString()}</span>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
