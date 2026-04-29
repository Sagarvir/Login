import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Translation, DashboardStats } from '../models/translation.model';

@Injectable({
  providedIn: 'root',
})
export class TranslationService {
  private translations: Translation[] = [];

  private translationsSubject = new BehaviorSubject<Translation[]>(
    this.translations
  );
  public translations$ = this.translationsSubject.asObservable();

  constructor() {}

  getTranslations(): Observable<Translation[]> {
    return this.translations$;
  }

  addTranslation(translation: Translation): void {
    this.translations.push(translation);
    this.translationsSubject.next([...this.translations]);
  }

  updateTranslation(index: number, translation: Translation): void {
    this.translations[index] = translation;
    this.translationsSubject.next([...this.translations]);
  }

  deleteTranslation(index: number): void {
    this.translations.splice(index, 1);
    this.translationsSubject.next([...this.translations]);
  }

  saveTranslations(): Observable<any> {
    return new Observable((observer) => {
      console.log('Saving translations:', this.translations);
      setTimeout(() => {
        observer.next({ success: true });
        observer.complete();
      }, 1000);
    });
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
