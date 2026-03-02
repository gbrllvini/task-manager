import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { finalize } from "rxjs";
import { TaskService } from "../../../../core/services/task.service";
import { PagedResult, TaskItem, TaskListQuery, TaskPriority, TaskStatus } from "../../../../core/models/task.model";

@Component({
  selector: "app-task-list",
  templateUrl: "./task-list.component.html",
  styleUrls: ["./task-list.component.css"]
})
export class TaskListComponent implements OnInit {
  protected readonly statusValues = Object.values(TaskStatus).filter((value) => typeof value === "number") as number[];
  protected readonly priorityValues = Object.values(TaskPriority).filter((value) => typeof value === "number") as number[];
  protected result: PagedResult<TaskItem> = {
    items: [],
    page: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 0
  };
  protected query: TaskListQuery = {
    page: 1,
    pageSize: 10,
    sortBy: "createdAt",
    sortDirection: "desc"
  };
  protected loading = false;
  protected errorMessage = "";

  constructor(
    private readonly taskService: TaskService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  protected loadTasks(): void {
    this.loading = true;
    this.errorMessage = "";

    this.taskService
      .getTasks(this.query)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          this.result = result;
        },
        error: () => {
          this.errorMessage = "Não foi possível carregar as tarefas.";
        }
      });
  }

  protected applyFilters(): void {
    this.query.page = 1;
    this.loadTasks();
  }

  protected changePage(nextPage: number): void {
    if (nextPage < 1 || nextPage > this.result.totalPages) {
      return;
    }

    this.query.page = nextPage;
    this.loadTasks();
  }

  protected createTask(): void {
    void this.router.navigate(["/tarefas/nova"]);
  }

  protected editTask(id: string): void {
    void this.router.navigate(["/tarefas", id, "editar"]);
  }

  protected deleteTask(id: string): void {
    if (!window.confirm("Deseja mesmo deletar essa tarefa?")) {
      return;
    }

    this.taskService.deleteTask(id).subscribe({
      next: () => {
        this.loadTasks();
      },
      error: () => {
        this.errorMessage = "Não foi possível deletar essa tarefa.";
      }
    });
  }

  protected statusLabel(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending:
        return "Pendente";
      case TaskStatus.InProgress:
        return "Fazendo";
      case TaskStatus.Completed:
        return "Concluida";
      case TaskStatus.Cancelled:
        return "Cancelada";
      default:
        return "Desconhecido";
    }
  }

  protected statusBadgeClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending:
        return "status-pending";
      case TaskStatus.InProgress:
        return "status-in-progress";
      case TaskStatus.Completed:
        return "status-completed";
      case TaskStatus.Cancelled:
        return "status-cancelled";
      default:
        return "status-pending";
    }
  }

  protected formatDueDate(dueDate?: string | null): string {
    if (!dueDate) {
      return "-";
    }

    const parsedDate = new Date(dueDate);
    if (Number.isNaN(parsedDate.getTime())) {
      return "-";
    }

    const formattedDate = parsedDate.toLocaleDateString("pt-BR", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric"
    });

    const formattedTime = parsedDate.toLocaleTimeString("pt-BR", {
      hour: "2-digit",
      minute: "2-digit"
    });

    return `${formattedDate} ${formattedTime}`;
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
}
