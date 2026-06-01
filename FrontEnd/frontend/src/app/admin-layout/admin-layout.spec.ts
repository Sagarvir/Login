import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminLayoutComponent } from './admin-layout';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { AuthService } from '../core/services/auth.service';
import { RouterTestingModule } from '@angular/router/testing';

describe('AdminLayoutComponent', () => {
  let component: AdminLayoutComponent;
  let fixture: ComponentFixture<AdminLayoutComponent>;
  let authServiceMock: any;
  let router: Router;

  beforeEach(async () => {
    authServiceMock = {
      getUserRole: vi.fn(),
      logout: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [AdminLayoutComponent, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceMock }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  function createComponentWithRole(role: string | null) {
    authServiceMock.getUserRole.mockReturnValue(role);

    fixture = TestBed.createComponent(AdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('should create', () => {
    createComponentWithRole('admin');
    expect(component).toBeTruthy();
  });

  it('should set userId = 1 for admin', () => {
    createComponentWithRole('admin');
    expect(component.userInfo.userId).toBe('1');
  });

  it('should set userId = 2 for translator', () => {
    createComponentWithRole('translator');
    expect(component.userInfo.userId).toBe('2');
  });

  it('should set userId = 3 for creator', () => {
    createComponentWithRole('creator');
    expect(component.userInfo.userId).toBe('3');
  });

  it('should set userId = 4 for viewer', () => {
    createComponentWithRole('viewer');
    expect(component.userInfo.userId).toBe('4');
  });

  it('should set default userId = 0 for unknown role', () => {
    createComponentWithRole('unknown');
    expect(component.userInfo.userId).toBe('0');
  });

  it('should default role to Admin if null', () => {
    createComponentWithRole(null);
    expect(component.userInfo.role).toBe('Admin');
    expect(component.userInfo.userId).toBe('0');
  });

  it('should trim and lowercase role', () => {
    createComponentWithRole('  ADMIN  ');
    expect(component.userInfo.role).toBe('admin');
    expect(component.userInfo.userId).toBe('1');
  });

  it('should call logout and navigate on logout()', () => {
    createComponentWithRole('admin');

    component.logout();

    expect(authServiceMock.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });
});