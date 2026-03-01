import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../environments/environment";
import {
  CreateTaskPayload,
  PagedResult,
  TaskItem,
  TaskListQuery,
  UpdateTaskPayload
} from "../models/task.model";

@Injectable({
  providedIn: "root"
})
export class TaskService {
  private readonly baseUrl = `${environment.apiUrl}/tasks`;

  constructor(private readonly httpClient: HttpClient) {}

  getTasks(query: TaskListQuery): Observable<PagedResult<TaskItem>> {
    let params = new HttpParams();

    if (query.status) {
      params = params.set("status", query.status);
    }

    if (query.priority) {
      params = params.set("priority", query.priority);
    }

    if (query.page) {
      params = params.set("page", query.page);
    }

    if (query.pageSize) {
      params = params.set("pageSize", query.pageSize);
    }

    if (query.sortBy) {
      params = params.set("sortBy", query.sortBy);
    }

    if (query.sortDirection) {
      params = params.set("sortDirection", query.sortDirection);
    }

    return this.httpClient.get<PagedResult<TaskItem>>(this.baseUrl, { params });
  }

  getTaskById(id: string): Observable<TaskItem> {
    return this.httpClient.get<TaskItem>(`${this.baseUrl}/${id}`);
  }

  createTask(payload: CreateTaskPayload): Observable<TaskItem> {
    return this.httpClient.post<TaskItem>(this.baseUrl, payload);
  }

  updateTask(id: string, payload: UpdateTaskPayload): Observable<void> {
    return this.httpClient.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  deleteTask(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
  }
}
