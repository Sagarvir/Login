import { Component } from '@angular/core';
import { AuthService } from '../core/services/auth.service';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [FormsModule], // 🔥 ADD THIS
  templateUrl: './admin.html'
})
export class AdminComponent {

  employeeId = '';
  role = '';

  constructor(private authService: AuthService) {}

  assignRole() {
    this.authService.assignRole({
      employeeId: this.employeeId,
      role: this.role
    }).subscribe({
      next: () => alert('Role assigned successfully'),
      error: () => alert('Error assigning role')
    });
  }
}