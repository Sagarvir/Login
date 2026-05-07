import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class NoAuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    const token = this.authService.getAccessToken();

    // If user is authenticated, redirect away from login/signup
    if (token) {
      const role = this.authService.getUserRole();
      if (role?.toLowerCase() === 'admin') {
        this.router.navigate(['/admin']);
      } else {
        this.router.navigate(['/dashboard']);
      }
      return false;
    }

    // Not authenticated, allow access to login/signup
    return true;
  }
}