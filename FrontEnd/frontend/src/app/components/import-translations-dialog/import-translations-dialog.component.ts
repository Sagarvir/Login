import { Component,signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslationImportService } from '../../core/services/translation-import.service';
import { ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'app-import-translations-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatSnackBarModule,
  ],
  templateUrl: './import-translations-dialog.component.html',
  styleUrls: ['./import-translations-dialog.component.css']
})
export class ImportTranslationsDialogComponent {
  selectedFile: File | null = null;
  private isImporting = false;

  constructor(
    private dialogRef: MatDialogRef<ImportTranslationsDialogComponent>,
    private importService: TranslationImportService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];
    const name = file.name.toLowerCase();

    if (!name.endsWith('.json') && !name.endsWith('.xlf') && !name.endsWith('.xliff')) {
      this.snackBar.open('Only JSON, XLF, or XLIFF files are allowed.', 'Close', {
        duration: 3000,
      });
      input.value = '';
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
  }

importTranslations(): void {
  if (this.isImporting) {
    return;
  }

  if (!this.selectedFile) {
    this.snackBar.open('Please select a file.', 'Close', {
      duration: 3000,
    });
    return;
  }

  this.isImporting = true;

  this.importService.importTranslations(this.selectedFile).subscribe({
    next: (res: any) => {
      this.isImporting = false;

      this.snackBar.open(
        `Import completed. Inserted: ${res.insertedCount}, Updated: ${res.updatedCount}, Skipped: ${res.skippedCount}`,
        'Close',
        { duration: 6000 }
      );

      this.dialogRef.close(true);
    },
    error: (err: any) => {
      this.isImporting = false;

      const message =
        err.error?.errors?.[0]?.message ||
        err.error?.message ||
        err.message ||
        'Import failed.';

      this.snackBar.open(message, 'Close', {
        duration: 6000,
      });
    },
  });
}

  cancel(): void {
    this.dialogRef.close(false);
  }
}