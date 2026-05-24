import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatPaginator } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { TranslationService } from '../../services/translation.service';
import { LanguageService } from '../../services/language.service';
import { Translation, Language } from '../../models/translation.model';
import { DeleteConfirmDialogComponent } from '../delete-confirm-dialog/delete-confirm-dialog.component';
import { AddTranslationDialogComponent } from '../add-translation-dialog/add-translation-dialog.component';

@Component({
  selector: 'app-translation-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatCardModule,
    MatSelectModule,
  ],
  templateUrl: './translation-table.component.html',
  styleUrl: './translation-table.component.scss',
})
export class TranslationTableComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = [
    'translationKey',
    'originalText',
    'translation',
    'tags',
    'actions',
  ];
  dataSource = new MatTableDataSource<Translation>();
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  searchTerm = '';
  languages: Language[] = [];
  selectedLanguage: string = 'EN';
  translations: { [key: string]: string } = {};

  constructor(
    private translationService: TranslationService,
    private languageService: LanguageService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadLanguages();

    // ✅ Load data from backend
    this.translationService.loadTranslations().subscribe({
      next: () => {
        console.log('Translations loaded successfully');
      },
      error: (err) => {
        console.error('Failed to load translations in table', err);
      }
    });

    // ✅ Subscribe to data
    this.translationService.getTranslations().subscribe((translations) => {
      this.dataSource.data = translations;

      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
    });

    // ✅ Subscribe to save requests from header
    this.translationService.saveRequested$.subscribe(() => {
      this.executeTableSave();
    });
  }

  loadLanguages(): void {
    this.languageService.getLanguages().subscribe({
      next: (languages) => {
        this.languages = languages;
        if (languages.length > 0) {
          this.selectedLanguage = languages[0].code;
          this.loadTranslationsForLanguage(this.selectedLanguage);
        }
      },
      error: (error) => {
        console.error('Error loading languages:', error);
      },
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.searchTerm = filterValue.trim().toLowerCase();
    this.dataSource.filter = this.searchTerm;
  }

  updateTranslation(index: number, translation: Translation): void {
    this.translationService.updateTranslation(index, translation).subscribe({
      next: () => {
        this.snackBar.open('Translation updated successfully', 'Close', {
          duration: 2000,
        });
      },
      error: (err) => {
        const message = err.error?.message || err.message || 'Failed to update translation';
        this.snackBar.open(message, 'Close', { duration: 3000 });
      }
    });
  }

  deleteTranslation(index: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent);

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.translationService.deleteTranslation(index).subscribe({
        next: () => {
          this.snackBar.open('Translation deleted successfully', 'Close', {
            duration: 2000,
          });
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to delete translation';
          this.snackBar.open(message, 'Close', { duration: 3000 });
        }
      });
    });
  }

  addNewTranslation(): void {
    const dialogRef = this.dialog.open(AddTranslationDialogComponent, {
      width: '520px',
    });

    dialogRef.afterClosed().subscribe((result: Translation | undefined) => {
      if (!result) {
        return;
      }

      const newTranslation: Translation = {
  translationKey: result.translationKey || '',
  originalText: result.originalText || '',
  translation: '',
  tags: result.tags || '',
  client: '',
  project: ''
};

      this.translationService.addTranslation(newTranslation).subscribe({
        next: () => {
          this.translationService.loadTranslations().subscribe();
          this.snackBar.open('New translation added', 'Close', {
            duration: 2000,
          });
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to add translation';
          this.snackBar.open(message, 'Close', { duration: 3000 });
        }
      });
    });
  }

  loadTranslationsForLanguage(languageCode: string): void {
    console.log('Loading translations for language:', languageCode);
    this.translationService.getAllTranslations(languageCode).subscribe({
      next: (translationDict) => {
        this.translations = translationDict;
        console.log('Translations loaded successfully for language:', languageCode);
        console.log('Translation dictionary:', this.translations);
        
        // Mark all rows as not modified when fresh translations are loaded
        this.dataSource.data.forEach((item) => {
          item.isModified = false;
        });
      },
      error: (error) => {
        console.error('Error loading translations for language:', languageCode, error);
        this.translations = {};
      },
    });
  }

  onLanguageChange(): void {
    console.log('Language changed to:', this.selectedLanguage);
    // Update the service's selected language
    this.translationService.setSelectedLanguage(this.selectedLanguage);
    // Load translations for the selected language in the service
    this.translationService.loadTranslations(this.selectedLanguage).subscribe({
      next: () => {
        console.log('Translations reloaded for language:', this.selectedLanguage);
        // Also load the translation dictionary for the table
        this.loadTranslationsForLanguage(this.selectedLanguage);
      },
      error: (error) => {
        console.error('Error loading translations for language:', this.selectedLanguage, error);
      }
    });
  }

  getTranslation(key: string): string {
    const value = this.translations[key];
    console.log(`Getting translation for key '${key}':`, value || `(empty)`);
    return value || '';
  }

  markAsModified(element: Translation): void {
    element.isModified = true;
    console.log(`Marked row with key '${element.translationKey}' as modified`);
  }

  getModifiedTranslations(): any[] {
    const modified = this.dataSource.data
      .filter((item) => item.isModified && item.translation?.trim())
      .map((item) => {
        console.log('Full item object:', item);
        console.log('item.id:', item.id);
        console.log('item.translationKey:', item.translationKey);

        // Ensure keyId is properly extracted from the item
        const keyId = item.id ? Number(item.id) : null;

        if (!keyId) {
          console.error('ERROR: keyId is null for item:', item);
          throw new Error(`Cannot save translation: keyId is missing for key '${item.translationKey}'`);
        }

        const request = {
          keyId: keyId,
          value: item.translation,
          languageCode: this.selectedLanguage.toUpperCase(),
        };
        console.log('Final request:', request);
        return request;
      });

    console.log('Modified translations to send:', modified);
    return modified;
    }
      getStats() {
    const totalKeys = this.dataSource.data.filter(
      (t) => t.translationKey?.trim()
    ).length;

    const translated = this.dataSource.data.filter((t) => {
      const value = this.translations[t.translationKey];
      return value && value.trim() !== '';
    }).length;

    const completion =
      totalKeys > 0 ? Math.round((translated / totalKeys) * 100) : 0;

    return { totalKeys, translated, completion };
  }

  saveTranslations(): void {
    this.executeTableSave();
  }

  executeTableSave(): void {
    console.log('Current table data:', this.dataSource.data);
    console.log('Selected language:', this.selectedLanguage);

    try {
      const modifiedTranslations = this.getModifiedTranslations();

      if (modifiedTranslations.length === 0) {
        this.snackBar.open('No changes to save', 'Close', { duration: 3000 });
        return;
      }

      console.log('Saving modified translations:', modifiedTranslations);

      this.translationService.upsertTranslations(modifiedTranslations).subscribe({
        next: (response) => {
          console.log('Translations saved successfully:', response);

          // Reset isModified flag
          this.dataSource.data.forEach((item) => {
            if (item.isModified) {
              item.isModified = false;
            }
          });
          this.snackBar.open('Translations saved successfully!', 'Close', {
  duration: 3000,
});
this.translationService.notifySaveCompleted();

          // Reload translations for current language
          this.loadTranslationsForLanguage(this.selectedLanguage);

          this.snackBar.open('Translations saved successfully!', 'Close', {
            duration: 3000,
          });
        },
        error: (error) => {
          console.error('Error saving translations:', error);
          const message = error?.error?.message || error?.message || 'Failed to save translations';
          this.snackBar.open(message, 'Close', { duration: 3000 });
          this.translationService.notifySaveCompleted();
        },
      });
    } catch (error: any) {
      console.error('Error preparing translations:', error);
      this.snackBar.open(error?.message || 'Failed to prepare translations for save', 'Close', { duration: 3000 });
    }
  }
}
