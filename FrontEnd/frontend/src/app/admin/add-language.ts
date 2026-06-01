import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-add-language',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './add-language.html',
  styleUrls: ['./add-language.css']
})
export class AddLanguageComponent {

  languageCode = '';
  languageName = '';
  isLoading = false;

  constructor(private http: HttpClient, private router: Router) {}

  addLanguage() {
    if (!this.languageCode.trim() || !this.languageName.trim()) {
      Swal.fire({
        icon: 'warning',
        title: 'Incomplete details',
        text: 'Please fill in both language code and language name.',
      });
      return;
    }

    this.isLoading = true;

    this.http.post('https://localhost:7199/api/Language', {
      id: 0,
      code: this.languageCode,
      name: this.languageName
    }).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        Swal.fire({
          icon: 'success',
          title: 'Language added',
          text: `Language '${this.languageName}' added successfully.`,
          timer: 1800,
          showConfirmButton: false,
        });
        this.languageCode = '';
        this.languageName = '';
      },
      error: (err) => {
        this.isLoading = false;
        const message = err.error?.message || err.message || 'Unable to add language.';
        Swal.fire({
          icon: 'error',
          title: 'Failed to add language',
          text: message,
        });
      }
    });
  }
}
