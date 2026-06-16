export interface DashboardAttendanceSummary {
  totalPresent: number;
  totalAbsent: number;
  totalLate: number;
  totalOnLeave: number;
  totalHoliday: number;
  totalPendingReview: number;
  totalActiveEmployees: number;
  departmentBreakdown: DepartmentAttendance[];
}

export interface DepartmentAttendance {
  departmentName: string;
  present: number;
  absent: number;
  late: number;
  onLeave: number;
  total: number;
}

export interface PendingLeaveRequest {
  leaveRequestId: string;
  employeeId: string;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string | null;
  createdAt: string;
}

export interface PayrollBudgetSummary {
  year: number;
  month: number;
  projectedGrossPay: number;
  actualNetPay: number;
  actualGrossPay: number;
  actualTotalDeductions: number;
  actualEmployeeCount: number;
  departmentBreakdown: DepartmentBudget[];
}

export interface DepartmentBudget {
  departmentName: string;
  projectedBaseSalaries: number;
  actualNetPay: number;
  employeeCount: number;
}

export interface HeadcountTrend {
  months: MonthlyHeadcount[];
  currentHeadcount: number;
}

export interface MonthlyHeadcount {
  label: string;
  year: number;
  month: number;
  count: number;
  changeFromPrevious: number;
}

export interface UpcomingContractRenewal {
  contractId: string;
  employeeId: string;
  employeeName: string;
  contractType: string;
  expiryDate: string;
  daysRemaining: number;
}
