import { Component } from "@angular/core";
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { finalize } from "rxjs";
import { AuthService } from "../../../../core/services/auth.service";

@Component({
  selector: "app-cadastro",
  templateUrl: "./cadastro.component.html",
  styleUrls: ["./cadastro.component.css"]
})
export class CadastroComponent {
  protected loading = false;
  protected errorMessage = "";

  protected form = this.formBuilder.group(
    {
      displayName: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(120)]],
      email: ["", [Validators.required, Validators.email, Validators.maxLength(256)]],
      password: ["", [Validators.required, Validators.minLength(6), Validators.maxLength(128)]],
      confirmPassword: ["", [Validators.required]]
    },
    {
      validators: [this.passwordsMatchValidator]
    }
  );

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
      .cadastro({
        displayName: (rawValue.displayName ?? "").trim(),
        email: (rawValue.email ?? "").trim(),
        password: rawValue.password ?? ""
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => {
          void this.router.navigate(["/tarefas"]);
        },
        error: () => {
          this.errorMessage = "Não foi possível concluir o cadastro.";
        }
      });
  }

  private passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get("password")?.value;
    const confirmPassword = control.get("confirmPassword")?.value;

    return password === confirmPassword ? null : { passwordsMismatch: true };
  }
}
