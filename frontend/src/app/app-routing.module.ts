import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

const routes: Routes = [
  {
    path: "",
    redirectTo: "tarefas",
    pathMatch: "full"
  },
  {
    path: "tarefas",
    loadChildren: () =>
      import("./features/tasks/tasks.module").then((module) => module.TasksModule)
  },
  {
    path: "**",
    redirectTo: "tarefas"
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
