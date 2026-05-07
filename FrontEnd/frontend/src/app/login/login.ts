import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import Swal from 'sweetalert2';
import { AuthService } from '../core/services/auth.service';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  form: FormGroup;
  isLoading = signal(false);
  error = signal<string | null>(null);

  constructor() {
    this.form = this.fb.group({
      employeeId: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  onLogin() {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.error.set(null);

    const { employeeId, password } = this.form.value;

    this.authService.login({ employeeId, password })
      .subscribe({
        next: () => {
          console.log('Login success');

          Swal.fire({
            icon: 'success',
            title: 'Login Successful',
            text: 'Welcome back!',
            timer: 1500,
            showConfirmButton: false
          });

          this.isLoading.set(false);

          // ✅ You can choose where to go:
          // OPTION 1 (recommended): dashboard first
         const role = this.authService.getUserRole();

if (role?.toLowerCase() === 'admin') {
  this.router.navigate(['/admin/assign-role']);
} else {
  this.router.navigate(['/dashboard']);
}
          // OPTION 2 (if testing admin):
          // this.router.navigate(['/admin']);
        },
        error: (err) => {
          console.error('Login failed:', err);
          const errorMessage = err.error?.message || err.message || `HTTP ${err.status}: ${err.statusText}`;
          this.error.set(`Login failed: ${errorMessage}`);
          this.isLoading.set(false);
        },
      });
  }
}