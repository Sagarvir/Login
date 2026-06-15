import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import Swal from 'sweetalert2';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true, // ✅ important if you're importing in tests
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
      employeeId: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  onLogin() {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.error.set(null);

    const { employeeId, password } = this.form.value;

    this.authService.login({ employeeId, password }).subscribe({
      next: () => {
        this.authService.loadUserProfile(true).subscribe((profile) => {
          this.isLoading.set(false);

          const userName =
            profile?.userName ||
            this.authService.getUsername() ||
            `Employee ${employeeId}`;


          Swal.fire({
            icon: 'success',
            title: 'Login Successful',
            text: `Welcome back, ${userName}!`,
            background: '#ffffff',
            color: '#0f4c81',
            iconColor: '#f28c28',
            showClass: {
              popup: 'swal2-show'
            },
            hideClass: {
              popup: 'swal2-hide'
            },
            timer: 1500,
            showConfirmButton: false,
            timerProgressBar: true,
            customClass: {
              popup: 'login-success-swal-popup',
              title: 'login-success-swal-title',
              htmlContainer: 'login-success-swal-content'
            }
          });

          this.router.navigate(['/dashboard']);
        });
      },
      error: (err) => {
        const errorMessage =
          err?.error?.message ||
          err?.message ||
          `HTTP ${err?.status}: ${err?.statusText}`;

        this.error.set(`Login failed: ${errorMessage}`);
        this.isLoading.set(false);
      }
    });
  }
}