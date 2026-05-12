import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Translation, DashboardStats } from '../models/translation.model';

@Injectable({
  providedIn: 'root',
})
export class TranslationService {
  private readonly translationKeyUrl = 'https://localhost:7199/api/TranslationKey';
  private translations: Translation[] = [];

  private translationsSubject = new BehaviorSubject<Translation[]>(
    this.translations
  );
  public translations$ = this.translationsSubject.asObservable();

  constructor(private http: HttpClient) {}

  loadTranslations(): Observable<Translation[]> {
    return this.http.get<any>(this.translationKeyUrl).pipe(
      tap((response) => {
        const items = Array.isArray(response)
          ? response
          : response?.items ?? response?.data ?? response?.results ?? [];

        const translations = (items || []).map((item: any) => this.mapApiTranslationKey(item));
        this.setTranslations(translations);
      }),
      map(() => this.translations),
      catchError((error) => {
        console.error('Failed to load translation keys', error);
        this.setTranslations([]);
        return of([] as Translation[]);
      })
    );
  }

  getTranslations(): Observable<Translation[]> {
    return this.translations$;
  }

  setTranslations(translations: Translation[]): void {
    this.translations = translations || [];
    this.translationsSubject.next([...this.translations]);
  }

  private mapApiTranslationKey(item: any): Translation {
    const nestedValues = Array.isArray(item.translationValues)
      ? item.translationValues
      : Array.isArray(item.values)
      ? item.values
      : Array.isArray(item.Translations)
      ? item.Translations
      : [];

    const nestedTranslation = Array.isArray(nestedValues) && nestedValues.length
      ? nestedValues[0].value ?? nestedValues[0].translation ?? nestedValues[0].text ?? ''
      : '';

    return {
      id: item.id ?? item.keyId ?? item.translationKeyId ?? item.KeyId ?? item.KeyID,
      translationKey: item.keyName ?? item.KeyName ?? item.key ?? item.name ?? item.Key ?? '',
      originalText: item.originalText ?? item.OriginalText ?? item.value ?? item.text ?? item.Name ?? '',
      translation: item.translation ?? item.Translation ?? item.translationValue ?? item.TranslationValue ?? item.value ?? nestedTranslation ?? '',
      tags: item.tags ?? item.Tags ?? item.tag ?? item.Tag ?? '',
      client: item.client ?? item.Client ?? item.projectClient ?? '',
      project: item.project ?? item.Project ?? item.projectId?.toString() ?? item.ProjectId?.toString() ?? ''
    };
  }

  addTranslation(translation: Translation): Observable<Translation> {
    const body = {
      KeyName: translation.translationKey,
      originalText: translation.originalText,
      projectId: 1
    };

    console.log('POST TranslationKey body', body);

    return this.http.post<any>(this.translationKeyUrl, body).pipe(
      tap((created) => {
        const item = this.mapApiTranslationKey(created);
        this.translations.push(item);
        this.translationsSubject.next([...this.translations]);
        console.log('POST TranslationKey response', created);
        this.loadTranslations().subscribe({
          error: (err) => console.error('Failed to refresh translations after add', err)
        });
      }),
      catchError((error) => {
        console.error('Failed to add translation key', error);
        return throwError(() => error);
      })
    );
  }

  updateTranslation(index: number, translation: Translation): Observable<Translation> {
    if (translation.id != null) {
      return this.http.put<Translation>(`${this.translationKeyUrl}/${translation.id}`, {
        KeyName: translation.translationKey,
        originalText: translation.originalText,
        projectId: 1
      }).pipe(
        tap((updated) => {
          this.translations[index] = this.mapApiTranslationKey(updated);
          this.translationsSubject.next([...this.translations]);
        }),
        catchError((error) => {
          console.error('Failed to update translation key', error);
          return throwError(() => error);
        })
      );
    }
    this.translations[index] = translation;
    this.translationsSubject.next([...this.translations]);
    return of(translation);
  }

  deleteTranslation(index: number): Observable<void> {
    const translation = this.translations[index];
    if (translation?.id != null) {
      return this.http.delete<void>(`${this.translationKeyUrl}/${translation.id}`).pipe(
        tap(() => {
          this.translations.splice(index, 1);
          this.translationsSubject.next([...this.translations]);
        }),
        catchError((error) => {
          console.error('Failed to delete translation key', error);
          return throwError(() => error);
        })
      );
    }
    this.translations.splice(index, 1);
    this.translationsSubject.next([...this.translations]);
    return of(undefined);
  }

  saveTranslations(): Observable<any> {
    return this.loadTranslations();
  }

  getStats(): DashboardStats {
    const totalKeys = this.translations.filter((t) => t.translationKey?.trim()).length;
    const translated = this.translations.filter(
      (t) => t.translationKey?.trim() && t.translation?.trim()
    ).length;
    const completion = totalKeys > 0 ? Math.round((translated / totalKeys) * 100) : 0;

    return {
      totalKeys,
      translated,
      completion,
    };
  }
}
