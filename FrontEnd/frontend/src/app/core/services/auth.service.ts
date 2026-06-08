import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private accessTokenKey = 'accessToken';
  private refreshTokenKey = 'refreshToken';
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  private tokenTimer: any;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  // ================= LOGIN =================
  login(credentials: { employeeId: string; password: string }): Observable<any> {
    return this.http.post('https://localhost:7199/api/auth/login', credentials).pipe(
      tap((res: any) => {
        this.setAccessToken(res.accessToken);
        this.setRefreshToken(res.refreshToken);
        this.isAuthenticatedSubject.next(true);
        this.startTokenTimer();
      })
    );
  }

  // ================= REFRESH =================
  refreshToken(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    return this.http.post('https://localhost:7199/api/auth/refresh', { refreshToken }).pipe(
      tap((res: any) => {
        this.setAccessToken(res.accessToken);
        this.setRefreshToken(res.refreshToken);
        this.isAuthenticatedSubject.next(true);
        this.startTokenTimer();
      })
    );
  }

  // ================= ROLE ASSIGN =================
  assignRole(data: { employeeId: string; roleName: string }): Observable<any> {
    return this.http.put('https://localhost:7199/api/auth/assign-role', data);
  }

  // ================= LOGOUT =================
  logout() {
    const refreshToken = this.getRefreshToken();

    if (refreshToken) {
      this.http.post('/api/auth/revoke', { refreshToken }).subscribe();
    }

    if (this.tokenTimer) {
      clearTimeout(this.tokenTimer);
    }

    if (isPlatformBrowser(this.platformId)) {
      localStorage.clear();
    }

    this.isAuthenticatedSubject.next(false);
  }

  // ================= TOKEN STORAGE =================
  getAccessToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.accessTokenKey);
    }
    return null;
  }

  setAccessToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.accessTokenKey, token);
    }
  }

  getRefreshToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.refreshTokenKey);
    }
    return null;
  }

  setRefreshToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.refreshTokenKey, token);
    }
  }

  // ================= AUTH STATE =================
  isAuthenticated(): Observable<boolean> {
    return this.isAuthenticatedSubject.asObservable();
  }

  private hasToken(): boolean {
    return !!this.getAccessToken();
  }

  // ================= TOKEN TIMER =================
  startTokenTimer() {
    const token = this.getAccessToken();
    if (!token) return;

    const payload = this.decodeToken();
    if (!payload) return;

    const expiry = payload.exp * 1000;
    const timeout = expiry - Date.now() - 60000;

    if (this.tokenTimer) {
      clearTimeout(this.tokenTimer);
    }

    this.tokenTimer = setTimeout(() => {
      this.refreshToken().subscribe();
    }, timeout);
  }

  // ================= TOKEN DECODER =================
  private decodeToken(): any | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload;
    } catch (e) {
      console.error('Invalid token', e);
      return null;
    }
  }

  // ================= ROLE METHODS =================

  // Normalize backend role → frontend friendly
  private normalizeRole(role: string | null): string | null {
    if (!role) return null;

    const normalized = role.trim().toLowerCase();
    switch (normalized) {
      case 'admin':
        return 'admin';
      case 'creator':
        return 'creator';
      case 'translator':
        return 'translator';
      case 'viewer':
        return 'viewer';
      default:
        return normalized;
    }
  }

  getUserRole(): string | null {
    const payload = this.decodeToken();
    if (!payload) return null;

    const rawRole =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      null;

    return this.normalizeRole(rawRole);
  }

  // ===== Specific helpers =====

  isAdmin(): boolean {
    return this.getUserRole() === 'admin';
  }

  isCreator(): boolean {
    const role = this.getUserRole();
    return role === 'creator'
  }

  isTranslator(): boolean {
    const role = this.getUserRole();
    return role === 'translator';
  }
  isViewer(): boolean {
  return this.getUserRole() === 'viewer';
}
  // ===== Generic helper =====

  hasRole(role: string): boolean {
    return this.getUserRole() === role.toLowerCase();
  }

  // ================= USER HELPERS =================

  getUserId(): string | null {
    return this.decodeToken()?.sub || null;
  }

  getUsername(): string | null {
    return this.decodeToken()?.username || null;
  }
}