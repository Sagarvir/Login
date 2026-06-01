import {
  Injectable,
} from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  Subject,
  catchError,
  finalize,
  map,
  of,
  tap,
  throwError,
} from 'rxjs';
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
  private readonly storageKey = 'translationManager.translations';

  private translations: Translation[] = [];

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
    this.saveRequestedSubject.next();
  }

  notifySaveCompleted(): void {
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
    const url = `${this.translationValueUrl}/with-translations?languageCode=${languageCode.toUpperCase()}`;

    return this.http.get<any[]>(url).pipe(
      tap((response) => {
        const items = Array.isArray(response) ? response : [];
        const backendTranslations = items.map((item: any) =>
          this.mapApiTranslationKey(item)
        );
        this.setTranslations(backendTranslations);
      }),
      map(() => this.translations),
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
    const url = `${this.translationValueUrl}/with-translations?languageCode=${upper}`;

    return this.http.get<any[]>(url).pipe(
      map((response) => {
        const dictionary: { [key: string]: string } = {};
        if (Array.isArray(response)) {
          response.forEach((item) => {
            const key = item.key ?? item.Key ?? item.keyName ?? item.KeyName;
            const value = item.value ?? item.Value;
            if (key && value) {
              dictionary[key] = value;
            }
          });
        }
        return dictionary;
      }),
      catchError((error) => {
        console.error('Failed to load translations for language:', languageCode, error);
        return of({} as { [key: string]: string });
      })
    );
  }

  addTranslation(translation: Translation): Observable<Translation> {
    const body = {
      keyName: translation.translationKey,
      originalText: translation.originalText,
      projectId: translation.tags ? Number(translation.tags) : 1,
    };

    return this.http.post<any>(this.translationKeyUrl, body).pipe(
      tap(() => {
        this.loadTranslations().subscribe();
      }),
      catchError((error) => {
        console.error('Failed to add translation', error);
        return throwError(() => error);
      })
    );
  }

  updateTranslation(index: number, translation: Translation): Observable<Translation> {
    if (translation.id != null) {
      return this.http
        .put<Translation>(`${this.translationKeyUrl}/${translation.id}`, {
          KeyName: translation.translationKey,
          originalText: translation.originalText,
          projectId: translation.tags ? Number(translation.tags) : 1,
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

  // --- Bulk save ---
  // notifySaveCompleted() called ONCE via finalize() — covers both success and error.
  // Do NOT call notifySaveCompleted() again after calling this method.
  upsertTranslations(translations: AddTranslationRequest[]): Observable<any> {
    const url = `${this.translationValueUrl}/bulk`;

    return this.http
      .post(url, { translations }, { responseType: 'text' })
      .pipe(
        finalize(() => {
          this.notifySaveCompleted();
        }),
        catchError((error) => {
          console.error('Failed to save translations', error);
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
      : [];

    const nestedTranslation = nestedValues.length
      ? nestedValues[0].value ??
        nestedValues[0].translation ??
        nestedValues[0].text ??
        ''
      : '';

    const id = item.id ?? item.Id ?? item.keyId ?? item.KeyId;
    const keyName = item.keyName ?? item.KeyName ?? item.key ?? item.Key;
    const originalText = item.originalText ?? item.OriginalText;
    const projectId = item.projectId ?? item.ProjectId;
    const translation =
      item.translation ?? item.value ?? item.Value ?? nestedTranslation;

    return {
      id,
      translationKey: keyName || '',
      originalText: originalText || '',
      translation: translation || '',
      isModified: false,
      tags: projectId?.toString() || '',
      client: '',
      project: projectId?.toString() || '',
    };
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