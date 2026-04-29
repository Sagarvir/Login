import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { Inject, PLATFORM_ID } from '@angular/core';
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private accessTokenKey = 'accessToken';
  private refreshTokenKey = 'refreshToken';
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  private tokenTimer: any;
constructor(private http: HttpClient, @Inject(PLATFORM_ID) private platformId: Object) {}

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
  assignRole(data: { employeeId: string; role: string }): Observable<any> {
  return this.http.post('https://localhost:7199/api/admin/assign-role', data);
}
 logout() {
  const refreshToken = this.getRefreshToken();

  if (refreshToken) {
    this.http.post('/api/auth/revoke', { refreshToken }).subscribe();
  }

  if (this.tokenTimer) {
    clearTimeout(this.tokenTimer);
  }

  if (isPlatformBrowser(this.platformId)) {
    localStorage.clear(); // 🔥 wrapped
  }

  this.isAuthenticatedSubject.next(false);
}

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

  isAuthenticated(): Observable<boolean> {
    return this.isAuthenticatedSubject.asObservable();
  }

  private hasToken(): boolean {
    return !!this.getAccessToken();
  }

  startTokenTimer() {
  const token = this.getAccessToken();
  if (!token) return;

  const payload = JSON.parse(atob(token.split('.')[1]));
  const expiry = payload.exp * 1000;

  const timeout = expiry - Date.now() - 60000;

  if (this.tokenTimer) {
    clearTimeout(this.tokenTimer);
  }

  this.tokenTimer = setTimeout(() => {
    this.refreshToken().subscribe();
  }, timeout);
}
getUserRole(): string | null {
    
  const token = this.getAccessToken();
  if (!token) return null;

  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log(payload);
  return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
}
}