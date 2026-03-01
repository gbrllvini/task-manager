export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3
}

export enum TaskStatus {
  Pending = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4
}

export interface TaskItem {
  id: string;
  title: string;
  description?: string | null;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate?: string | null;
  createdAt: string;
}

export interface TaskListQuery {
  status?: TaskStatus;
  priority?: TaskPriority;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface CreateTaskPayload {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  dueDate?: string | null;
}

export interface UpdateTaskPayload {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate?: string | null;
}
