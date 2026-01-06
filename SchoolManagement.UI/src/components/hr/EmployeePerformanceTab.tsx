import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { PerformanceReview, PerformanceGoal } from "@/types/navigation.types";
import { TrendingUp, Target, Award, Plus } from "lucide-react";

interface Props {
  employeeId: string;
}

export default function EmployeePerformanceTab({ employeeId }: Props) {
  const [reviews, setReviews] = useState<PerformanceReview[]>([]);
  const [goals, setGoals] = useState<PerformanceGoal[]>([]);

  useEffect(() => {
    // Mock data - replace with actual API call
    const mockReviews: PerformanceReview[] = [
      {
        id: "1",
        employeeId,
        employeeName: "John Doe",
        reviewerId: "mgr-1",
        reviewerName: "Manager Name",
        reviewPeriod: "2023-Q4",
        reviewType: "quarterly",
        reviewDate: "2024-01-15",
        ratings: [
          { category: "Job Knowledge", rating: 4, maxRating: 5, comments: "Excellent understanding" },
          { category: "Quality of Work", rating: 4.5, maxRating: 5, comments: "Consistently high quality" },
          { category: "Communication", rating: 4, maxRating: 5, comments: "Good team collaboration" },
          { category: "Initiative", rating: 3.5, maxRating: 5, comments: "Could be more proactive" },
        ],
        overallRating: 4,
        strengths: ["Strong technical skills", "Good team player", "Reliable"],
        areasForImprovement: ["Take more initiative", "Improve time management"],
        goals: ["Lead a project", "Mentor junior staff"],
        trainingNeeds: ["Leadership training", "Project management"],
        managerComments: "Overall excellent performance with room for growth in leadership.",
        status: "reviewed",
        nextReviewDate: "2024-04-15",
        createdAt: "2024-01-10T10:00:00Z",
        updatedAt: "2024-01-15T14:00:00Z",
      },
    ];

    const mockGoals: PerformanceGoal[] = [
      {
        id: "g1",
        employeeId,
        title: "Complete Advanced Training Course",
        description: "Complete the advanced mathematics teaching methodology course",
        category: "individual",
        startDate: "2024-01-01",
        targetDate: "2024-06-30",
        status: "in-progress",
        progress: 60,
        metrics: ["Course completion", "Apply new methods in classroom"],
        createdAt: "2024-01-01T00:00:00Z",
        updatedAt: "2024-01-15T00:00:00Z",
      },
      {
        id: "g2",
        employeeId,
        title: "Improve Student Performance by 15%",
        description: "Help students achieve a 15% improvement in test scores",
        category: "individual",
        startDate: "2024-01-01",
        targetDate: "2024-12-31",
        status: "in-progress",
        progress: 40,
        metrics: ["Average test scores", "Student feedback"],
        createdAt: "2024-01-01T00:00:00Z",
        updatedAt: "2024-01-15T00:00:00Z",
      },
    ];

    setReviews(mockReviews);
    setGoals(mockGoals);
  }, [employeeId]);

  const getStatusBadge = (status: string) => {
    const colors: Record<string, string> = {
      "not-started": "bg-muted",
      "in-progress": "bg-blue-500",
      completed: "bg-status-present",
      cancelled: "bg-status-absent",
    };
    return <Badge className={colors[status] || "bg-muted"}>{status}</Badge>;
  };

  const getRatingColor = (rating: number, maxRating: number) => {
    const percentage = (rating / maxRating) * 100;
    if (percentage >= 80) return "text-status-present";
    if (percentage >= 60) return "text-blue-500";
    if (percentage >= 40) return "text-amber-500";
    return "text-status-absent";
  };

  return (
    <div className="space-y-4">
      {/* Performance Overview */}
      {reviews[0] && (
        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Award className="h-5 w-5" />
                Latest Performance Review
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <div className="flex justify-between items-center mb-2">
                  <span className="text-sm text-muted-foreground">Overall Rating</span>
                  <span className={`text-2xl font-bold ${getRatingColor(reviews[0].overallRating, 5)}`}>
                    {reviews[0].overallRating}/5
                  </span>
                </div>
                <Progress value={(reviews[0].overallRating / 5) * 100} />
              </div>

              <div className="space-y-2">
                {reviews[0].ratings.map((rating, index) => (
                  <div key={index}>
                    <div className="flex justify-between text-sm mb-1">
                      <span>{rating.category}</span>
                      <span className="font-medium">{rating.rating}/{rating.maxRating}</span>
                    </div>
                    <Progress value={(rating.rating / rating.maxRating) * 100} />
                  </div>
                ))}
              </div>

              <div className="text-sm text-muted-foreground">
                Review Period: {reviews[0].reviewPeriod}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <TrendingUp className="h-5 w-5" />
                Key Highlights
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <h4 className="font-semibold text-sm mb-2 text-status-present">Strengths</h4>
                <ul className="list-disc list-inside text-sm space-y-1">
                  {reviews[0].strengths.map((strength, index) => (
                    <li key={index}>{strength}</li>
                  ))}
                </ul>
              </div>

              <div>
                <h4 className="font-semibold text-sm mb-2 text-amber-500">Areas for Improvement</h4>
                <ul className="list-disc list-inside text-sm space-y-1">
                  {reviews[0].areasForImprovement.map((area, index) => (
                    <li key={index}>{area}</li>
                  ))}
                </ul>
              </div>

              <div>
                <h4 className="font-semibold text-sm mb-2">Manager Comments</h4>
                <p className="text-sm text-muted-foreground">{reviews[0].managerComments}</p>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Performance Goals */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-2">
              <Target className="h-5 w-5" />
              Performance Goals
            </CardTitle>
            <Button className="bg-gradient-primary">
              <Plus className="mr-2 h-4 w-4" />
              Add Goal
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {goals.map((goal) => (
              <Card key={goal.id}>
                <CardContent className="pt-6">
                  <div className="flex justify-between items-start mb-4">
                    <div className="flex-1">
                      <h4 className="font-semibold">{goal.title}</h4>
                      <p className="text-sm text-muted-foreground mt-1">{goal.description}</p>
                    </div>
                    {getStatusBadge(goal.status)}
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Progress</span>
                      <span className="font-medium">{goal.progress}%</span>
                    </div>
                    <Progress value={goal.progress} />
                  </div>

                  <div className="flex justify-between text-sm text-muted-foreground mt-4">
                    <span>Start: {new Date(goal.startDate).toLocaleDateString()}</span>
                    <span>Target: {new Date(goal.targetDate).toLocaleDateString()}</span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Review History */}
      <Card>
        <CardHeader>
          <CardTitle>Review History</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {reviews.map((review) => (
              <div key={review.id} className="flex justify-between items-center p-4 border rounded-lg">
                <div className="flex-1">
                  <div className="flex items-center gap-4">
                    <span className="font-medium">{review.reviewPeriod}</span>
                    <Badge>{review.reviewType}</Badge>
                  </div>
                  <p className="text-sm text-muted-foreground mt-1">
                    Reviewed by {review.reviewerName} on {new Date(review.reviewDate).toLocaleDateString()}
                  </p>
                </div>
                <div className="text-right">
                  <div className={`text-2xl font-bold ${getRatingColor(review.overallRating, 5)}`}>
                    {review.overallRating}/5
                  </div>
                  <Button variant="outline" size="sm" className="mt-2">
                    View Details
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
