import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Translation, DashboardStats, AddTranslationRequest } from '../models/translation.model';

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

  private selectedLanguageSubject = new BehaviorSubject<string>('EN');
  public selectedLanguage$ = this.selectedLanguageSubject.asObservable();

  private saveRequestedSubject = new BehaviorSubject<void>(undefined);
  public saveRequested$ = this.saveRequestedSubject.asObservable();

  private pendingSaveSubject = new BehaviorSubject<{
    languageCode: string;
    modifiedTranslations: any[];
  } | null>(null);
  public pendingSave$ = this.pendingSaveSubject.asObservable();
  // ✅ ADD BELOW pendingSave$
// ✅ ADD THIS BLOCK RIGHT HERE
private saveCompletedSubject = new BehaviorSubject<boolean>(false);
public saveCompleted$ = this.saveCompletedSubject.asObservable();

notifySaveCompleted(): void {
  this.saveCompletedSubject.next(true);
}
  constructor(private http: HttpClient) {
    this.translations = this.loadCachedTranslations();
  }

  setSelectedLanguage(languageCode: string): void {
    this.selectedLanguageSubject.next(languageCode);
  }
  
  getSelectedLanguage(): string {
  return this.selectedLanguageSubject.value;
}

  setPendingSave(languageCode: string, modifiedTranslations: any[]): void {
    this.pendingSaveSubject.next({ languageCode, modifiedTranslations });
  }

  requestSave(): void {
    this.saveRequestedSubject.next();
  }

  loadTranslations(languageCode = 'EN'): Observable<Translation[]> {
    const params = { languageCode };
    // Use the /with-translations endpoint which includes KeyId, Key, OriginalText, and Value
    const url = `${this.translationValueUrl}/with-translations?languageCode=${languageCode.toUpperCase()}`;

    return this.http.get<any[]>(url).pipe(
      tap((response) => {
        console.log('=== RAW API RESPONSE ===', response);
        if (Array.isArray(response) && response.length > 0) {
          console.log('First item from API:', response[0]);
          console.log('API response structure keys:', Object.keys(response[0]));
        }
      }),
      tap((response) => {
        console.log('TranslationValue API response', response);
        const items = Array.isArray(response) ? response : [];

        // Don't merge with old cache when loading specific language - use fresh data only
        const backendTranslations = (items || [])
          .map((item: any) => this.mapApiTranslationKey(item));

        // Use only the backend translations for the current language, no cache merging
        this.setTranslations(backendTranslations);
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

    // Handle both PascalCase (from ASP.NET) and camelCase responses
    // Try: id, Id, keyId, KeyId - this handles both endpoints
    const id = item.id ?? item.Id ?? item.keyId ?? item.KeyId;
    const keyName = item.keyName ?? item.KeyName ?? item.key ?? item.Key;
    const originalText = item.originalText ?? item.OriginalText;
    const projectId = item.projectId ?? item.ProjectId;
    // For with-translations endpoint response, also check Value field
    const translation = item.translation ?? item.value ?? item.Value ?? nestedTranslation;

    console.log('=== MAPPING DEBUG ===');
    console.log('Raw item:', item);
    console.log('Extracted - id:', id, 'keyName:', keyName, 'originalText:', originalText, 'translation:', translation);

    return {
      id: id,
      translationKey: keyName || '',
      originalText: originalText || '',
      translation: translation || '',
      isModified: false,
      tags: projectId?.toString() || '',
      client: '',
      project: projectId?.toString() || ''
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

  getAllTranslations(languageCode: string): Observable<{ [key: string]: string }> {
    const upperLanguageCode = languageCode.toUpperCase();
    const url = `${this.translationValueUrl}/with-translations?languageCode=${upperLanguageCode}`;

    return this.http.get<any[]>(url).pipe(
      map((response) => {
        console.log('API Response for language', upperLanguageCode, ':', response);
        const dictionary: { [key: string]: string } = {};

        if (Array.isArray(response)) {
          response.forEach((item) => {
            // Handle both camelCase (key) and PascalCase (Key) from API
            const key = item.key ?? item.Key;
            // Handle both camelCase (value) and PascalCase (Value) from API
            const value = item.value ?? item.Value;

            if (item && key && value) {
              dictionary[key] = value;
            }
          });
        }

        console.log('Converted dictionary:', dictionary);
        return dictionary;
      }),
      catchError((error) => {
        console.error('Failed to load translations for language:', languageCode, error);
        return of({} as { [key: string]: string });
      })
    );
  }

  upsertTranslations(translations: AddTranslationRequest[]): Observable<any> {
    const bulkRequest = { translations };
    const url = `${this.translationValueUrl}/bulk`;

    console.log('Sending bulk request to:', url);
    console.log('Request payload:', JSON.stringify(bulkRequest, null, 2));
    console.log('Translations array:', translations);

    return this.http.post(url, bulkRequest, { responseType: 'text' }).pipe(
      tap((response) => {
        console.log('Translations upserted successfully:', response);
        this.pendingSaveSubject.next(null);
      }),
      catchError((error) => {
        console.error('Failed to upsert translations', error);
        return throwError(() => error);
      })
    );
  }

  executePendingSave(): Observable<any> {
    const pending = this.pendingSaveSubject.value;
    if (!pending || pending.modifiedTranslations.length === 0) {
      return of({ message: 'No changes to save' });
    }

    return this.upsertTranslations(pending.modifiedTranslations);
  }
  publishTranslations(): Observable<any> {
  return this.http.post(
    `${this.translationValueUrl}/publish`,
    {}
  );
}
publishLanguage(
  languageCode: string
): Observable<any> {

  return this.http.post(
    `${this.translationValueUrl}/publish/${languageCode}`,
    {}
  );
}

  getStats(): DashboardStats {
    const totalKeys = this.translations.filter((t) => t.translationKey?.trim()).length;
    const translated = this.translations.filter((t) => {
  return (
    t.translationKey?.trim() &&
    t.translation &&
    t.translation.trim() !== ''
  );
}).length;
    const completion = totalKeys > 0 ? Math.round((translated / totalKeys) * 100) : 0;

    return {
      totalKeys,
      translated,
      completion,
    };
  }
}
