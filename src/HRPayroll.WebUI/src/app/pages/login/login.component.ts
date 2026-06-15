import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { LoginRequest } from '../../models/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h1>HR Payroll</h1>
        <p class="subtitle">Sign in to your account</p>

          <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              name="email"
              [(ngModel)]="email"
              #emailCtrl="ngModel"
              required
              email
              placeholder="you@company.com"
            />
            @if (emailCtrl.invalid && emailCtrl.touched) {
              <span class="error">Valid email is required</span>
            }
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              id="password"
              type="password"
              name="password"
              [(ngModel)]="password"
              #passwordCtrl="ngModel"
              required
              minlength="8"
              placeholder="Enter your password"
            />
            @if (passwordCtrl.invalid && passwordCtrl.touched) {
              <span class="error">Password is required (min 8 characters)</span>
            }
          </div>

          @if (errorMessage()) {
            <div class="error global-error">{{ errorMessage() }}</div>
          }

          <button type="submit" [disabled]="loginForm.invalid || loading()">
            {{ loading() ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: #f0f2f5;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    }
    .login-card {
      background: white;
      padding: 2.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      width: 100%;
      max-width: 400px;
    }
    h1 {
      margin: 0 0 0.25rem;
      font-size: 1.5rem;
      color: #1a1a1a;
    }
    .subtitle {
      margin: 0 0 1.5rem;
      color: #666;
      font-size: 0.875rem;
    }
    .form-group {
      margin-bottom: 1rem;
    }
    label {
      display: block;
      margin-bottom: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      color: #333;
    }
    input {
      width: 100%;
      padding: 0.625rem 0.75rem;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 0.875rem;
      box-sizing: border-box;
      transition: border-color 0.15s;
    }
    input:focus {
      outline: none;
      border-color: #2563eb;
      box-shadow: 0 0 0 2px rgba(37,99,235,0.1);
    }
    .error {
      display: block;
      color: #dc2626;
      font-size: 0.75rem;
      margin-top: 0.25rem;
    }
    .global-error {
      background: #fef2f2;
      border: 1px solid #fecaca;
      border-radius: 6px;
      padding: 0.5rem 0.75rem;
      margin-bottom: 1rem;
    }
    button {
      width: 100%;
      padding: 0.625rem;
      background: #2563eb;
      color: white;
      border: none;
      border-radius: 6px;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: background 0.15s;
    }
    button:hover:not(:disabled) {
      background: #1d4ed8;
    }
    button:disabled {
      background: #93c5fd;
      cursor: not-allowed;
    }
  `],
})
export class LoginComponent {
  email = '';
  password = '';
  loading = signal(false);
  errorMessage = signal('');

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(): void {
    if (!this.email || !this.password) return;

    this.loading.set(true);
    this.errorMessage.set('');

    const request: LoginRequest = {
      email: this.email,
      password: this.password,
    };

    this.authService.login(request).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/']);
      },
      error: err => {
        this.loading.set(false);
        if (err.status === 401) {
          this.errorMessage.set('Invalid email or password.');
        } else if (err.status === 403) {
          this.errorMessage.set('Account is locked. Try again later.');
        } else {
          this.errorMessage.set('An unexpected error occurred. Please try again.');
        }
      },
    });
  }
}
