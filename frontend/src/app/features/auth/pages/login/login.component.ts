import { Component } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { finalize } from "rxjs";
import { AuthService } from "../../../../core/services/auth.service";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"]
})
export class LoginComponent {
  protected loading = false;
  protected errorMessage = "";

  protected form = this.formBuilder.group({
    email: ["", [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ["", [Validators.required, Validators.minLength(6), Validators.maxLength(128)]]
  });

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const rawValue = this.form.getRawValue();

    this.loading = true;
    this.errorMessage = "";

    this.authService
      .login({
        email: (rawValue.email ?? "").trim(),
        password: rawValue.password ?? ""
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          void this.router.navigate(["/tarefas"]);
        },
        error: () => {
          this.errorMessage = "Não foi possível entrar. Verifique email e senha.";
        }
      });
  }
}
