import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Translation, DashboardStats } from '../models/translation.model';

@Injectable({
  providedIn: 'root',
})
export class TranslationService {
  private readonly translationKeyUrl = 'https://localhost:7199/api/TranslationKey';
  private readonly translationValueUrl = 'https://localhost:7199/api/TranslationValue';
  private readonly storageKey = 'translationManager.translations';
  private translations: Translation[] = [];

  private translationsSubject = new BehaviorSubject<Translation[]>(
    this.loadCachedTranslations()
  );
  public translations$ = this.translationsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.translations = this.loadCachedTranslations();
  }

  loadTranslations(languageCode = 'EN'): Observable<Translation[]> {
    const params = { languageCode };

    return this.http.get<any>(this.translationKeyUrl).pipe(
      tap((response) => {
        console.log('TranslationValue API response', response);
        const items = Array.isArray(response)
          ? response
          : response?.items ?? response?.data ?? response?.results ?? [];

        const cached = this.loadCachedTranslations();
        const backendTranslations = (items || [])
          .map((item: any) => this.mapApiTranslationKey(item))
          .map((item: Translation) => this.mergeWithCachedTranslation(item, cached));

        const translations = this.combineWithCachedTranslations(backendTranslations, cached);
        this.setTranslations(translations);
      }),
      map(() => this.translations),
      catchError((error) => {
        console.error('Failed to load translation values', error);
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
    this.saveCachedTranslations(this.translations);
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
  id: item.id,
  translationKey: item.keyName,
  originalText: item.originalText,
  translation: '',
  tags: item.projectId?.toString() || '',
  client: '',
  project: item.projectId?.toString() || ''
};
  }

  private mergeWithCachedTranslation(item: Translation, cached: Translation[]): Translation {
    const existing = cached.find((cachedItem) => {
      return (
        (item.id != null && cachedItem.id != null && cachedItem.id === item.id) ||
        (item.translationKey && cachedItem.translationKey === item.translationKey)
      );
    });

    if (!existing) {
      return item;
    }

    return {
      ...existing,
      ...item,
      translationKey: item.translationKey || existing.translationKey,
      originalText: item.originalText || existing.originalText,
      translation: item.translation || existing.translation,
      tags: item.tags || existing.tags,
      client: item.client || existing.client,
      project: item.project || existing.project,
    };
  }

  private combineWithCachedTranslations(backend: Translation[], cached: Translation[]): Translation[] {
    const merged = backend.map((item) => this.mergeWithCachedTranslation(item, cached));

    cached.forEach((cachedItem) => {
      const exists = merged.some((item) => {
        return (
          (item.id != null && cachedItem.id != null && item.id === cachedItem.id) ||
          (item.translationKey && item.translationKey === cachedItem.translationKey)
        );
      });

      if (!exists) {
        merged.push(cachedItem);
      }
    });

    return merged;
  }

  private loadCachedTranslations(): Translation[] {
    if (typeof localStorage === 'undefined') {
      return [];
    }

    try {
      const stored = localStorage.getItem(this.storageKey);
      return stored ? (JSON.parse(stored) as Translation[]) : [];
    } catch {
      return [];
    }
  }

  private saveCachedTranslations(translations: Translation[]): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    try {
      localStorage.setItem(this.storageKey, JSON.stringify(translations));
    } catch {
      // ignore storage errors
    }
  }

  addTranslation(translation: Translation): Observable<Translation> {
    const body = {
      keyName: translation.translationKey,
      originalText: translation.originalText,
      projectId: translation.tags ? Number(translation.tags) : 1
    };

    console.log('POST TranslationKey body', body);

    return this.http.post<any>(this.translationKeyUrl, body).pipe(
      tap(() => {

  // ✅ Reload fresh data from backend
  this.loadTranslations().subscribe();

  console.log('Translation added successfully');

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
        projectId: translation.tags ? Number(translation.tags) : 1
      }).pipe(
        tap((updated) => {
          this.translations[index] = this.mapApiTranslationKey(updated);
          this.saveCachedTranslations(this.translations);
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
          this.saveCachedTranslations(this.translations);
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
