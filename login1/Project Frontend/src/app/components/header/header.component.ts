import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslationService } from '../../services/translation.service';

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
    private snackBar: MatSnackBar
  ) {}

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

  logout(): void {
    console.log('Logout clicked');
    this.snackBar.open('You have been logged out', 'Close', {
      duration: 3000,
    });
  }
}
