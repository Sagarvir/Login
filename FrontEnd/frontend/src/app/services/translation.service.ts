import {
  Injectable,
} from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  Subject,
  catchError,
  finalize,
  from,
  map,
  of,
  tap,
  throwError,
} from 'rxjs';
import { HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { concatMap, toArray } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import {
  Translation,
  DashboardStats,
  AddTranslationRequest,
} from '../models/translation.model';

@Injectable({
  providedIn: 'root',
})
export class TranslationService {
  private readonly translationKeyUrl = 'https://localhost:7199/api/TranslationKey';
  private readonly translationValueUrl = 'https://localhost:7199/api/TranslationValue';
  private readonly translationWithValuesUrl = 'https://localhost:7199/api/TranslationValue/with-translations';
  private readonly storageKey = 'translationManager.translations';

  private translations: Translation[] = [];
  private saveInProgress = false;

  // --- Subjects ---

  private translationsSubject = new BehaviorSubject<Translation[]>(
    this.loadCachedTranslations()
  );
  public translations$ = this.translationsSubject.asObservable();

  private selectedLanguageSubject = new BehaviorSubject<string>('EN');
  public selectedLanguage$ = this.selectedLanguageSubject.asObservable();

  // Subject (not BehaviorSubject) — does NOT emit on subscribe
  private saveRequestedSubject = new Subject<void>();
  public saveRequested$ = this.saveRequestedSubject.asObservable();

  // Subject (not BehaviorSubject) — no replay, no race conditions
  private saveCompletedSubject = new Subject<void>();
  public saveCompleted$ = this.saveCompletedSubject.asObservable();

  constructor(private http: HttpClient) {
    this.translations = this.loadCachedTranslations();
  }

  // --- Save coordination ---

  requestSave(): void {
    if (this.saveInProgress) {
      return;
    }

    this.saveInProgress = true;
    this.saveRequestedSubject.next();
  }

  notifySaveCompleted(): void {
    this.saveInProgress = false;
    this.saveCompletedSubject.next();
  }

  // --- Language ---

  setSelectedLanguage(languageCode: string): void {
    this.selectedLanguageSubject.next(languageCode);
  }

  getSelectedLanguage(): string {
    return this.selectedLanguageSubject.value;
  }

  // --- Translations CRUD ---

  loadTranslations(languageCode = 'EN'): Observable<Translation[]> {
    const upper = languageCode.toUpperCase();
    const url = `${this.translationWithValuesUrl}?languageCode=${upper}`;

    return this.http.get<any>(url).pipe(
      map((response) => {
        const items = this.extractArray(response);
        const backendTranslations = items.map((item: any) =>
          this.mapApiTranslationKey(item)
        );
        this.setTranslations(backendTranslations);
        return backendTranslations;
      }),
      catchError((error) => {
        console.error('Failed to load translations', error);
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

  getAllTranslations(languageCode: string): Observable<{ [key: string]: string }> {
    const upper = languageCode.toUpperCase();
    // Use the "with-translations" endpoint which returns translation keys
    // with their values; then reuse the existing mapper to build the dictionary.
    const url = `${this.translationWithValuesUrl}?languageCode=${upper}`;

    return this.http.get<any>(url).pipe(
      map((response) => {
        const items = this.extractArray(response);
        const dictionary: { [key: string]: string } = {};
        items.forEach((item) => {
          const t = this.mapApiTranslationKey(item);
          const key = t.translationKey?.trim();
          const value = t.translation ?? '';
          if (key) {
            dictionary[key] = value;
          }
        });
        return dictionary;
      }),
      catchError((error) => {
        console.error('Failed to load translations for language:', languageCode, error);
        return of({} as { [key: string]: string });
      })
    );
  }

  addTranslation(translation: Translation): Observable<Translation> {
    try {
      const body = this.buildKeyPayload(translation);

      return this.http.post<any>(this.translationKeyUrl, body).pipe(
        tap(() => {
          this.loadTranslations().subscribe();
        }),
        catchError((error) => {
          console.error('Failed to add translation', error);
          return throwError(() => error);
        })
      );
    } catch (error) {
      return throwError(() => error);
    }
  }

  updateTranslation(index: number, translation: Translation): Observable<Translation> {
    if (translation.id != null) {
      try {
        return this.http
          .put<Translation>(`${this.translationKeyUrl}/${translation.id}`, {
            KeyName: translation.translationKey,
            originalText: translation.originalText,
            projectId: this.resolveProjectId(translation),
          })
          .pipe(
            tap((updated) => {
              this.translations[index] = this.mapApiTranslationKey(updated);
              this.saveCachedTranslations(this.translations);
              this.translationsSubject.next([...this.translations]);
            }),
            catchError((error) => {
              console.error('Failed to update translation', error);
              return throwError(() => error);
            })
          );
      } catch (error) {
        return throwError(() => error);
      }
    }

    this.translations[index] = translation;
    this.translationsSubject.next([...this.translations]);
    return of(translation);
  }

  deleteTranslation(index: number): Observable<void> {
    const translation = this.translations[index];

      if (!translation?.id) {
        return throwError(() => new Error('Invalid translation id'));
    }
  
      return this.http
  .delete(`${this.translationKeyUrl}/${translation.id}`, {
    responseType: 'text'
  })
  .pipe(
    tap(() => {
      this.translations.splice(index, 1);
      this.saveCachedTranslations(this.translations);
      this.translationsSubject.next([...this.translations]);
    }),
    map(() => void 0),
    catchError((error) => {
      console.error('Failed to delete translation', error);
      return throwError(() => error);
    })
  );
  }
  /**
 * Delete entire translation key (Creator only)
 * Calls: DELETE /api/translations/{keyName}
 */
deleteKeyAsCreator(keyName: string): Observable<any> {
  return this.http.delete(`${this.translationKeyUrl}/${keyName}`);
}

/**
 * Delete specific translation (Translator only)
 * Calls: DELETE /api/translations/{keyName}/{languageCode}
 */
deleteTranslationAsTranslator(keyName: string, languageCode: string): Observable<any> {
  return this.http.delete(`${this.translationValueUrl}/${keyName}/${languageCode}`);
}
  // --- Bulk save ---
  // notifySaveCompleted() called ONCE via finalize() — covers both success and error.
  // Do NOT call notifySaveCompleted() again after calling this method.
  upsertTranslations(translations: AddTranslationRequest[]): Observable<any> {
    if (translations.length === 0) {
      return of([]).pipe(
        finalize(() => {
          this.notifySaveCompleted();
        })
      );
    }

    const payload = { translations };

    return this.http.post<any>(`${this.translationValueUrl}/bulk`, payload).pipe(
      finalize(() => {
        console.log('Bulk upsert completed');
        this.notifySaveCompleted();
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('Failed to bulk upsert translations', error);
        return throwError(() => error);
      })
    );
  }

  // --- Publish ---

  publishTranslations(): Observable<any> {
    return this.http.post(`${this.translationValueUrl}/publish`, {});
  }

  publishLanguage(languageCode: string): Observable<any> {
    return this.http.post(
      `${this.translationValueUrl}/publish/${languageCode}`,
      {}
    );
  }
  
 publishTranslationsDownload(fileType: string): Observable<Blob> {
  return this.http.post(
    `${this.translationValueUrl}/publish/download`,
    { fileType },
    { responseType: 'blob' }
  );
}

publishLanguageDownload(
  languageCode: string,
  fileType: string
): Observable<Blob> {
  return this.http.post(
    `${this.translationValueUrl}/publish/${languageCode}/download`,
    { fileType },
    { responseType: 'blob' }
  );
}

  // --- Stats ---

  getStats(): DashboardStats {
    const totalKeys = this.translations.filter((t) =>
      t.translationKey?.trim()
    ).length;

    const translated = this.translations.filter(
      (t) => t.translationKey?.trim() && t.translation?.trim()
    ).length;

    const completion =
      totalKeys > 0 ? Math.round((translated / totalKeys) * 100) : 0;

    return { totalKeys, translated, completion };
  }

  // --- Private helpers ---

  private mapApiTranslationKey(item: any): Translation {
    const nestedValues = Array.isArray(item.translationValues)
      ? item.translationValues
      : Array.isArray(item.values)
      ? item.values
      : Array.isArray(item.Translations)
      ? item.Translations
      : Array.isArray(item.translationValue)
      ? item.translationValue
      : [];

    const nestedTranslation = nestedValues.length
      ? nestedValues[0].value ??
        nestedValues[0].translation ??
        nestedValues[0].text ??
        ''
      : '';

    const id = item.id ?? item.Id ?? item.keyId ?? item.KeyId;
    const keyId = item.keyId ?? item.KeyId ?? item.id ?? item.Id;
    const keyName =
      item.key_name ??
      item.Key_name ??
      item.keyName ??
      item.KeyName ??
      item.key ??
      item.Key ??
      item.translationKey?.keyName ??
      item.translationKey?.KeyName ??
      item.translationKey ??
      item.TranslationKey;
    const originalText =
      item.originalText ??
      item.OriginalText ??
      item.original_text ??
      item.Original_text ??
      item.translationKey?.originalText ??
      item.translationKey?.OriginalText;
    const projectId =
      item.projectId ?? item.ProjectId ?? item.project_id ?? item.Project_id;
    const translation =
      item.translation ?? item.value ?? item.Value ?? item.text ?? item.Text ?? nestedTranslation;

    return {
      id,
      keyId: keyId != null ? Number(keyId) : undefined,
      translationKey: keyName || '',
      originalText: originalText || '',
      translation: translation || '',
      isModified: false,
      projectId: projectId != null ? Number(projectId) : undefined,
      tags: projectId?.toString() || '',
      client: '',
      project: projectId?.toString() || '',
    };
  }

  private buildKeyPayload(translation: Translation): {
    keyName: string;
    originalText: string;
    projectId: number;
  } {
    return {
      keyName: translation.translationKey?.trim() || '',
      originalText: translation.originalText?.trim() || '',
      projectId: this.resolveProjectId(translation),
    };
  }

  private resolveProjectId(translation: Translation): number {
    const projectId =
      translation.projectId ??
      (translation.tags ? Number(translation.tags) : undefined);

    if (!Number.isFinite(projectId) || (projectId as number) <= 0) {
      throw new Error('Project selection is required');
    }

    return projectId as number;
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

    if (Array.isArray(response.translations)) {
      return response.translations;
    }

    if (Array.isArray(response.translationValues)) {
      return response.translationValues;
    }

    return [];
  }

  private loadCachedTranslations(): Translation[] {
    if (typeof localStorage === 'undefined') return [];
    try {
      const stored = localStorage.getItem(this.storageKey);
      return stored ? (JSON.parse(stored) as Translation[]) : [];
    } catch {
      return [];
    }
  }

  private saveCachedTranslations(translations: Translation[]): void {
    if (typeof localStorage === 'undefined') return;
    try {
      localStorage.setItem(this.storageKey, JSON.stringify(translations));
    } catch {
      // ignore storage errors
    }
  }
}