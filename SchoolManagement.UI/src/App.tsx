import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import { NavigationProvider } from "./contexts/NavigationContext";
import { Layout } from "./components/layout/Layout";
import Login from "./pages/Login";
import Dashboard from "./pages/Dashboard";
import Students from "./pages/Students";
import AttendanceDashboard from "./pages/attendance/AttendanceDashboard";
import MarkAttendance from "./pages/attendance/MarkAttendance";
import MonthlyCalendar from "./pages/attendance/MonthlyCalendar";
import StudentDetail from "./pages/attendance/StudentDetail";
import AttendanceReports from "./pages/attendance/AttendanceReports";
import AttendanceSettings from "./pages/attendance/AttendanceSettings";
import UserManagement from "./pages/UserManagement";
import RoleManagement from "./pages/RoleManagement";
import PermissionMatrix from "./pages/PermissionMatrix";
import ExamDashboard from "./pages/examinations/ExamDashboard";
import ExamSchedule from "./pages/examinations/ExamSchedule";
import MarksEntry from "./pages/examinations/MarksEntry";
import ResultSummary from "./pages/examinations/ResultSummary";
import FeeDashboard from "./pages/fees/FeeDashboard";
import FeeMasterSetup from "./pages/fees/FeeMasterSetup";
import FeeStructureSetup from "./pages/fees/FeeStructureSetup";
import StudentFeeDetails from "./pages/fees/StudentFeeDetails";
import FeeCollection from "./pages/fees/FeeCollection";
import DefaultersList from "./pages/fees/DefaultersList";
import FeeReports from "./pages/fees/FeeReports";
import FeeManagement from "./pages/FeeManagement";
import EmployeeManagement from "./pages/EmployeeManagement";
import ClassManagement from "./pages/ClassManagement";
import ClassList from "./pages/classes/ClassList";
import SectionManagement from "./pages/classes/SectionManagement";
import AssignClassTeacher from "./pages/classes/AssignClassTeacher";
import SubjectMapping from "./pages/classes/SubjectMapping";
import ClassTimetable from "./pages/classes/ClassTimetable";
import ClassStudentList from "./pages/classes/ClassStudentList";
import TimetableBuilder from "./pages/classes/TimetableBuilder";
import StudentImportExport from "./pages/classes/StudentImportExport";
import TeacherWorkload from "./pages/classes/TeacherWorkload";
import AutoScheduler from "./pages/classes/AutoScheduler";
import RoomAllocation from "./pages/classes/RoomAllocation";
import DepartmentManagement from "./pages/DepartmentManagement";
import Payroll from "./pages/Payroll";
import ExamResults from "./pages/ExamResults";
import NotFound from "./pages/NotFound";
import Settings from "./pages/Settings";
import Notifications from "./pages/Notifications";
import Reports from "./pages/Reports";
import StudentAssignments from "./pages/StudentAssignments";
import ProgressReports from "./pages/ProgressReports";
import AcademicCalendar from "./pages/AcademicCalendar";
import AdmitCard from "./pages/AdmitCard";
import HRDashboard from "./pages/hr/HRDashboard";
import EmployeeDetails from "./pages/hr/EmployeeDetails";
import AttendanceManagement from "./pages/hr/AttendanceManagement";
import LeaveManagement from "./pages/hr/LeaveManagement";
import PerformanceManagement from "./pages/hr/PerformanceManagement";
import UserDashboard from "./pages/users/UserDashboard";
import UsersList from "./pages/users/UsersList";
import UserForm from "./pages/users/UserForm";
import UserProfile from "./pages/users/UserProfile";
import RolesList from "./pages/users/RolesList";
import RoleForm from "./pages/users/RoleForm";
import PermissionsManagement from "./pages/users/PermissionsManagement";
import MenuAccessControl from "./pages/users/MenuAccessControl";
import AuditTrail from "./pages/users/AuditTrail";
import SubstitutionManagement from "./pages/hr/SubstitutionManagement";
import StudentPortal from "./pages/StudentPortal";
// Student & Parent Management Module
import StudentProfile from "./pages/students/StudentProfile";
import StudentForm from "./pages/students/StudentForm";
import PromotionSetup from "./pages/students/PromotionSetup";
import PromotionMapping from "./pages/students/PromotionMapping";
import PromotionPreview from "./pages/students/PromotionPreview";
import ReportCardTemplates from "./pages/students/ReportCardTemplates";
import GenerateReportCards from "./pages/students/GenerateReportCards";
import ReportCardView from "./pages/students/ReportCardView";
import ParentDashboard from "./pages/parent/ParentDashboard";
import ParentTeacherChat from "./pages/parent/ParentTeacherChat";
import LeaveApply from "./pages/parent/LeaveApply";
import LeaveApproval from "./pages/leave/LeaveApproval";
import TeacherAssignments from "./pages/assignments/TeacherAssignments";
// Transport Management Module
import TransportDashboard from "./pages/transport/TransportDashboard";
import RouteManagement from "./pages/transport/RouteManagement";
import VehicleManagement from "./pages/transport/VehicleManagement";
import DriverManagement from "./pages/transport/DriverManagement";
import VehicleTracking from "./pages/transport/VehicleTracking";
import TransportReports from "./pages/transport/TransportReports";
// Hostel Management Module
import HostelDashboard from "./pages/hostel/HostelDashboard";
import HostelRoomAllocation from "./pages/hostel/RoomAllocation";
import MessMenu from "./pages/hostel/MessMenu";
import VisitorTracking from "./pages/hostel/VisitorTracking";
import HostelReports from "./pages/hostel/HostelReports";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const { user, isLoading, isAuthenticated } = useAuth();
  
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-primary">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
          <p className="text-white text-sm">Loading...</p>
        </div>
      </div>
    );
  }
  
  return isAuthenticated && user ? <Layout>{children}</Layout> : <Navigate to="/login" replace />;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      
      {/* Dashboard */}
      <Route path="/dashboard" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
      
      {/* Students Management */}
      <Route path="/students" element={<PrivateRoute><Students /></PrivateRoute>} />
      <Route path="/students/add" element={<PrivateRoute><StudentForm /></PrivateRoute>} />
      <Route path="/students/edit/:id" element={<PrivateRoute><StudentForm /></PrivateRoute>} />
      <Route path="/students/profile/:id" element={<PrivateRoute><StudentProfile /></PrivateRoute>} />
      <Route path="/students/promotion" element={<PrivateRoute><PromotionSetup /></PrivateRoute>} />
      <Route path="/students/promotion/mapping" element={<PrivateRoute><PromotionMapping /></PrivateRoute>} />
      <Route path="/students/promotion/preview" element={<PrivateRoute><PromotionPreview /></PrivateRoute>} />
      <Route path="/students/report-cards/templates" element={<PrivateRoute><ReportCardTemplates /></PrivateRoute>} />
      <Route path="/students/report-cards/generate" element={<PrivateRoute><GenerateReportCards /></PrivateRoute>} />
      <Route path="/students/report-cards/view/:id" element={<PrivateRoute><ReportCardView /></PrivateRoute>} />
      <Route path="/students/assignments" element={<PrivateRoute><StudentAssignments /></PrivateRoute>} />
      <Route path="/students/progress" element={<PrivateRoute><ProgressReports /></PrivateRoute>} />
      
      {/* Attendance Management */}
      <Route path="/attendance" element={<PrivateRoute><AttendanceDashboard /></PrivateRoute>} />
      <Route path="/attendance/mark" element={<PrivateRoute><MarkAttendance /></PrivateRoute>} />
      <Route path="/attendance/calendar" element={<PrivateRoute><MonthlyCalendar /></PrivateRoute>} />
      <Route path="/attendance/student-detail" element={<PrivateRoute><StudentDetail /></PrivateRoute>} />
      <Route path="/attendance/reports" element={<PrivateRoute><AttendanceReports /></PrivateRoute>} />
      <Route path="/attendance/settings" element={<PrivateRoute><AttendanceSettings /></PrivateRoute>} />
      
      {/* Examinations */}
      <Route path="/examinations" element={<PrivateRoute><ExamDashboard /></PrivateRoute>} />
      <Route path="/examinations/schedule" element={<PrivateRoute><ExamSchedule /></PrivateRoute>} />
      <Route path="/examinations/marks-entry" element={<PrivateRoute><MarksEntry /></PrivateRoute>} />
      <Route path="/examinations/results" element={<PrivateRoute><ResultSummary /></PrivateRoute>} />
      
      {/* Fee Management */}
      <Route path="/fees" element={<PrivateRoute><FeeDashboard /></PrivateRoute>} />
      <Route path="/fees/master" element={<PrivateRoute><FeeMasterSetup /></PrivateRoute>} />
      <Route path="/fees/structure" element={<PrivateRoute><FeeStructureSetup /></PrivateRoute>} />
      <Route path="/fees/student-details" element={<PrivateRoute><StudentFeeDetails /></PrivateRoute>} />
      <Route path="/fees/collection" element={<PrivateRoute><FeeCollection /></PrivateRoute>} />
      <Route path="/fees/defaulters" element={<PrivateRoute><DefaultersList /></PrivateRoute>} />
      <Route path="/fees/reports" element={<PrivateRoute><FeeReports /></PrivateRoute>} />
      
      {/* HR Management */}
      <Route path="/hr" element={<PrivateRoute><HRDashboard /></PrivateRoute>} />
      <Route path="/hr/employees" element={<PrivateRoute><EmployeeManagement /></PrivateRoute>} />
      <Route path="/hr/employees/:id" element={<PrivateRoute><EmployeeDetails /></PrivateRoute>} />
      <Route path="/hr/attendance" element={<PrivateRoute><AttendanceManagement /></PrivateRoute>} />
      <Route path="/hr/leaves" element={<PrivateRoute><LeaveManagement /></PrivateRoute>} />
      <Route path="/hr/payroll" element={<PrivateRoute><Payroll /></PrivateRoute>} />
      <Route path="/hr/performance" element={<PrivateRoute><PerformanceManagement /></PrivateRoute>} />
      <Route path="/hr/departments" element={<PrivateRoute><DepartmentManagement /></PrivateRoute>} />
      <Route path="/hr/substitution" element={<PrivateRoute><SubstitutionManagement /></PrivateRoute>} />
      
      {/* Class Management */}
      <Route path="/classes" element={<PrivateRoute><ClassList /></PrivateRoute>} />
      <Route path="/classes/:classId/sections" element={<PrivateRoute><SectionManagement /></PrivateRoute>} />
      <Route path="/classes/:classId/assign-teacher" element={<PrivateRoute><AssignClassTeacher /></PrivateRoute>} />
      <Route path="/classes/:classId/subjects" element={<PrivateRoute><SubjectMapping /></PrivateRoute>} />
      <Route path="/classes/:classId/timetable" element={<PrivateRoute><ClassTimetable /></PrivateRoute>} />
      <Route path="/classes/:classId/students" element={<PrivateRoute><ClassStudentList /></PrivateRoute>} />
      <Route path="/classes/timetable-builder" element={<PrivateRoute><TimetableBuilder /></PrivateRoute>} />
      <Route path="/classes/import-export" element={<PrivateRoute><StudentImportExport /></PrivateRoute>} />
      <Route path="/classes/teacher-workload" element={<PrivateRoute><TeacherWorkload /></PrivateRoute>} />
      <Route path="/classes/auto-scheduler" element={<PrivateRoute><AutoScheduler /></PrivateRoute>} />
      <Route path="/classes/room-allocation" element={<PrivateRoute><RoomAllocation /></PrivateRoute>} />
      
      {/* User Management */}
      <Route path="/users" element={<PrivateRoute><UserDashboard /></PrivateRoute>} />
      <Route path="/users/list" element={<PrivateRoute><UsersList /></PrivateRoute>} />
      <Route path="/users/add" element={<PrivateRoute><UserForm /></PrivateRoute>} />
      <Route path="/users/edit/:id" element={<PrivateRoute><UserForm /></PrivateRoute>} />
      <Route path="/users/profile/:id" element={<PrivateRoute><UserProfile /></PrivateRoute>} />
      <Route path="/roles" element={<PrivateRoute><RolesList /></PrivateRoute>} />
      <Route path="/roles/add" element={<PrivateRoute><RoleForm /></PrivateRoute>} />
      <Route path="/roles/edit/:id" element={<PrivateRoute><RoleForm /></PrivateRoute>} />
      <Route path="/permissions" element={<PrivateRoute><PermissionsManagement /></PrivateRoute>} />
      <Route path="/permissions/matrix" element={<PrivateRoute><PermissionMatrix /></PrivateRoute>} />
      <Route path="/menu-access" element={<PrivateRoute><MenuAccessControl /></PrivateRoute>} />
      <Route path="/audit-trail" element={<PrivateRoute><AuditTrail /></PrivateRoute>} />
      
      {/* Parent Portal */}
      <Route path="/parent" element={<PrivateRoute><ParentDashboard /></PrivateRoute>} />
      <Route path="/parent/chat" element={<PrivateRoute><ParentTeacherChat /></PrivateRoute>} />
      <Route path="/parent/leave-apply" element={<PrivateRoute><LeaveApply /></PrivateRoute>} />
      
      {/* Leave & Assignments */}
      <Route path="/leave/approval" element={<PrivateRoute><LeaveApproval /></PrivateRoute>} />
      <Route path="/assignments/manage" element={<PrivateRoute><TeacherAssignments /></PrivateRoute>} />
      
      {/* Transport Management */}
      <Route path="/transport" element={<PrivateRoute><TransportDashboard /></PrivateRoute>} />
      <Route path="/transport/routes" element={<PrivateRoute><RouteManagement /></PrivateRoute>} />
      <Route path="/transport/vehicles" element={<PrivateRoute><VehicleManagement /></PrivateRoute>} />
      <Route path="/transport/drivers" element={<PrivateRoute><DriverManagement /></PrivateRoute>} />
      <Route path="/transport/tracking" element={<PrivateRoute><VehicleTracking /></PrivateRoute>} />
      <Route path="/transport/reports" element={<PrivateRoute><TransportReports /></PrivateRoute>} />
      
      {/* Hostel Management */}
      <Route path="/hostel" element={<PrivateRoute><HostelDashboard /></PrivateRoute>} />
      <Route path="/hostel/rooms" element={<PrivateRoute><HostelRoomAllocation /></PrivateRoute>} />
      <Route path="/hostel/mess" element={<PrivateRoute><MessMenu /></PrivateRoute>} />
      <Route path="/hostel/visitors" element={<PrivateRoute><VisitorTracking /></PrivateRoute>} />
      <Route path="/hostel/reports" element={<PrivateRoute><HostelReports /></PrivateRoute>} />
      
      {/* Common Routes */}
      <Route path="/student-portal" element={<PrivateRoute><StudentPortal /></PrivateRoute>} />
      <Route path="/notifications" element={<PrivateRoute><Notifications /></PrivateRoute>} />
      <Route path="/settings" element={<PrivateRoute><Settings /></PrivateRoute>} />
      <Route path="/reports" element={<PrivateRoute><Reports /></PrivateRoute>} />
      <Route path="/calendar" element={<PrivateRoute><AcademicCalendar /></PrivateRoute>} />
      <Route path="/admit-card" element={<PrivateRoute><AdmitCard /></PrivateRoute>} />
      
      {/* Default & Not Found Routes */}
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

const App = () => (
  <QueryClientProvider client={queryClient}>
    <BrowserRouter>
      <AuthProvider>
        <NavigationProvider>
          <TooltipProvider>
            <Toaster />
            <Sonner />
            <AppRoutes />
          </TooltipProvider>
        </NavigationProvider>
      </AuthProvider>
    </BrowserRouter>
  </QueryClientProvider>
);

export default App;
