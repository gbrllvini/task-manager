import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { TaskFormComponent } from "./pages/task-form/task-form.component";
import { TaskListComponent } from "./pages/task-list/task-list.component";

const routes: Routes = [
  {
    path: "",
    component: TaskListComponent
  },
  {
    path: "nova",
    component: TaskFormComponent
  },
  {
    path: ":id/editar",
    component: TaskFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TasksRoutingModule {}
