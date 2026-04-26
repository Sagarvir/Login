import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import Swal from 'sweetalert2';
interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private http = inject(HttpClient);
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

    const body = this.form.value;

this.http.post<LoginResponse>('https://localhost:7199/api/auth/login', body)
      .subscribe({
        next: (res) => {
          localStorage.setItem('accessToken', res.accessToken);
          localStorage.setItem('refreshToken', res.refreshToken);
          console.log('Login success');
          // Show success alert using SweetAlert2
          // dont change this while changing UI
           Swal.fire({
    icon: 'success',
    title: 'Login Successful',
    text: 'Welcome back!',
    timer: 2000,
    showConfirmButton: false
  });//TILL HERE 
          this.isLoading.set(false);
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
