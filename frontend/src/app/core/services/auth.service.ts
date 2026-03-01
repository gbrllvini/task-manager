import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, catchError, map, of, tap } from "rxjs";
import { environment } from "../../../environments/environment";
import { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from "../models/auth.model";

const ACCESS_TOKEN_KEY = "tm_access_token";
const USER_KEY = "tm_user";

@Injectable({
  providedIn: "root"
})
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;
  private readonly currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);

  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(private readonly httpClient: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.httpClient
      .post<AuthResponse>(`${this.baseUrl}/login`, payload, { withCredentials: true })
      .pipe(tap((response) => this.setSession(response)));
  }

  cadastro(payload: RegisterRequest): Observable<AuthResponse> {
    return this.httpClient
      .post<AuthResponse>(`${this.baseUrl}/cadastro`, payload, { withCredentials: true })
      .pipe(tap((response) => this.setSession(response)));
  }

  refresh(): Observable<AuthResponse> {
    return this.httpClient
      .post<AuthResponse>(`${this.baseUrl}/refresh`, {}, { withCredentials: true })
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): Observable<void> {
    return this.httpClient.post<void>(`${this.baseUrl}/logout`, {}, { withCredentials: true }).pipe(
      catchError(() => of(void 0)),
      tap(() => this.clearSession())
    );
  }

  me(): Observable<CurrentUser> {
    return this.httpClient.get<CurrentUser>(`${this.baseUrl}/me`);
  }

  bootstrapSession(): Promise<void> {
    const token = localStorage.getItem(ACCESS_TOKEN_KEY);
    const userRaw = localStorage.getItem(USER_KEY);

    if (!token || !userRaw) {
      this.clearSession();
      return Promise.resolve();
    }

    try {
      const user = JSON.parse(userRaw) as CurrentUser;
      this.currentUserSubject.next(user);
    } catch {
      this.clearSession();
    }

    return Promise.resolve();
  }

  hasAccessToken(): boolean {
    return !!this.getAccessToken();
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  setSession(response: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
  }

  clearSession(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUserSubject.next(null);
  }

  tryRefreshSession(): Observable<boolean> {
    return this.refresh().pipe(
      map(() => true),
      catchError(() => {
        this.clearSession();
        return of(false);
      })
    );
  }
}
