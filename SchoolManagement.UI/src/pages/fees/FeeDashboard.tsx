import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import { DollarSign, TrendingUp, AlertCircle, Users } from "lucide-react";

const mockDashboardData = {
  todaysCollection: 18500,
  monthlyCollection: 520000,
  outstanding: 142000,
  defaulters: 37,
  totalPending: 245,
  classWiseCollection: [
    { class: "Class 1", amount: 24000 },
    { class: "Class 2", amount: 27000 },
    { class: "Class 3", amount: 32000 },
    { class: "Class 4", amount: 29000 },
    { class: "Class 5", amount: 35000 },
    { class: "Class 6", amount: 38000 },
    { class: "Class 7", amount: 42000 },
    { class: "Class 8", amount: 45000 },
  ],
};

const StatCard = ({ title, value, icon: Icon, trend }: any) => (
  <Card>
    <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
      <CardTitle className="text-sm font-medium">{title}</CardTitle>
      <Icon className="h-4 w-4 text-muted-foreground" />
    </CardHeader>
    <CardContent>
      <div className="text-2xl font-bold">₹{value.toLocaleString()}</div>
      {trend && <p className="text-xs text-muted-foreground mt-1">{trend}</p>}
    </CardContent>
  </Card>
);

export default function FeeDashboard() {
  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Fee Dashboard</h1>
        <p className="text-muted-foreground">Overview of fee collections and outstanding amounts</p>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Today's Collection"
          value={mockDashboardData.todaysCollection}
          icon={DollarSign}
          trend="+12% from yesterday"
        />
        <StatCard
          title="Monthly Collection"
          value={mockDashboardData.monthlyCollection}
          icon={TrendingUp}
          trend="+8% from last month"
        />
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Outstanding Amount</CardTitle>
            <AlertCircle className="h-4 w-4 text-destructive" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-destructive">
              ₹{mockDashboardData.outstanding.toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {mockDashboardData.totalPending} students pending
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Defaulters</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{mockDashboardData.defaulters}</div>
            <p className="text-xs text-muted-foreground mt-1">Students with overdue fees</p>
          </CardContent>
        </Card>
      </div>

      {/* Class-wise Collection Chart */}
      <Card>
        <CardHeader>
          <CardTitle>Class-wise Fee Collection (This Month)</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={350}>
            <BarChart data={mockDashboardData.classWiseCollection}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="class" />
              <YAxis />
              <Tooltip
                formatter={(value: number) => `₹${value.toLocaleString()}`}
                contentStyle={{
                  backgroundColor: "hsl(var(--card))",
                  border: "1px solid hsl(var(--border))",
                  borderRadius: "6px",
                }}
              />
              <Bar dataKey="amount" fill="hsl(var(--primary))" radius={[8, 8, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>
    </div>
  );
}
