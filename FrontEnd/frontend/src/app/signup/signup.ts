import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';

@Component({
  standalone: true,
  imports: [FormsModule],
  selector: 'app-signup',
  templateUrl: './signup.html'
})
export class SignupComponent {

  form = {
    employeeId: '',
    firstName: '',
    lastName: '',
    password: '',
    preferredLanguage: ''
  };

  constructor(private http: HttpClient) {}

  onSignup() {
    this.http.post('https://localhost:7199/api/auth/register', this.form)
      .subscribe({
        next: (res) => {
          console.log('Signup success', res);
          alert('User registered successfully');
          Swal.fire({
              icon: 'success',
              title: 'Signup Successful',
              text: 'Welcome!',
              timer: 2000,
              showConfirmButton: false
            });
        },
        error: (err) => {
          console.error(err);
          alert(err.error);
        }
      });
  }
}