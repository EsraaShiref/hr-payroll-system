export interface PaginatedList<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface EmployeeDto {
  id: string;
  employeeCode: string;
  firstName: string;
  middleName: string | null;
  lastName: string;
  fullName: string;
  departmentName: string | null;
  positionTitle: string | null;
  employmentStatus: string;
  hireDate: string;
}

export interface AddressDto {
  street: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
}

export interface EmployeeDetailDto extends EmployeeDto {
  dateOfBirth: string;
  gender: string;
  nationalId: string;
  personalEmail: string | null;
  phoneNumber: string | null;
  street: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
  activeContract: ContractDto | null;
  terminationDate: string | null;
}

export interface ContractDto {
  id: string;
  employeeId: string;
  contractType: string;
  status: string;
  signedDate: string;
  expiryDate: string | null;
  currentVersion: ContractVersionDto;
  terminationDate: string | null;
}

export interface ContractVersionDto {
  id: string;
  versionNumber: number;
  baseSalaryAmount: number;
  baseSalaryCurrency: string;
  effectiveFrom: string;
  effectiveTo: string | null;
  allowanceAssignments: AllowanceAssignmentDto[];
}

export interface AllowanceAssignmentDto {
  allowanceId: string;
  allowanceName: string;
  allowanceType: string;
  overrideAmount: number | null;
  overridePercentage: number | null;
  computedValue: number;
}

export interface DepartmentDto {
  id: string;
  name: string;
  code: string;
  description: string | null;
  parentDepartmentId: string | null;
  parentDepartmentName: string | null;
  employeeCount: number;
}

export interface PositionDto {
  id: string;
  title: string;
  code: string;
  description: string | null;
  departmentId: string;
  departmentName: string;
}

export interface AllowanceDto {
  id: string;
  name: string;
  code: string;
  description: string | null;
  type: string;
  defaultAmount: number | null;
  defaultPercentage: number | null;
  taxability: string;
  isActive: boolean;
}

export interface CreateEmployeeRequest {
  employeeCode: string;
  firstName: string;
  middleName: string | null;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  nationalId: string;
  departmentId: string;
  positionId: string;
  hireDate: string;
}

export interface AddContractVersionRequest {
  newBaseSalaryAmount: number;
  newBaseSalaryCurrency: string;
  effectiveFrom: string;
  taxBracketSetId: string | null;
  socialInsuranceConfigId: string | null;
  allowanceAssignments: AllowanceAssignmentInput[] | null;
}

export interface AllowanceAssignmentInput {
  allowanceId: string;
  overrideAmount: number | null;
  overridePercentage: number | null;
}

// Payroll
export interface PayrollRunListItem {
  id: string;
  year: number;
  month: number;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  processedBy: string;
  totalEmployees: number;
  totalNetPay: number;
}

export interface PayrollRunStatus {
  id: string;
  year: number;
  month: number;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  totalEmployees: number;
  calculatedCount: number;
  skippedCount: number;
  failedCount: number;
}

export interface SkippedEmployeeInfo {
  employeeId: string;
  employeeName: string;
  skipReason: string;
}

export interface FailedEmployeeInfo {
  employeeId: string;
  employeeName: string;
  failureMessage: string;
}

export interface PayrollRunSummary {
  id: string;
  year: number;
  month: number;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  processedBy: string;
  totalGrossPay: number;
  totalDeductions: number;
  totalNetPay: number;
  totalEmployees: number;
  calculatedCount: number;
  skippedCount: number;
  failedCount: number;
  skippedEmployees: SkippedEmployeeInfo[];
  failedEmployees: FailedEmployeeInfo[];
}

export interface PayrollRunDetail {
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  status: string;
  skipReason: string | null;
  failureMessage: string | null;
  totalScheduledDays: number;
  totalPresentDays: number;
  totalAbsentDays: number;
  totalLeaveDays: number;
  totalOvertimeMinutes: number;
  lateOccurrenceCount: number;
  latePenaltyUnits: number;
  baseSalary: number;
  totalAllowances: number;
  overtimePay: number;
  grossPay: number;
  leaveDeduction: number;
  latePenaltyDeduction: number;
  socialInsuranceEmployeeShare: number;
  taxableIncome: number;
  taxAmount: number;
  totalDeductions: number;
  netPay: number;
}

export interface PaginatedPayrollRuns {
  items: PayrollRunListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}
