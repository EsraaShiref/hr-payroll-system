export interface AttendanceRecordDto {
  id: string;
  employeeId: string;
  employeeName: string;
  date: string;
  clockIn: string | null;
  clockOut: string | null;
  breakDurationMinutes: number;
  workedMinutes: number;
  status: string;
  notes: string | null;
}

export interface AttendanceSummaryDto {
  employeeId: string;
  employeeName: string;
  year: number;
  month: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  halfDays: number;
  leaveDays: number;
  totalWorkedMinutes: number;
}

export interface LeaveRequestDto {
  id: string;
  employeeId: string;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  status: string;
  reason: string | null;
  rejectionReason: string | null;
  createdAt: string;
}

export interface LeaveBalanceDto {
  id: string;
  employeeId: string;
  leaveType: string;
  year: number;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
}
