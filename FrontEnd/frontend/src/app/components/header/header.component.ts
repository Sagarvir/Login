import { Component,OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
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
    MatTooltipModule,
    MatSnackBarModule,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit {
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
  ) 
  
  {
    this.userInfo.role = this.authService.getUserRole() || 'Creator';
  }
  ngOnInit(): void {
    this.translationService.saveCompleted$.subscribe(() => {
    this.isSaving = false;
  });
  }

 saveTranslations(): void {
  if (this.isSaving) return;

  this.isSaving = true;
  this.translationService.requestSave();
}

publishTranslations(): void {
  this.translationService.publishTranslations().subscribe({
    next: (res: any) => {
      console.log('Publish successful', res);

      this.snackBar.open(
        `Published ${res.fileCount} files successfully`,
        'Close',
        {
          duration: 3000
        }
      );
    },

    error: (error) => {
      console.error('Publish failed', error);

      this.snackBar.open(
        'Publish failed',
        'Close',
        {
          duration: 3000
        }
      );
    }
  });
}

publishCurrentLanguage(): void {

  const languageCode =
      this.translationService
          .getSelectedLanguage();

  this.translationService
      .publishLanguage(
          languageCode
      )
      .subscribe({

          next: (res:any) => {

              this.snackBar.open(
                  `${languageCode} published successfully`,
                  'Close',
                  {
                      duration:3000
                  }
              );
          },

          error: () => {

              this.snackBar.open(
                  'Publish failed',
                  'Close',
                  {
                      duration:3000
                  }
              );
          }
      });
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
