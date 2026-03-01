import { Injectable } from "@angular/core";
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from "@angular/common/http";
import { Router } from "@angular/router";
import { Observable, Subject, catchError, switchMap, take, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshSubject = new Subject<string | null>();

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    let authRequest = request;
    const accessToken = this.authService.getAccessToken();

    if (accessToken && !this.isAuthLoginOrRefreshRequest(request.url) && !request.headers.has("Authorization")) {
      authRequest = this.addAuthHeader(request, accessToken);
    }

    return next.handle(authRequest).pipe(
      catchError((error: unknown) => {
        if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
          return throwError(() => error);
        }

        if (this.isAuthLoginOrRefreshRequest(authRequest.url) || authRequest.headers.has("x-refresh-retry")) {
          this.authService.clearSession();
          void this.router.navigate(["/login"]);
          return throwError(() => error);
        }

        return this.handleUnauthorizedError(authRequest, next, error);
      })
    );
  }

  private handleUnauthorizedError(
    request: HttpRequest<unknown>,
    next: HttpHandler,
    originalError: HttpErrorResponse
  ): Observable<HttpEvent<unknown>> {
    if (this.isRefreshing) {
      return this.refreshSubject.pipe(
        take(1),
        switchMap((token) => {
          if (!token) {
            return throwError(() => originalError);
          }

          return next.handle(this.addAuthHeader(request, token, true));
        })
      );
    }

    this.isRefreshing = true;
    this.refreshSubject = new Subject<string | null>();

    return this.authService.refresh().pipe(
      switchMap((response) => {
        this.isRefreshing = false;
        this.refreshSubject.next(response.accessToken);
        this.refreshSubject.complete();

        return next.handle(this.addAuthHeader(request, response.accessToken, true));
      }),
      catchError((refreshError: unknown) => {
        this.isRefreshing = false;
        this.refreshSubject.next(null);
        this.refreshSubject.complete();
        this.authService.clearSession();
        void this.router.navigate(["/login"]);

        return throwError(() => refreshError);
      })
    );
  }

  private addAuthHeader(request: HttpRequest<unknown>, token: string, markRetry = false): HttpRequest<unknown> {
    if (markRetry) {
      return request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
          "x-refresh-retry": "1"
        }
      });
    }

    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private isAuthLoginOrRefreshRequest(url: string): boolean {
    return url.includes("/auth/login") || url.includes("/auth/cadastro") || url.includes("/auth/refresh");
  }
}
