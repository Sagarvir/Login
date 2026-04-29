import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router'; // added route imports
import Swal from 'sweetalert2';

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
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  private router = inject(Router); // added route injection

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

    const body = this.form.value;

    this.http.post<LoginResponse>('https://localhost:7199/api/auth/login', body)
      .subscribe({
        next: (res) => {
          localStorage.setItem('accessToken', res.accessToken);
          localStorage.setItem('refreshToken', res.refreshToken);

          console.log('Login success');

          //  KEEP THIS AS YOU SAID
          Swal.fire({
            icon: 'success',
            title: 'Login Successful',
            text: 'Welcome back!',
            timer: 1500,
            showConfirmButton: false
          });

          this.isLoading.set(false);

          //  REDIRECT TO DASHBOARD
          setTimeout(() => {
            this.router.navigate(['/dashboard']);
          }, 1500);
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