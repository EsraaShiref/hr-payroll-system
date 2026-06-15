import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, AuthState, initialAuthState } from '../../models/auth';

interface JwtPayload {
  sub: string;
  email: string;
  employeeId?: string;
  role?: string | string[];
  permission?: string | string[];
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = environment.apiBaseUrl;
  private state = signal<AuthState>(initialAuthState);

  readonly accessToken = computed(() => this.state().accessToken);
  readonly isAuthenticated = computed(() => this.state().isAuthenticated);
  readonly roles = computed(() => this.state().roles);
  readonly employeeId = computed(() => this.state().employeeId);
  readonly userId = computed(() => this.state().userId);
  readonly email = computed(() => this.state().email);

  constructor(private http: HttpClient) {}

  /** Called via APP_INITIALIZER on bootstrap — tries to re-authenticate via HttpOnly cookie */
  async initialize(): Promise<void> {
    try {
      await firstValueFrom(this.refreshToken());
    } catch {
      this.state.set(initialAuthState);
    }
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, request).pipe(
      tap(response => this.handleAuthResponse(response)),
    );
  }

  refreshToken(): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/refresh`, {}).pipe(
      tap(response => this.handleAuthResponse(response)),
    );
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/auth/revoke`, {}).subscribe({
      error: () => this.state.set(initialAuthState),
      complete: () => this.state.set(initialAuthState),
    });
  }

  refreshAccessToken(): Promise<string | null> {
    return new Promise(resolve => {
      this.refreshToken().subscribe({
        next: response => {
          this.handleAuthResponse(response);
          resolve(response.accessToken);
        },
        error: () => {
          this.state.set(initialAuthState);
          resolve(null);
        },
      });
    });
  }

  private handleAuthResponse(response: LoginResponse): void {
    const decoded = this.decodeToken(response.accessToken);
    this.state.set({
      accessToken: response.accessToken,
      userId: decoded.sub,
      employeeId: decoded.employeeId ?? null,
      email: decoded.email,
      roles: this.normalizeArray(decoded.role),
      permissions: this.normalizeArray(decoded.permission),
      isAuthenticated: true,
    });
  }

  private decodeToken(token: string): JwtPayload {
    const payload = token.split('.')[1];
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const decoded = JSON.parse(atob(normalized));
    return decoded as JwtPayload;
  }

  private normalizeArray(value: string | string[] | undefined): string[] {
    if (!value) return [];
    return Array.isArray(value) ? value : [value];
  }
}
