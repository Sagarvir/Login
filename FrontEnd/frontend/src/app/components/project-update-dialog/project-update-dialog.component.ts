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

export interface ProjectUpdateDialogResult {
  projectId: number;
  newName: string;
}

@Component({
  selector: 'app-project-update-dialog',
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
  templateUrl: './project-update-dialog.component.html',
  styleUrls: ['./project-update-dialog.component.scss'],
})
export class ProjectUpdateDialogComponent implements OnInit {
  form: FormGroup;
  readonly projects = signal<ProjectOption[]>([]);
  readonly loadingProjects = signal(false);
  readonly projectLoadError = signal('');

  constructor(
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<ProjectUpdateDialogComponent>,
    private projectService: ProjectService
  ) {
    this.form = this.formBuilder.group({
      projectId: [null, Validators.required],
      newName: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadingProjects.set(true);
    this.projectService.getProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects || []);
        this.loadingProjects.set(false);
        if (projects.length === 1) {
          this.form.controls['projectId'].setValue(projects[0].id);
          this.form.controls['newName'].setValue(projects[0].name);
        }
      },
      error: () => {
        this.projectLoadError.set('Unable to load projects.');
        this.loadingProjects.set(false);
      },
    });
  }

  onProjectSelected(projectId: number): void {
    const selected = this.projects().find((project) => project.id === projectId);
    if (selected) {
      this.form.controls['newName'].setValue(selected.name);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.form.value as ProjectUpdateDialogResult);
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
