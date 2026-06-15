import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then(c => c.LoginComponent),
  },
  {
    path: 'employees',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/employees/employee-list/employee-list.component').then(c => c.EmployeeListComponent),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./pages/employees/employee-form/employee-form.component').then(c => c.EmployeeFormComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./pages/employees/employee-detail/employee-detail.component').then(c => c.EmployeeDetailComponent),
      },
      {
        path: ':id/attendance',
        loadComponent: () =>
          import('./pages/attendance/employee-attendance/employee-attendance.component').then(c => c.EmployeeAttendanceComponent),
      },
      {
        path: ':employeeId/leave/new',
        loadComponent: () =>
          import('./pages/leave-requests/leave-form/leave-form.component').then(c => c.LeaveFormComponent),
      },
    ],
  },
  {
    path: 'contracts',
    canActivate: [authGuard],
    children: [
      {
        path: ':id',
        loadComponent: () =>
          import('./pages/contracts/contract-detail/contract-detail.component').then(c => c.ContractDetailComponent),
      },
      {
        path: ':id/versions/new',
        loadComponent: () =>
          import('./pages/contracts/add-version-form/add-version-form.component').then(c => c.AddVersionFormComponent),
      },
    ],
  },
  {
    path: 'departments',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/departments/department-list/department-list.component').then(c => c.DepartmentListComponent),
  },
  {
    path: 'leave-requests',
    canActivate: [authGuard],
    children: [
      {
        path: 'pending',
        loadComponent: () =>
          import('./pages/leave-requests/pending-list/pending-list.component').then(c => c.PendingLeaveListComponent),
      },
    ],
  },
  { path: '', redirectTo: '/employees', pathMatch: 'full' },
  { path: '**', redirectTo: '/employees' },
];
