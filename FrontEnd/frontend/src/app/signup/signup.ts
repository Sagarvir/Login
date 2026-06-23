import { Component, ChangeDetectionStrategy, inject, signal , OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router'; // imported route for navigation
import Swal from 'sweetalert2';
import { LanguageService } from '../services/language.service';
@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  selector: 'app-signup',
  templateUrl: './signup.html',
  styleUrls: ['./signup.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupComponent implements OnInit {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  private router = inject(Router); // added route injection

  form: FormGroup;
  isLoading = signal(false);
  error = signal<string | null>(null);
  
  private languageService = inject(LanguageService);

languages = signal<any[]>([]);
  ngOnInit(): void {
  this.languageService.getLanguages().subscribe({
    next: (languages) => {
       console.log('SIGNUP LANGUAGES:', languages);
      this.languages.set(languages);
    },
    error: (err) => {
      console.error('Failed to load languages:', err);
      this.error.set('Unable to load languages.');
    }
  });
}

  constructor() {
    this.form = this.fb.group({
      employeeId: ['', [Validators.required]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      preferredLanguageId: ['', [Validators.required]],
    });
  }

  onSignup() {
    console.log(this.form.value);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    const body = { ...this.form.value };

    this.http.post('https://localhost:7199/api/auth/register', body, { responseType: 'text' })
      .subscribe({
        next: (res) => {
          console.log('Signup success', res);

          Swal.fire({
            icon: 'success',
            title: 'Signup Successful',
            text: 'Please log in with your credentials',
            background: '#ffffff',
            color: '#173a5c',
            iconColor: '#ff6b35',
            timer: 2000,
            showConfirmButton: false,
            timerProgressBar: true,
            customClass: {
              popup: 'eurofins-swal-popup',
              title: 'eurofins-swal-title',
              htmlContainer: 'eurofins-swal-content'
            }
          });

          this.isLoading.set(false);

          // redirect to login
          setTimeout(() => {
            this.router.navigate(['']);
          }, 2000);
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