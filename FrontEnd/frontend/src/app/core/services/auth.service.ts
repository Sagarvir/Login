import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';

interface UserProfile {
  employeeId?: string;
  userName?: string;
  preferredLanguage?: string;
  role?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private accessTokenKey = 'accessToken';
  private refreshTokenKey = 'refreshToken';
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  private userProfileSubject = new BehaviorSubject<UserProfile | null>(null);
  userProfile$ = this.userProfileSubject.asObservable();
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

        const profile: UserProfile = {
          employeeId:
            res.employeeId ||
            res.employee_id ||
            res.empId ||
            this.getEmployeeId() ||
            undefined,
          role: this.getUserRole() || undefined,
        };

        if (profile.employeeId) {
          this.userProfileSubject.next({
            ...this.userProfileSubject.value,
            ...profile,
          });
        }
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

    this.userProfileSubject.next(null);
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
  private normalizeRole(role: any): string | null {
  if (!role) return null;

  if (Array.isArray(role)) {
    role = role[0]; // take first role
  }

  if (typeof role !== 'string') {
    return null;
  }

  return role.trim().toLowerCase();
}

  getUserRole(): string | null {
  const payload = this.decodeToken();
  if (!payload) return null;

  const rawRole =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
    null;

  console.log('RAW ROLE:', rawRole);
  console.log('TYPE:', typeof rawRole);

  return this.normalizeRole(rawRole as any);
}
  getPreferredLanguage(): string {
    const profile = this.userProfileSubject.value;
    if (profile?.preferredLanguage) {
      return profile.preferredLanguage;
    }

    const payload = this.decodeToken();

    if (!payload) {
      return 'en';
    }

    return payload.preferred_language || 'en';
  }

  private fetchUserProfileByEmployeeId(employeeId: string): Observable<any> {
    const userNameUrl = `https://localhost:7199/api/User/${encodeURIComponent(employeeId)}/user-name`;
    console.log('Fetching user profile from:', userNameUrl);

    return this.http.get(userNameUrl, { responseType: 'text' }).pipe(
      map((responseText: string) => {
        const trimmed = responseText?.trim();
        if (!trimmed) {
          return null;
        }

        if ((trimmed.startsWith('{') && trimmed.endsWith('}')) || (trimmed.startsWith('[') && trimmed.endsWith(']'))) {
          try {
            return JSON.parse(trimmed);
          } catch {
            return trimmed;
          }
        }

        if (trimmed.startsWith('"') && trimmed.endsWith('"')) {
          return trimmed.slice(1, -1);
        }

        return trimmed;
      })
    );
  }

  loadUserProfile(forceReload = false): Observable<UserProfile | null> {
    const cachedProfile = this.userProfileSubject.value;
    const employeeId = this.getEmployeeId();
    console.log('loadUserProfile - employeeId:', employeeId, 'cachedProfile:', cachedProfile, 'forceReload:', forceReload);

    if (!employeeId) {
      console.log('No employeeId available');
      return of(null);
    }

    const shouldUseCache =
      !forceReload &&
      cachedProfile?.userName &&
      cachedProfile.employeeId === employeeId;

    if (shouldUseCache) {
      console.log('Returning cached profile with username for current employeeId:', cachedProfile);
      return of(cachedProfile);
    }

    if (!forceReload && cachedProfile && cachedProfile.employeeId !== employeeId) {
      console.log('Cached profile employeeId differs from current token; refreshing profile', {
        cachedEmployeeId: cachedProfile.employeeId,
        currentEmployeeId: employeeId,
      });
    }

    return this.fetchUserProfileByEmployeeId(employeeId!).pipe(
      map((response) => {
        console.log('User profile endpoint response:', response);
        const rawName =
          typeof response === 'string'
            ? response
            : response?.userName ||
              response?.username ||
              response?.name ||
              response?.fullName ||
              response?.displayName ||
              response?.employeeName ||
              response?.employeeFullName ||
              null;

        console.log('Extracted userName:', rawName);

        const profile: UserProfile = {
          employeeId,
          userName: rawName || null,
          preferredLanguage:
            typeof response === 'object'
              ? response?.preferredLanguage || response?.preferred_language || response?.language || null
              : null,
          role: this.getUserRole() || undefined,
        };

        console.log('Setting profile:', profile);
        this.userProfileSubject.next(profile);
        return profile;
      }),
      catchError((error) => {
        console.error('Failed to load user profile from endpoint:', error);
        return of(null);
      })
    );
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

  getEmployeeId(): string | null {
    const profile = this.userProfileSubject.value;
    if (profile?.employeeId) {
      return profile.employeeId;
    }

    const payload = this.decodeToken();
    if (!payload) return null;

    return (
      payload.employeeId ||
      payload.employee_id ||
      payload.empId ||
      payload.sub ||
      payload.id ||
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      null
    );
  }

  getUsername(): string | null {
    return this.userProfileSubject.value?.userName || null;
  }
}
