import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AuthGuard } from "./core/guards/auth.guard";
import { CadastroComponent } from "./features/auth/pages/cadastro/cadastro.component";
import { LoginComponent } from "./features/auth/pages/login/login.component";

const routes: Routes = [
  {
    path: "",
    redirectTo: "tarefas",
    pathMatch: "full"
  },
  {
    path: "login",
    component: LoginComponent
  },
  {
    path: "cadastro",
    component: CadastroComponent
  },
  {
    path: "tarefas",
    canActivate: [AuthGuard],
    loadChildren: () =>
      import("./features/tasks/tasks.module").then((module) => module.TasksModule)
  },
  {
    path: "**",
    redirectTo: "login"
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
