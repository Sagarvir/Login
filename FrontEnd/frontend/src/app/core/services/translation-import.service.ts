import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TranslationImportService {
  private apiUrl = 'https://localhost:7199/api/TranslationImport';

  constructor(private http: HttpClient) {}

  importKeys(file: File, projectId: number): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('projectId', projectId.toString());

    return this.http.post<any>(`${this.apiUrl}/keys`, formData);
  }
}