import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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
  private state = signal<AuthState>(this.loadState());

  readonly accessToken = computed(() => this.state().accessToken);
  readonly isAuthenticated = computed(() => this.state().isAuthenticated);
  readonly roles = computed(() => this.state().roles);
  readonly employeeId = computed(() => this.state().employeeId);
  readonly userId = computed(() => this.state().userId);
  readonly email = computed(() => this.state().email);

  constructor(private http: HttpClient) {}

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
      error: () => this.clearState(),
      complete: () => this.clearState(),
    });
  }

  private handleAuthResponse(response: LoginResponse): void {
    const decoded = this.decodeToken(response.accessToken);
    const newState: AuthState = {
      accessToken: response.accessToken,
      userId: decoded.sub,
      employeeId: decoded.employeeId ?? null,
      email: decoded.email,
      roles: this.normalizeArray(decoded.role),
      permissions: this.normalizeArray(decoded.permission),
      isAuthenticated: true,
    };
    this.state.set(newState);
    this.persistState(newState);
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

  private persistState(state: AuthState): void {
    try {
      localStorage.setItem('auth_state', JSON.stringify({
        accessToken: state.accessToken,
        userId: state.userId,
        employeeId: state.employeeId,
        email: state.email,
        roles: state.roles,
        permissions: state.permissions,
      }));
    } catch {
      // localStorage unavailable
    }
  }

  private loadState(): AuthState {
    try {
      const raw = localStorage.getItem('auth_state');
      if (!raw) return initialAuthState;

      const parsed = JSON.parse(raw);
      if (!parsed.accessToken) return initialAuthState;

      const decoded = this.decodeToken(parsed.accessToken);
      const now = Math.floor(Date.now() / 1000);
      if (decoded.exp < now) {
        localStorage.removeItem('auth_state');
        return initialAuthState;
      }

      return {
        accessToken: parsed.accessToken,
        userId: parsed.userId ?? null,
        employeeId: parsed.employeeId ?? null,
        email: parsed.email ?? null,
        roles: parsed.roles ?? [],
        permissions: parsed.permissions ?? [],
        isAuthenticated: true,
      };
    } catch {
      return initialAuthState;
    }
  }

  private clearState(): void {
    this.state.set(initialAuthState);
    try {
      localStorage.removeItem('auth_state');
    } catch {
      // localStorage unavailable
    }
  }

  refreshAccessToken(): Promise<string | null> {
    return new Promise(resolve => {
      this.refreshToken().subscribe({
        next: response => {
          this.handleAuthResponse(response);
          resolve(response.accessToken);
        },
        error: () => {
          this.clearState();
          resolve(null);
        },
      });
    });
  }
}
