import { APP_INITIALIZER, NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { ReactiveFormsModule } from "@angular/forms";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { AuthInterceptor } from "./core/interceptors/auth.interceptor";
import { AuthService } from "./core/services/auth.service";
import { CadastroComponent } from "./features/auth/pages/cadastro/cadastro.component";
import { LoginComponent } from "./features/auth/pages/login/login.component";

function bootstrapSessionFactory(authService: AuthService): () => Promise<void> {
  return () => authService.bootstrapSession();
}

@NgModule({
  declarations: [AppComponent, LoginComponent, CadastroComponent],
  imports: [BrowserModule, HttpClientModule, ReactiveFormsModule, AppRoutingModule],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: bootstrapSessionFactory,
      deps: [AuthService],
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
