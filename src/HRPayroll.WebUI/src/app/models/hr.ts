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
  code: string | null;
  description: string | null;
  managerName: string | null;
  isActive: boolean;
}
