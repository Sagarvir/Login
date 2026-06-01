import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { RouterTestingModule } from '@angular/router/testing';

// Vitest/JSDOM in CI may not implement matchMedia which SweetAlert2 uses.
// Provide a simple mock so SweetAlert2 doesn't throw during tests.
beforeAll(() => {
  (window as any).matchMedia = (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false
  });
});


describe('LoginComponent', () => {
let component: LoginComponent;
let fixture: ComponentFixture<LoginComponent>;
let authServiceSpy: any;
let router: Router;

beforeEach(async () => {
authServiceSpy = {
login: vi.fn(),
getUserRole: vi.fn()
};

await TestBed.configureTestingModule({
  imports: [LoginComponent, RouterTestingModule],
  providers: [
    { provide: AuthService, useValue: authServiceSpy }
  ]
}).compileComponents();

fixture = TestBed.createComponent(LoginComponent);
component = fixture.componentInstance;
router = TestBed.inject(Router);

// Prevent real navigation attempts during tests which would trigger route recognition
vi.spyOn(router, 'navigate').mockImplementation(() => Promise.resolve(true));

fixture.detectChanges();

});

it('should create', () => {
expect(component).toBeTruthy();
});

it('should initialize form with empty fields', () => {
expect(component.form.value).toEqual({
employeeId: '',
password: ''
});
});

it('should be invalid when form is empty', () => {
expect(component.form.valid).toBeFalsy();
});

it('should not call login if form is invalid', () => {
component.onLogin();
expect(authServiceSpy.login).not.toHaveBeenCalled();
});

it('should call authService.login when form is valid', () => {
component.form.setValue({
employeeId: '123',
password: 'pass'
});

authServiceSpy.login.mockReturnValue(of({}));
authServiceSpy.getUserRole.mockReturnValue('user');

component.onLogin();

expect(authServiceSpy.login).toHaveBeenCalledWith({
  employeeId: '123',
  password: 'pass'
});

});

it('should handle login error and set error message', async () => {
component.form.setValue({
employeeId: '123',
password: 'wrong'
});

const mockError = {
  error: { message: 'Invalid credentials' }
};

authServiceSpy.login.mockReturnValue(throwError(() => mockError));

component.onLogin();
await fixture.whenStable();

expect(component.error()).toContain('Invalid credentials');

});
});
