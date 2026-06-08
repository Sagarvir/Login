import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { ProjectOption } from '../models/project.model';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private readonly projectUrl = 'https://localhost:7199/api/Project';
  private readonly http = inject(HttpClient);

  getProjects(): Observable<ProjectOption[]> {
    return this.http.get<any>(this.projectUrl).pipe(
      map((response) => this.mapProjects(response)),
      catchError((error) => {
        console.error('Failed to load projects', error);
        return of([] as ProjectOption[]);
      })
    );
  }

  updateProject(projectId: number, newName: string): Observable<any> {
    const url = `${this.projectUrl}?projectId=${projectId}&newName=${encodeURIComponent(
      newName
    )}`;

    return this.http.put<any>(url, {}).pipe(
      catchError((error) => {
        console.error('Failed to update project', error);
        return throwError(() => error);
      })
    );
  }

  private mapProjects(response: any): ProjectOption[] {
    const items = this.extractArray(response);

    return items
      .map((item) => {
        const id = item.id ?? item.Id ?? item.projectId ?? item.ProjectId;
        const name =
          item.name ??
          item.Name ??
          item.projectName ??
          item.ProjectName ??
          item.title ??
          item.Title;

        if (id == null || !name) {
          return null;
        }

        return {
          id: Number(id),
          name: String(name),
        };
      })
      .filter((project): project is ProjectOption => project !== null);
  }

  private extractArray(response: any): any[] {
    if (Array.isArray(response)) {
      return response;
    }

    if (!response || typeof response !== 'object') {
      return [];
    }

    if (Array.isArray(response.data)) {
      return response.data;
    }

    if (Array.isArray(response.items)) {
      return response.items;
    }

    if (Array.isArray(response.value)) {
      return response.value;
    }

    if (Array.isArray(response.projects)) {
      return response.projects;
    }

    return [];
  }
}