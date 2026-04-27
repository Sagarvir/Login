import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  selector: 'app-signup',
  templateUrl: './signup.html',
  styleUrls: ['./signup.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupComponent {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);

  form: FormGroup;
  isLoading = signal(false);
  error = signal<string | null>(null);

  constructor() {
    this.form = this.fb.group({
      employeeId: ['', [Validators.required]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      preferredLanguage: [''],
    });
  }

  onSignup() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    const body = { ...this.form.value };
    if (!body.preferredLanguage) {
      delete body.preferredLanguage;
    }

    this.http.post('https://localhost:7199/api/auth/register', body, { responseType: 'text' })
      .subscribe({
        next: (res) => {
          console.log('Signup success', res);
          Swal.fire({
            icon: 'success',
            title: 'Signup Successful',
            text: 'Welcome!',
            timer: 2000,
            showConfirmButton: false
          });
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Signup failed:', err);
          let errorMessage = 'Signup failed. Please check your data.';
          if (err.error?.message) {
            errorMessage = err.error.message;
          } else if (err.error && typeof err.error === 'string') {
            errorMessage = err.error;
          } else if (err.message) {
            errorMessage = err.message;
          }
          this.error.set(errorMessage);
          this.isLoading.set(false);
        },
      });
  }
}