import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { TranslationService } from '../../services/translation.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    MatSnackBarModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  userInfo = {
    userId: '101',
    language: 'EN',
    role: 'Creator',
  };

  isSaving = false;

  constructor(
    private translationService: TranslationService,
    private snackBar: MatSnackBar,
    private router: Router,
    private authService: AuthService
  ) {
    this.userInfo.role = this.authService.getUserRole() || 'Creator';
  }

  saveTranslations(): void {
    this.isSaving = true;
    this.translationService.saveTranslations().subscribe({
      next: () => {
        this.isSaving = false;
        this.snackBar.open('Translations saved successfully!', 'Close', {
          duration: 3000,
        });
      },
      error: () => {
        this.isSaving = false;
        this.snackBar.open('Error saving translations', 'Close', {
          duration: 3000,
        });
      },
    });
  }

  isAdmin(): boolean {
    return this.userInfo.role === 'Admin';
  }

  goToAddLanguage(): void {
    this.router.navigate(['/admin/add-language']);
  }

  logout(): void {
    // Clear authentication tokens
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');

    this.snackBar.open('You have been logged out', 'Close', {
      duration: 3000,
    });

    // Navigate back to login page
    setTimeout(() => {
      this.router.navigate(['/']);
    }, 1000);
  }
}
