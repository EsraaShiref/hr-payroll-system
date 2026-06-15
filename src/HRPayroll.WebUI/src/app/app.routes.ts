import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then(c => c.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      // Module B feature routes added in sub-step
    ],
  },
  { path: '**', redirectTo: '' },
];
