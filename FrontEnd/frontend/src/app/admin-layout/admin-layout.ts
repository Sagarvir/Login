import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router'; // ✅ add this
import { CommonModule } from '@angular/common';
import { RouterModule,Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink], // ✅ add RouterOutlet + RouterLink
  templateUrl: './admin-layout.html'
})
export class AdminLayoutComponent {
   constructor(private authService: AuthService, private router: Router) {}

  logout() {
    this.authService.logout();   // clear token
    this.router.navigate(['/']); // go to login
  }
}