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
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { TranslationService } from '../../services/translation.service';
import { AuthService } from '../../core/services/auth.service';

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
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit, OnDestroy {
  userInfo = {
    userId: '0',
    language: 'EN',
    role: 'Creator',
  };

  isSaving = false;

  private saveCompletedSub: Subscription | null = null;
  private saveTimeoutId: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private translationService: TranslationService,
    private snackBar: MatSnackBar,
    private router: Router,
    private authService: AuthService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {
    this.userInfo.role = this.authService.getUserRole() || 'Creator';

    const role = this.authService.getUserRole()?.trim().toLowerCase();
    const roleIdMap: Record<string, string> = {
      admin: '1',
      translator: '2',
      creator: '3',
      viewer: '4',
    };
    this.userInfo.userId = roleIdMap[role ?? ''] ?? '0';
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

  ngOnDestroy(): void {
    this.saveCompletedSub?.unsubscribe();
    this.clearSaveTimeout();
  }

  saveTranslations(): void {
    if (this.isSaving) return;

    this.isSaving = true;
    this.cdr.detectChanges();

    // Safety net: reset after 15s if saveCompleted$ never emits
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
  this.translationService.publishTranslationsDownload().subscribe({
    next: (blob: Blob) => {
      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = `PublishedTranslations.zip`;
      a.click();

      window.URL.revokeObjectURL(url);

      this.snackBar.open('Translations published and downloaded', 'Close', {
        duration: 3000
      });
    },
    error: (error) => {
      console.error(error);
      this.snackBar.open('Publish download failed', 'Close', {
        duration: 3000
      });
    }
  });
}

 publishCurrentLanguage(): void {
  const languageCode = this.translationService.getSelectedLanguage();

  this.translationService.publishLanguageDownload(languageCode).subscribe({
    next: (blob: Blob) => {
      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = `${languageCode}_PublishedTranslations.zip`;
      a.click();

      window.URL.revokeObjectURL(url);

      this.snackBar.open(`${languageCode} published and downloaded`, 'Close', {
        duration: 3000
      });
    },
    error: (error) => {
      console.error(error);
      this.snackBar.open('Language publish download failed', 'Close', {
        duration: 3000
      });
    }
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