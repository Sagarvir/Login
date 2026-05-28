import { Component, OnInit } from '@angular/core';
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
  styleUrls: ['./admin-layout.css']
})
export class AdminLayoutComponent implements OnInit {
  userInfo = {
    userId: '',
    language: 'EN',
    role: 'Admin',
  };

  constructor(private authService: AuthService, private router: Router) {
    const role = this.authService
  .getUserRole()
  ?.trim()
  .toLowerCase();

this.userInfo.role = role || 'Admin';

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

  ngOnInit(): void {}

  logout() {
    this.authService.logout();   // clear token
    this.router.navigate(['/']); // go to login
  }
}