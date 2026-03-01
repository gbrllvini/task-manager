import { Injectable } from "@angular/core";
import { CanActivate, Router, UrlTree } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError, map } from "rxjs/operators";
import { AuthService } from "../services/auth.service";

@Injectable({
  providedIn: "root"
})
export class AuthGuard implements CanActivate {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> {
    if (this.authService.hasAccessToken()) {
      return of(true);
    }

    return this.authService.refresh().pipe(
      map(() => true),
      catchError(() => of(this.router.createUrlTree(["/login"])))
    );
  }
}
