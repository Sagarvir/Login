import { Component } from '@angular/core';
import { AuthService } from '../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule
  ],
  templateUrl: './main-layout.html',
  styleUrls: ['./main-layout.css'],
})
export class MainLayout {
  constructor(private authService: AuthService, private router: Router) {}

  isAdmin(): boolean {
    return this.authService.getUserRole()?.toLowerCase() === 'admin';
  }

  getRole(): string {
  return this.authService.getUserRole() || 'User';
}

  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }
  getUserId(): number {
  const role = this.authService.getUserRole()?.trim()?.toLowerCase();

  switch (role) {
    case 'admin':
      return 1;

    case 'translator':
      return 2;

    case 'creator':
      return 3;

    case 'viewer':
      return 4;

    default:
      return 0;
  }
}
}
