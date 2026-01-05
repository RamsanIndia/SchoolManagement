import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Users, UserCheck, UserX, Clock } from "lucide-react";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from "recharts";

const mockStats = {
  totalStudents: 850,
  present: 760,
  absent: 70,
  late: 20,
  percentage: 89.4,
};

const mockClassWise = [
  { class: "1", present: 27, absent: 2, late: 1 },
  { class: "5", present: 30, absent: 3, late: 2 },
  { class: "8", present: 35, absent: 3, late: 2 },
  { class: "10", present: 40, absent: 7, late: 3 },
];

const COLORS = {
  present: 'hsl(142, 76%, 36%)',
  absent: 'hsl(0, 84%, 60%)',
  late: 'hsl(25, 95%, 53%)',
};

export default function AttendanceDashboard() {
  const pieData = [
    { name: 'Present', value: mockStats.present, color: COLORS.present },
    { name: 'Absent', value: mockStats.absent, color: COLORS.absent },
    { name: 'Late', value: mockStats.late, color: COLORS.late },
  ];

  const barData = mockClassWise.map(item => ({
    class: `Class ${item.class}`,
    Present: item.present,
    Absent: item.absent,
    Late: item.late,
  }));

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Attendance Dashboard</h1>
        <p className="text-muted-foreground">Overview of today's attendance across all classes</p>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Users className="h-4 w-4" />
              Total Students
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{mockStats.totalStudents}</div>
            <p className="text-xs text-muted-foreground mt-1">Enrolled students</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <UserCheck className="h-4 w-4" />
              Present
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-present">{mockStats.present}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {((mockStats.present / mockStats.totalStudents) * 100).toFixed(1)}% of total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <UserX className="h-4 w-4" />
              Absent
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-absent">{mockStats.absent}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {((mockStats.absent / mockStats.totalStudents) * 100).toFixed(1)}% of total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Late
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-status-late">{mockStats.late}</div>
            <p className="text-xs text-muted-foreground mt-1">
              {((mockStats.late / mockStats.totalStudents) * 100).toFixed(1)}% of total
            </p>
          </CardContent>
        </Card>

        <Card className="bg-gradient-primary">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-white/80">
              Attendance Rate
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-white">{mockStats.percentage}%</div>
            <p className="text-xs text-white/70 mt-1">Today's overall rate</p>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Class-wise Attendance Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={barData}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
                <XAxis dataKey="class" className="text-xs" />
                <YAxis className="text-xs" />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: 'hsl(var(--card))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: '8px'
                  }}
                />
                <Legend />
                <Bar dataKey="Present" fill={COLORS.present} radius={[4, 4, 0, 0]} />
                <Bar dataKey="Absent" fill={COLORS.absent} radius={[4, 4, 0, 0]} />
                <Bar dataKey="Late" fill={COLORS.late} radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Overall Status Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={pieData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                  outerRadius={100}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {pieData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: 'hsl(var(--card))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: '8px'
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Class-wise Summary Table */}
      <Card>
        <CardHeader>
          <CardTitle>Class-wise Summary</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-3 px-4 font-medium">Class</th>
                  <th className="text-right py-3 px-4 font-medium">Total</th>
                  <th className="text-right py-3 px-4 font-medium text-status-present">Present</th>
                  <th className="text-right py-3 px-4 font-medium text-status-absent">Absent</th>
                  <th className="text-right py-3 px-4 font-medium text-status-late">Late</th>
                  <th className="text-right py-3 px-4 font-medium">Attendance %</th>
                </tr>
              </thead>
              <tbody>
                {mockClassWise.map((item) => {
                  const total = item.present + item.absent + item.late;
                  const percentage = ((item.present + item.late) / total * 100).toFixed(1);
                  return (
                    <tr key={item.class} className="border-b hover:bg-muted/30 transition-colors">
                      <td className="py-3 px-4 font-medium">Class {item.class}</td>
                      <td className="text-right py-3 px-4">{total}</td>
                      <td className="text-right py-3 px-4 text-status-present font-semibold">{item.present}</td>
                      <td className="text-right py-3 px-4 text-status-absent font-semibold">{item.absent}</td>
                      <td className="text-right py-3 px-4 text-status-late font-semibold">{item.late}</td>
                      <td className="text-right py-3 px-4 font-semibold">{percentage}%</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
