export interface MyPayslipDto {
  runId: string;
  year: number;
  month: number;
  period: string;
  completedAt: string | null;
  netPay: number;
  grossPay: number;
  totalDeductions: number;
  baseSalary: number;
}

export interface MyLeaveRequestDto {
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

export interface SubmitMyLeaveRequest {
  leaveType: string;
  startDate: string;
  endDate: string;
  reason: string | null;
}

export interface DisputeAttendanceRequest {
  summaryId: string;
  claimedPunchIn: string | null;
  claimedPunchOut: string | null;
  reason: string;
}

export interface UpdateMyProfileRequest {
  phoneNumber: string | null;
  street: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
}

export interface RequestEmailChangeRequest {
  newEmail: string;
}
