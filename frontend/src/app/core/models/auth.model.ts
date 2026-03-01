export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  displayName: string;
  email: string;
  password: string;
}

export interface CurrentUser {
  id: string;
  displayName: string;
  email: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: CurrentUser;
}
