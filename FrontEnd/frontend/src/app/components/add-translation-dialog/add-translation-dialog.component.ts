import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ProjectOption } from '../../models/project.model';
import { ProjectService } from '../../services/project.service';

export interface AddTranslationDialogResult {
  translationKey: string;
  originalText: string;
  projectId: number;
}

@Component({
  selector: 'app-add-translation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './add-translation-dialog.component.html',
  styleUrl: './add-translation-dialog.component.scss',
})
export class AddTranslationDialogComponent implements OnInit {
  form: FormGroup;
  readonly projects = signal<ProjectOption[]>([]);
  readonly loadingProjects = signal(false);
  readonly projectLoadError = signal('');

  constructor(
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<AddTranslationDialogComponent>,
    private projectService: ProjectService
  ) {
    this.form = this.formBuilder.group({
      translationKey: ['', Validators.required],
      originalText: ['', Validators.required],
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

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.form.value);
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
