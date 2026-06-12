import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatIconModule, MatTooltipModule],
  templateUrl: './admin-layout.html',
  styleUrls: ['./admin-layout.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent implements OnInit {
  userInfo = {
    userId: '',
    employeeId: 'Unknown',
    userName: 'User',
    language: 'EN',
    role: 'Admin',
  };

  constructor(public authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {
    const role = this.authService
  .getUserRole()
  ?.trim()
  .toLowerCase();

    this.userInfo.role = role || 'Admin';
    this.userInfo.userName = this.authService.getUsername() || 'User';
    this.userInfo.employeeId =
      this.authService.getEmployeeId() || this.authService.getUserId() || 'Unknown';

    this.userInfo.language =
      this.authService.getPreferredLanguage().toUpperCase();

    switch (role) {
  case 'admin':
    this.userInfo.userId = '1';
    break;

  case 'translator':
    this.userInfo.userId = '2';
    break;

  case 'creator':
    this.userInfo.userId = '3';
    break;

  case 'viewer':
    this.userInfo.userId = '4';
    break;

  default:
    this.userInfo.userId = '0';
}
  }

  ngOnInit(): void {
    console.log('AdminLayout ngOnInit - Starting profile load');
    console.log('EmployeeId from token:', this.authService.getEmployeeId());
    
    this.authService.loadUserProfile().subscribe({
      next: (profile) => {
        console.log('Profile loaded:', profile);
        if (profile) {
          this.userInfo.userName = profile.userName || this.userInfo.userName;
          this.userInfo.employeeId = profile.employeeId || this.userInfo.employeeId;
          this.userInfo.language = profile.preferredLanguage?.toUpperCase() || this.userInfo.language;
        } else {
          this.userInfo.language = this.authService.getPreferredLanguage().toUpperCase();
        }
        console.log('UserInfo updated:', this.userInfo);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load user profile:', err);
        this.userInfo.language = this.authService.getPreferredLanguage().toUpperCase();
        this.cdr.detectChanges();
      }
    });
  }

  logout() {
    this.authService.logout();   // clear token
    this.router.navigate(['/']); // go to login
  }
}