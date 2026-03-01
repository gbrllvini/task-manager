import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { TasksRoutingModule } from "./tasks-routing.module";
import { TaskFormComponent } from "./pages/task-form/task-form.component";
import { TaskListComponent } from "./pages/task-list/task-list.component";

@NgModule({
  declarations: [TaskListComponent, TaskFormComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, TasksRoutingModule]
})
export class TasksModule {}
