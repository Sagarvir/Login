import { Component } from '@angular/core';
import { AuthService } from '../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
@Component({
  selector: 'app-main-layout',
  imports: [CommonModule, RouterModule],
  templateUrl: './main-layout.html',
  styleUrls: ['./main-layout.css'],
})
export class MainLayout {
  constructor(private authService: AuthService,private router: Router) {}

  isAdmin(): boolean {
  return this.authService.getUserRole()?.toLowerCase() === 'admin';
}
logout() {
  this.authService.logout();
  this.router.navigate(['/']);
}
}
