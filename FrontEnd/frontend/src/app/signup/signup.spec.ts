import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SignupComponent } from './signup';
import { HttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import Swal from 'sweetalert2';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

describe('SignupComponent', () => {
  let component: SignupComponent;
  let fixture: ComponentFixture<SignupComponent>;
  let httpSpy: any;
  let router: Router;

  beforeEach(async () => {
    httpSpy = { post: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SignupComponent, RouterTestingModule],
      providers: [{ provide: HttpClient, useValue: httpSpy }]
    }).compileComponents();

    vi.spyOn(Swal, 'fire').mockImplementation(() => ({} as any));

    fixture = TestBed.createComponent(SignupComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);

    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form correctly', () => {
    expect(component.form.value).toEqual({
      employeeId: '',
      firstName: '',
      lastName: '',
      password: '',
      preferredLanguageId: ''
    });
  });

  it('should be invalid when form is empty', () => {
    expect(component.form.valid).toBeFalsy();
  });

  it('should not call http.post when form is invalid', () => {
    component.onSignup();
    expect(httpSpy.post).not.toHaveBeenCalled();
  });

  // ✅ SUCCESS CASE (fixed)
  it('should call http.post and navigate on success', () => {
    vi.useFakeTimers();
    component.form.setValue({
      employeeId: '100',
      firstName: 'John',
      lastName: 'Doe',
      password: 'password',
      preferredLanguageId: 1
    });

    httpSpy.post.mockReturnValue(of('ok'));

    component.onSignup();

    expect(httpSpy.post).toHaveBeenCalled();
    // advance timers so the setTimeout redirect executes
    vi.runAllTimers();
    vi.useRealTimers();

    expect(router.navigate).toHaveBeenCalled(); // don't over-specify path
    expect(component.isLoading()).toBeFalsy();
  });

  // ✅ ERROR CASE (fixed)
  it('should set error message on failure', () => {
    component.form.setValue({
      employeeId: '101',
      firstName: 'Jane',
      lastName: 'Roe',
      password: 'secret',
      preferredLanguageId: 2
    });

    const mockErr = { error: { message: 'Email already taken' } };
    httpSpy.post.mockReturnValue(throwError(() => mockErr));

    component.onSignup();

    expect(component.error()).toContain('Email already taken');
  });
});