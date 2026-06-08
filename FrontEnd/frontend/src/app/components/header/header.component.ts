import {
  Component,
  OnInit,
  OnDestroy,
  NgZone,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { TranslationService } from '../../services/translation.service';
import { AuthService } from '../../core/services/auth.service';
import { ProjectService } from '../../services/project.service';
import { ProjectUpdateDialogComponent, ProjectUpdateDialogResult } from '../project-update-dialog/project-update-dialog.component';

@Component({
  selector: 'app-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit, OnDestroy {
  userInfo = {
    userId: '0',
    language: '',
    role: 'Creator',
  };

  isSaving = false;
  selectedFileType = 'both';

  private saveCompletedSub: Subscription | null = null;
  private saveTimeoutId: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private translationService: TranslationService,
    private projectService: ProjectService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router,
    private authService: AuthService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {
    this.userInfo.role = this.authService.getUserRole() || 'Creator';

   this.userInfo.language = this.authService.getPreferredLanguage().toUpperCase();
   
    const role = this.authService.getUserRole()?.trim().toLowerCase();
    const roleIdMap: Record<string, string> = {
      admin: '1',
      translator: '2',
      creator: '3',
      viewer: '4',
    };
    this.userInfo.userId = roleIdMap[role ?? ''] ?? '0';
  }
  ngOnDestroy(): void {
  this.saveCompletedSub?.unsubscribe();
  this.clearSaveTimeout();
}
    isCreator(): boolean {
  return this.authService.isCreator();
}
  isAdmin(): boolean {
    return this.authService.isAdmin();
  }
  isTranslator(): boolean {
    return this.authService.isTranslator();
  }
  isViewer(): boolean {
    return this.authService.hasRole('viewer');  
  }
  ngOnInit(): void {
    this.saveCompletedSub = this.translationService.saveCompleted$.subscribe(() => {
      // Run inside Angular zone, then explicitly trigger change detection.
      // This prevents NG0100 by ensuring the state change is processed
      // in a controlled change detection pass, not mid-cycle.
      this.ngZone.run(() => {
        this.isSaving = false;
        this.clearSaveTimeout();
        this.cdr.detectChanges();
      });
    });

  }

  saveTranslations(): void {
  if (!this.isTranslator()) {
    this.snackBar.open('Access denied', 'Close', { duration: 3000 });
    return;
  }

  if (this.isSaving) return;

  this.isSaving = true;
  this.cdr.detectChanges();

  this.saveTimeoutId = setTimeout(() => {
    this.ngZone.run(() => {
      if (this.isSaving) {
        this.isSaving = false;
        this.cdr.detectChanges();
        this.snackBar.open('Save timed out. Please try again.', 'Close', {
          duration: 3000,
        });
      }
    });
  }, 15000);

  this.translationService.requestSave();
}

 

 publishTranslations(): void {
  if (!(this.isCreator()||this.isAdmin())) {
    this.snackBar.open('Access denied', 'Close', { duration: 3000 });
    return;
  }

  const fileType = this.selectedFileType;
  if (!fileType) return;

  this.translationService
    .publishTranslationsDownload(fileType)
    .subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = `PublishedTranslations_${fileType}.zip`;
        a.click();

        window.URL.revokeObjectURL(url);

        this.snackBar.open(
          `Translations published and ${fileType} files downloaded`,
          'Close',
          { duration: 3000 }
        );
      },
      error: (error) => {
        console.error(error);
        this.snackBar.open('Publish download failed', 'Close', {
          duration: 3000,
        });
      },
    });
}

  publishCurrentLanguage(): void {
  if (!this.isCreator()) {
    this.snackBar.open('Access denied', 'Close', { duration: 3000 });
    return;
  }

  const fileType = this.selectedFileType;
  if (!fileType) return;

  const languageCode = this.translationService.getSelectedLanguage();

  this.translationService
    .publishLanguageDownload(languageCode, fileType)
    .subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = `${languageCode}_${fileType}_PublishedTranslations.zip`;
        a.click();

        window.URL.revokeObjectURL(url);

        this.snackBar.open(
          `${languageCode} ${fileType} files downloaded`,
          'Close',
          { duration: 3000 }
        );
      },
      error: (error) => {
        console.error(error);
        this.snackBar.open('Language publish download failed', 'Close', {
          duration: 3000,
        });
      },
    });
}

  openProjectUpdateDialog(): void {
    if (!(this.isCreator() || this.isAdmin())) {
      this.snackBar.open('Access denied', 'Close', { duration: 3000 });
      return;
    }

    const dialogRef = this.dialog.open(ProjectUpdateDialogComponent, {
      width: '520px',
    });

    dialogRef.afterClosed().subscribe((result: ProjectUpdateDialogResult | undefined) => {
      if (!result) {
        return;
      }

      this.projectService.updateProject(result.projectId, result.newName).subscribe({
        next: () => {
          this.snackBar.open('Project name updated successfully.', 'Close', {
            duration: 3000,
          });
        },
        error: (error) => {
          console.error(error);
          this.snackBar.open('Failed to update project name.', 'Close', {
            duration: 3000,
          });
        },
      });
    });
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.snackBar.open('You have been logged out', 'Close', { duration: 3000 });
    setTimeout(() => this.router.navigate(['/']), 1000);
  }

  private clearSaveTimeout(): void {
    if (this.saveTimeoutId !== null) {
      clearTimeout(this.saveTimeoutId);
      this.saveTimeoutId = null;
    }
  }
}