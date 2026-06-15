export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

export interface AuthState {
  accessToken: string | null;
  userId: string | null;
  employeeId: string | null;
  email: string | null;
  roles: string[];
  permissions: string[];
  isAuthenticated: boolean;
}

export const initialAuthState: AuthState = {
  accessToken: null,
  userId: null,
  employeeId: null,
  email: null,
  roles: [],
  permissions: [],
  isAuthenticated: false,
};
