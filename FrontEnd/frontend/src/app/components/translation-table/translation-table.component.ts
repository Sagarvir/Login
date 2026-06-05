import {
  Component,
  OnInit,
  OnDestroy,
  AfterViewInit,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { Subscription } from 'rxjs';
import { TranslationService } from '../../services/translation.service';
import { LanguageService } from '../../services/language.service';
import { ProjectService } from '../../services/project.service';
import { Translation, Language } from '../../models/translation.model';
import { ProjectOption } from '../../models/project.model';
import { DeleteConfirmDialogComponent } from '../delete-confirm-dialog/delete-confirm-dialog.component';
import { AddTranslationDialogComponent, AddTranslationDialogResult } from '../add-translation-dialog/add-translation-dialog.component';
import { AuthService } from '../../core/services/auth.service';
import { isDataSource } from '@angular/cdk/collections';

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
export class TranslationTableComponent implements OnInit, AfterViewInit, OnDestroy {
  displayedColumns: string[] = [
    'translationKey',
    'originalText',
    'translation',
    'tags',
    'actions',
  ];

  dataSource = new MatTableDataSource<Translation>();
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  languages: Language[] = [];
  selectedLanguage = 'EN';
  projects: ProjectOption[] = [];

  // Dictionary of key -> translated value for the selected language
  translationDict: { [key: string]: string } = {};

  private saveRequestedSub: Subscription | null = null;
  private translationsSub: Subscription | null = null;

  constructor(
    private translationService: TranslationService,
    private languageService: LanguageService,
    private projectService: ProjectService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
     private authService: AuthService
  ) {}
  isCreator(): boolean {
  return this.authService.isCreator(); // adjust based on your logic
}
isTranslator(): boolean {
  return this.authService.isTranslator(); // adjust based on your logic
}
isAdmin(): boolean {
  return this.authService.isAdmin(); // adjust based on your logic
}
isViewer(): boolean {
  return this.authService.hasRole('viewer'); // adjust based on your logic
}
  ngOnInit(): void {
    this.setFilterPredicate();
    this.loadLanguages();
    this.loadProjects();

    // Keep table in sync with service translations$
    this.translationsSub = this.translationService
      .getTranslations()
      .subscribe((translations) => {
        this.dataSource.data = translations;
        if (this.paginator) {
          this.dataSource.paginator = this.paginator;
        }
      });

    // Listen for save requests from the header.
    // saveRequested$ is a plain Subject — will NOT fire on subscribe.
    this.saveRequestedSub = this.translationService.saveRequested$.subscribe(
      () => this.executeTableSave()
    );
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  ngOnDestroy(): void {
    this.saveRequestedSub?.unsubscribe();
    this.translationsSub?.unsubscribe();
  }

  // --- Language ---

  loadLanguages(): void {
    this.languageService.getLanguages().subscribe({
      next: (languages) => {
        this.languages = languages;
        if (languages.length > 0) {
          this.selectedLanguage = languages[0].code;
          this.translationService.setSelectedLanguage(this.selectedLanguage);
          this.loadAllData(this.selectedLanguage);
        }
      },
      error: (err) => console.error('Failed to load languages', err),
    });
  }

  onLanguageChange(): void {
     if (!(this.isCreator()||this.isAdmin()||this.isViewer())) {
    this.snackBar.open('Access denied', 'Close', { duration: 3000 });
    return;
  }
    this.translationService.setSelectedLanguage(this.selectedLanguage);
    this.loadAllData(this.selectedLanguage);
  }

  private loadProjects(): void {
    this.projectService.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.assignProjectNames();
      },
      error: (err) => console.error('Failed to load projects', err),
    });
  }

  // Loads both the table rows and the translation value dictionary for a language
  private loadAllData(languageCode: string): void {
    this.translationService.loadTranslations(languageCode).subscribe({
      next: () => {
        this.assignTranslationsToRows();
        this.assignProjectNames();
      },
      error: (err) => {
        console.error('Failed to load translations', err);
        this.snackBar.open('Failed to load translation keys.', 'Close', {
          duration: 4000,
        });
        this.dataSource.data = [];
      },
    });

    this.translationService.getAllTranslations(languageCode).subscribe({
      next: (dict) => {
        this.translationDict = dict;
        this.assignTranslationsToRows();
      },
      error: (err) => {
        console.error('Failed to load translation dictionary', err);
        this.snackBar.open('Failed to load translation values.', 'Close', {
          duration: 4000,
        });
        this.translationDict = {};
      },
    });
  }

  // --- Table helpers ---

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }

  private assignTranslationsToRows(): void {
    if (!this.translationDict || this.dataSource.data.length === 0) {
      return;
    }

    this.dataSource.data.forEach((item) => {
      item.translation =
        this.translationDict[item.translationKey] ?? item.translation ?? '';
      item.isModified = false;
    });
    this.dataSource.data = [...this.dataSource.data];
  }

  markAsModified(element: Translation): void {
    element.isModified = true;
  }

  getProjectName(element: Translation): string {
    const projectId = element.projectId ?? (element.tags ? Number(element.tags) : undefined);

    if (projectId == null || Number.isNaN(projectId)) {
      return '';
    }

    return this.projects.find((project) => project.id === projectId)?.name || element.project || String(projectId);
  }

  private setFilterPredicate(): void {
    this.dataSource.filterPredicate = (data: Translation, filter: string) => {
      const search = filter.trim().toLowerCase();
      const fields = [
        data.translationKey,
        data.originalText,
        data.projectName,
        data.project,
        data.tags,
      ];
      return fields
        .filter((value): value is string => typeof value === 'string' && value.length > 0)
        .some((value) => value.toLowerCase().includes(search));
    };
  }

  private assignProjectNames(): void {
    if (this.projects.length === 0 || this.dataSource.data.length === 0) {
      return;
    }

    this.dataSource.data = this.dataSource.data.map((item) => ({
      ...item,
      projectName:
        this.projects.find((project) => project.id === item.projectId)?.name || item.project || '',
    }));

    this.dataSource.filter = this.dataSource.filter;
  }

  // --- CRUD ---

  updateTranslation(index: number, translation: Translation): void {
    this.translationService.updateTranslation(index, translation).subscribe({
      next: () =>
        this.snackBar.open('Translation updated', 'Close', { duration: 2000 }),
      error: (err) => {
        const msg = err.error?.message || err.message || 'Failed to update';
        this.snackBar.open(msg, 'Close', { duration: 3000 });
      },
    });
  }

  deleteTranslation(index: number): void {
    const dialogRef = this.dialog.open(DeleteConfirmDialogComponent);
    dialogRef.afterClosed().subscribe((confirmed) => {
      
      if (!confirmed) return;
      
      this.translationService.deleteTranslation(index).subscribe({
        next: () =>
          this.snackBar.open('Translation deleted', 'Close', { duration: 2000 }),
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to delete';
          this.snackBar.open(msg, 'Close', { duration: 3000 });
        },
      });
    });
  }

  addNewTranslation(): void {
    const dialogRef = this.dialog.open(AddTranslationDialogComponent, {
      width: '520px',
    });

    dialogRef.afterClosed().subscribe((result: AddTranslationDialogResult | undefined) => {
      if (!result) return;

      const newTranslation: Translation = {
        translationKey: result.translationKey || '',
        originalText: result.originalText || '',
        translation: '',
        projectId: result.projectId,
        tags: result.projectId?.toString() || '',
        client: '',
        project: result.projectId?.toString() || '',
      };

      this.translationService.addTranslation(newTranslation).subscribe({
        next: () => {
          this.snackBar.open('Translation added', 'Close', { duration: 2000 });
          this.loadAllData(this.selectedLanguage);
        },
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to add translation';
          this.snackBar.open(msg, 'Close', { duration: 3000 });
        },
      });
    });
  }

  // --- Save ---

  // Called by saveRequested$ subscription (from header Save button).
  // Rule: notifySaveCompleted() must be called in every code path.
  // When upsertTranslations() is called, it handles notification via finalize().
  // All other exit paths (no changes, error before HTTP) call it explicitly.
  executeTableSave(): void {
    let modifiedTranslations: any[];

    try {
      modifiedTranslations = this.buildSavePayload();
    } catch (err: any) {
      this.snackBar.open(
        err?.message || 'Failed to prepare save',
        'Close',
        { duration: 3000 }
      );
      // Must notify — header is waiting
      this.translationService.notifySaveCompleted();
      return;
    }

    if (modifiedTranslations.length === 0) {
      this.snackBar.open('No changes to save', 'Close', { duration: 3000 });
      // Must notify — header is waiting
      this.translationService.notifySaveCompleted();
      return;
    }

    // upsertTranslations calls notifySaveCompleted() in finalize() — do NOT call it again here
    this.translationService.upsertTranslations(modifiedTranslations).subscribe({
      next: () => {
        this.dataSource.data.forEach((item) => (item.isModified = false));
        this.snackBar.open('Translations saved successfully!', 'Close', {
          duration: 3000,
        });
        // Reload dictionary so table shows the newly saved values
        this.translationService
          .getAllTranslations(this.selectedLanguage)
          .subscribe({
            next: (dict) => (this.translationDict = dict),
          });
      },
      error: (err) => {
        const msg =
          err?.error?.message || err?.message || 'Failed to save translations';
        this.snackBar.open(msg, 'Close', { duration: 3000 });
      },
    });
  }

  // Builds the payload for upsertTranslations. Throws if any modified item is missing a keyId.
  private buildSavePayload(): any[] {
    return this.dataSource.data
      .filter((item) => item.isModified && item.translation?.trim())
      .map((item) => {
        const keyName = item.translationKey?.trim() || item.keyName?.trim();
        if (!keyName) {
          throw new Error(
            `Cannot save: key name missing for row with original text '${item.originalText}'`
          );
        }
        return {
          keyName,
          value: item.translation,
          languageCode: this.selectedLanguage.toUpperCase(),
        };
      });
  }
}