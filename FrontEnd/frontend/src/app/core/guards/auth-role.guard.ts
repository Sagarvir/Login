import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthRoleGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {

  const token = this.authService.getAccessToken();
  if (!token) {
    this.router.navigate(['']);
    return false;
  }

  // ✅ FIX: get roles from parent if not present
  let expectedRoles = route.data['roles'] as string[];

  if (!expectedRoles && route.parent) {
    expectedRoles = route.parent.data['roles'];
  }

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