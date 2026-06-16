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

export interface AttendanceExceptionDto {
  id: string;
  employeeName: string;
  employeeCode: string;
  date: string;
  exceptionType: string;
  severity: string;
  details: string;
  summaryId: string | null;
  canOverride: boolean;
}

export interface OverrideSummaryRequest {
  summaryId: string;
  overridePunchIn: string | null;
  overridePunchOut: string | null;
  reason: string;
}

export interface AttendanceViewerItemDto {
  id: string;
  date: string;
  status: string;
  firstPunchIn: string | null;
  lastPunchOut: string | null;
  netWorkedMinutes: number;
  lateMinutes: number;
  earlyDepartureMinutes: number;
  overtimeMinutes: number;
  isOnLeave: boolean;
  isHoliday: boolean;
  notes: string | null;
}

export interface AttendanceViewerSummaryDto {
  totalPresentDays: number;
  totalLateOccurrences: number;
  totalAbsentDays: number;
  totalLeaveDays: number;
  totalHolidayDays: number;
  totalOvertimeHours: number;
  totalWorkedMinutes: number;
}

export interface AttendanceViewerResult {
  employeeId: string;
  employeeName: string;
  year: number;
  month: number;
  days: AttendanceViewerItemDto[];
  summary: AttendanceViewerSummaryDto;
}
