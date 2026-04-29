import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthRoleGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {

    // ✅ 1. Check login
    const token = this.authService.getAccessToken();
    if (!token) {
      this.router.navigate(['']);
      return false;
    }

    // ✅ 2. Check role (if defined)
    const expectedRoles = route.data['roles'] as string[];
    if (expectedRoles && expectedRoles.length > 0) {
      const userRole = this.authService.getUserRole();

      if (
  !userRole ||
  !expectedRoles.some(r => r.toLowerCase() === userRole.toLowerCase())
) {
        this.router.navigate(['']);
        return false;
      }
    }

    return true;
  }
}