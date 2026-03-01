import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { finalize } from "rxjs";
import { AuthService } from "./core/services/auth.service";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"]
})
export class AppComponent {
  protected menuOpen = false;
  protected logoutLoading = false;
  protected readonly currentUser$ = this.authService.currentUser$;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  protected toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  protected openTasks(): void {
    this.menuOpen = false;
    void this.router.navigate(["/tarefas"]);
  }

  protected openLogin(): void {
    this.menuOpen = false;
    void this.router.navigate(["/login"]);
  }

  protected logout(): void {
    if (this.logoutLoading) {
      return;
    }

    this.logoutLoading = true;
    this.authService
      .logout()
      .pipe(finalize(() => (this.logoutLoading = false)))
      .subscribe(() => {
        this.menuOpen = false;
        void this.router.navigate(["/login"]);
      });
  }
}
