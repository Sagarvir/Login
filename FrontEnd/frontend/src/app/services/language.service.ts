import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { Language } from '../models/translation.model';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private readonly languageUrl = 'https://localhost:7199/api/language';

  constructor(private http: HttpClient) {}

  getLanguages(): Observable<Language[]> {
    return this.http.get<Language[]>(this.languageUrl).pipe(
      catchError((error) => {
        console.error('Failed to load languages', error);
        return of([] as Language[]);
      })
    );
  }
}
