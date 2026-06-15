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
        path: ':id',
        loadComponent: () =>
          import('./pages/employees/employee-detail/employee-detail.component').then(c => c.EmployeeDetailComponent),
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
    ],
  },
  {
    path: 'departments',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/departments/department-list/department-list.component').then(c => c.DepartmentListComponent),
  },
  { path: '', redirectTo: '/employees', pathMatch: 'full' },
  { path: '**', redirectTo: '/employees' },
];
