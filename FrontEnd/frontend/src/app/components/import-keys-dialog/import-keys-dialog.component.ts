import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { ProjectOption } from '../../models/project.model';
import { ProjectService } from '../../services/project.service';
import { TranslationImportService } from '../../core/services/translation-import.service';

@Component({
  selector: 'app-import-keys-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatSnackBarModule,
  ],
  templateUrl: './import-keys-dialog.component.html',
  styleUrl: './import-keys-dialog.component.css',
})
export class ImportKeysDialogComponent implements OnInit {
  form: FormGroup;
  selectedFile: File | null = null;

  readonly projects = signal<ProjectOption[]>([]);
  readonly loadingProjects = signal(false);
  readonly projectLoadError = signal('');
  readonly importing = signal(false);

  constructor(
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<ImportKeysDialogComponent>,
    private projectService: ProjectService,
    private importService: TranslationImportService,
    private snackBar: MatSnackBar
  ) {
    this.form = this.formBuilder.group({
      projectId: [null, Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadingProjects.set(true);

    this.projectService.getProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loadingProjects.set(false);

        if (projects.length === 1) {
          this.form.controls['projectId'].setValue(projects[0].id);
        }
      },
      error: () => {
        this.projectLoadError.set('Unable to load projects.');
        this.loadingProjects.set(false);
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    const isValidFile =
      file.name.toLowerCase().endsWith('.json') ||
      file.name.toLowerCase().endsWith('.xlf') ||
      file.name.toLowerCase().endsWith('.xliff');

    if (!isValidFile) {
      this.snackBar.open('Only JSON, XLF, or XLIFF files are allowed.', 'Close', {
        duration: 3000,
      });
      input.value = '';
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
  }

  importKeys(): void {
    if (this.form.invalid || !this.selectedFile) {
      this.form.markAllAsTouched();
      this.snackBar.open('Please select project and file.', 'Close', {
        duration: 3000,
      });
      return;
    }

    this.importing.set(true);

    this.importService.importKeys(this.selectedFile, this.form.value.projectId).subscribe({
      next: (res) => {
        this.importing.set(false);

        this.snackBar.open(
          `Import completed. Inserted: ${res.insertedCount}, Skipped: ${res.skippedCount}`,
          'Close',
          { duration: 5000 }
        );

        this.dialogRef.close(true);
      },
      error: (err) => {
        this.importing.set(false);

        const message =
          err.error?.errors?.[0]?.message ||
          err.error?.message ||
          'Import failed.';

        this.snackBar.open(message, 'Close', {
          duration: 5000,
        });
      },
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}