import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MainLayout } from './main-layout';
import { AuthService } from '../core/services/auth.service';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { vi } from 'vitest';

describe('MainLayout', () => {
  let component: MainLayout;
  let fixture: ComponentFixture<MainLayout>;
  let authServiceMock: any;
  let router: Router;

  beforeEach(async () => {
    authServiceMock = {
      getUserRole: vi.fn(),
      logout: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [MainLayout, RouterTestingModule],
      providers: [{ provide: AuthService, useValue: authServiceMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayout);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ✅ isAdmin
  it('should return true if role is admin', () => {
    authServiceMock.getUserRole.mockReturnValue('admin');
    expect(component.isAdmin()).toBe(true);
  });

  it('should return false if role is not admin', () => {
    authServiceMock.getUserRole.mockReturnValue('viewer');
    expect(component.isAdmin()).toBe(false);
  });

  // ✅ getRole
  it('should return role from auth service', () => {
    authServiceMock.getUserRole.mockReturnValue('translator');
    expect(component.getRole()).toBe('translator');
  });

  it('should return default role if null', () => {
    authServiceMock.getUserRole.mockReturnValue(null);
    expect(component.getRole()).toBe('User');
  });

  // ✅ logout
  it('should call logout and navigate', () => {
    component.logout();

    expect(authServiceMock.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  // ✅ getUserId
  it('should return correct userId for admin', () => {
    authServiceMock.getUserRole.mockReturnValue('admin');
    expect(component.getUserId()).toBe(1);
  });

  it('should return correct userId for translator', () => {
    authServiceMock.getUserRole.mockReturnValue('translator');
    expect(component.getUserId()).toBe(2);
  });

  it('should return correct userId for creator', () => {
    authServiceMock.getUserRole.mockReturnValue('creator');
    expect(component.getUserId()).toBe(3);
  });

  it('should return correct userId for viewer', () => {
    authServiceMock.getUserRole.mockReturnValue('viewer');
    expect(component.getUserId()).toBe(4);
  });

  it('should return 0 for unknown role', () => {
    authServiceMock.getUserRole.mockReturnValue('unknown');
    expect(component.getUserId()).toBe(0);
  });

  it('should handle trimmed and uppercase roles', () => {
    authServiceMock.getUserRole.mockReturnValue('  ADMIN ');
    expect(component.getUserId()).toBe(1);
  });
});