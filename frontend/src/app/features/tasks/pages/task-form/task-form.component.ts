import { Component, OnInit } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { finalize } from "rxjs";
import { CreateTaskPayload, TaskPriority, TaskStatus, UpdateTaskPayload } from "../../../../core/models/task.model";
import { TaskService } from "../../../../core/services/task.service";

@Component({
  selector: "app-task-form",
  templateUrl: "./task-form.component.html",
  styleUrls: ["./task-form.component.css"]
})
export class TaskFormComponent implements OnInit {
  protected readonly priorityValues = Object.values(TaskPriority).filter((value) => typeof value === "number") as number[];
  protected readonly statusValues = Object.values(TaskStatus).filter((value) => typeof value === "number") as number[];
  protected isEditMode = false;
  protected loading = false;
  protected errorMessage = "";
  private taskId: string | null = null;

  protected form = this.formBuilder.group({
    title: ["", [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
    description: ["", [Validators.maxLength(500)]],
    priority: [TaskPriority.Medium, Validators.required],
    status: [TaskStatus.Pending, Validators.required],
    dueDate: [""]
  });

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly taskService: TaskService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get("id");
    this.isEditMode = !!this.taskId;

    if (!this.isEditMode || !this.taskId) {
      return;
    }

    this.loading = true;

    this.taskService
      .getTaskById(this.taskId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (task) => {
          this.form.patchValue({
            title: task.title,
            description: task.description ?? "",
            priority: task.priority,
            status: task.status,
            dueDate: this.toDateTimeLocal(task.dueDate)
          });
        },
        error: () => {
          this.errorMessage = "Não foi possível carregar a tarefa.";
        }
      });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = "";

    const formValue = this.form.getRawValue();

    if (this.isEditMode && this.taskId) {
      const updatePayload: UpdateTaskPayload = {
        title: (formValue.title ?? "").trim(),
        description: (formValue.description ?? "").trim() || null,
        priority: formValue.priority ?? TaskPriority.Medium,
        status: formValue.status ?? TaskStatus.Pending,
        dueDate: this.mapDueDate(formValue.dueDate ?? "")
      };

      this.taskService
        .updateTask(this.taskId, updatePayload)
        .pipe(finalize(() => (this.loading = false)))
        .subscribe({
          next: () => this.navigateToList(),
          error: () => (this.errorMessage = "Não foi possível atualizar a tarefa.")
        });

      return;
    }

    const createPayload: CreateTaskPayload = {
      title: (formValue.title ?? "").trim(),
      description: (formValue.description ?? "").trim() || null,
      priority: formValue.priority ?? TaskPriority.Medium,
      dueDate: this.mapDueDate(formValue.dueDate ?? "")
    };

    this.taskService
      .createTask(createPayload)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.navigateToList(),
        error: () => (this.errorMessage = "Não foi possível criar a tarefa.")
      });
  }

  protected cancel(): void {
    this.navigateToList();
  }

  protected statusLabel(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending:
        return "Pendente";
      case TaskStatus.InProgress:
        return "Fazendo";
      case TaskStatus.Completed:
        return "Completa";
      case TaskStatus.Cancelled:
        return "Cancelada";
      default:
        return "Desconhecido";
    }
  }

  protected priorityLabel(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low:
        return "Baixa";
      case TaskPriority.Medium:
        return "Média";
      case TaskPriority.High:
        return "Alta";
      default:
        return "Desconhecida";
    }
  }

  private navigateToList(): void {
    void this.router.navigate(["/tarefas"]);
  }

  private toDateTimeLocal(value?: string | null): string {
    if (!value) {
      return "";
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return "";
    }

    const timezoneOffset = date.getTimezoneOffset() * 60000;
    return new Date(date.getTime() - timezoneOffset).toISOString().slice(0, 16);
  }

  private mapDueDate(inputValue: string): string | null {
    if (!inputValue) {
      return null;
    }

    return `${inputValue}:00`;
  }
}
